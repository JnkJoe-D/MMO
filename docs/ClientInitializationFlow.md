# 游戏客户端初始化与各子系统生命周期全景流程

本文档用于详细追溯本客户端在游戏启动瞬间（`Init.unity`）到完成所有资源和配置热更就绪的全部时序和底层 API 调度。

---

## 一、宏观初始化流水线 (GameRoot Pipeline)
在玩家启动游戏后，位于 `Init` 场景中的 `[GameRoot]` 对象被 Awake，利用 `DontDestroyOnLoad` 将自身置为常驻节点，并开启核心的协程流水序列 `InitializeSequence()`。

目前的时序严格按照以下步骤推演：

1. **Step 1: 全局对象池 (Pool)**
   - 调用 `GlobalPoolManager.Initialize()`
   - 为后续所有 UI 组件和基础游戏对象的缓存提供最底层的容器。
2. **Step 2: UI 系统基建 (UIManager)**
   - 创建 `UIManager` 并调用 `_uiManager.Initialize(this)`。
   - 于场景中创建持久化的 `[UIRoot]` 节点以容纳摄像机及多重画布画布层级（Background / Window / Dialog 等）。
3. **Step 3: 唤醒首屏热更防崩溃界面 (Loading UI)**
   - 调用 `_uiManager.Open<HotUpdateModule>()`。
   - **【底层的降级加载策略】**：此时 `ResourceManager` 还未启动。`UIManager` 在判断加载路径以 `Resources/` 开头后，主动降级采用底层原生的 `Resources.LoadAsync<GameObject>` 将热更屏直接挂载。此时 UI 进入监听状态，准备接收来自资源模块的任何事件。
4. **Step 4: 资源系统及热更流程引擎 (ResourceManager & YooAsset)**
   - 初始化 `YooAssets.Initialize()` 并获取/创建包 `defaultPackageName`。
   - 如果配置处于 `HostPlayMode` 且 `autoUpdate=true`，则由 `ResourceUpdater.Run()` 接管整个控制权（下方第二节将详细展开这套流程）。
   - 【极其重要】：在此阶段，整个系统的协程是被挂起的，直至热更新完成。
5. **Step 5: 配置系统 (Luban)**
   - 调用 `ConfigManager.Instance.InitializeAsync()` 
   - 依赖上一步已经热更或就绪完毕的资源层，从 `Assets/Configs/` 中读取并反序列化 JSON，内存常驻所有游戏表格（TbXXX）。
6. **Step 6-8: 模块补全阶段 (Lua / Network / Scene)**
   - xlua, TCP/UDP 连接初始化。
7. **Step 9: 游戏就绪 (GameReady)**
   - 通过 `EventCenter` 广播 `GameInitializedEvent()`，宣告客户端环境彻底准备好，随时可以正式向登录场景或选角发力。

---

## 二、联机热更内部运作时序 (ResourceUpdater)
当流水线进行到 Step 4，且启动了 `HostPlayMode` 时，以下是针对 `YooAsset 2.x` API 具体流转的底层细节梳理：

### 阶段 1：请求版本查询 (Requesting Version)
1. 发送提示事件：`HotUpdateStatusEvent { Progress = 0.1f, StatusText = "正在检查版本更新" }`。
2. 触发 API：`_package.RequestPackageVersionAsync()` 访问 CDN。
3. **结果判定**：如果成功，记录下此时的服务端版本号，并将其推给内部的 `IRemoteServices` 以便建立诸如 `http://X.X.X.X/CDN/PC/v1.0.1/` 这种动态构建的下载路径前缀。

### 阶段 2：拉取清单差距 (Update Manifest)
1. 发送提示事件：在 `!manifestOp.IsDone` 的轮询中不断更新 `HotUpdateStatusEvent` 并把进度条强行控制在 `0.2`~`0.99` 之间。
2. 触发 API：`_package.UpdatePackageManifestAsync(packageVersion)` 
3. **底层运作**：将刚拿到版本号下的 `PackageManifest_{version}.hash` 文件拉取并校验。如果和本地记录的不同，则把整个文件拉下来建立哈希和资源对比视图。

### 阶段 3：计算并拦截 (Downloader Creation & Suspend)
1. 触发 API：`_package.CreateResourceDownloader(downloadingMaxNumber: 10, failedTryAgain: 3)`。根据 Manifest 的差异，建立一个底层的下载队列。
2. **分支判断**：
    *   **A: 文件差异为 0**
        * 发送结束事件：`HotUpdateStatusEvent` 进度拉满至 `1.0`。宣告热更完毕，跳出，GameRoot 继续访问 Step 5。
    *   **B: 文件有差异** (拦截挂起)
        * 触发 API：利用 `downloader.TotalDownloadBytes` 算出总需求流量。
        * 发送提示事件：抛出带有**动作闭包(ConfirmAction)**的回调事件 `HotUpdateRequireConfirmEvent`。
        * **UI 接管流程**：此时处于内存中等待已久的 `HotUpdateModule` 捕获到此事件，通过 `UIManager` 在最顶层弹出 `MessageBox` 并将闭包回调传入“确定”按钮。
        * `ResourceUpdater` 的主进程代码遇到 `yield return new WaitUntil(() => userConfirmed);`，完全静止冻结。

### 阶段 4：真实下载 (Downloading)
1. 玩家点击了界面上的**确认**，触发了 `WaitUntil` 的条件变量变为真。挂起苏醒。
2. 注册回调：
    *   指定 `downloader.DownloadUpdateCallback = OnDownloadProgress;`
    *   指定 `downloader.DownloadErrorCallback  = OnDownloadError;`
3. 触发 API：`downloader.BeginDownload();` 
    * 调用此句后，YooAsset 开始创建多线程，基于我们前面算出的差异清单从 CDN 文件池逐个读取 Bytes。
4.  **UI 数据更新**：
    *   `OnDownloadProgress` 回调被高频触发（其入参带来了下载量和总管量）。
    *   我们将其转换或装箱并抛为 `HotUpdateProgressEvent`，`HotUpdateModule` 根据此事件修改自己的进度条 FillAmount 和 Text 表现。
5. **结束判定**：`downloader.Status == Succeed` 时视为下载圆满结束。释放一个完成信号。

### 阶段 5：清理与就绪
当更新结束，`ResourceUpdater` 生命周期终止。此时系统抛出 `HotUpdateCompletedEvent`，UI 层收到该事件后，**自动销毁热更新与加载面板**，系统彻底变清白。接着，`GameRoot` 收拢执行流，顺利切入配置表加载等后续步骤。从始至终所有的错误和网络断线都会转入 `HotUpdateFailedEvent` 做重试闭环。

---

*这份文档即为目前《系统/UI/YooAsset》互调的最佳工业实现方案大纲。*
