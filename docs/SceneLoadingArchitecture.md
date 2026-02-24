# 客户端混合场景加载架构 (Scene Transition Architecture)

> 日期：2026-02-25
> 状态：✅ 已实装

## 1. 架构背景与痛点思考

在现代商业游戏（特别是基于 YooAsset / AssetBundle 体系）的开发中，Unity 原生的 `SceneManager.LoadScene` 往往只是一个“骨架切换”。
**面临的核心问题包括：**
1. **场景体积极度真空**：为了控制首包包体与灵活热更，真正的地图 `.unity` 文件里几乎没有任何真实的网格（Mesh）、角色、怪物或者环境贴图。它只是一个挂载着环境参数（如光照贴图配置、天空盒）和各类标记点（Spawn Points）的容器。
2. **纯场景加载的错觉缺失**：如果在 `LoadSceneAsync` 等到 1.0 就关掉黑屏，玩家看到的将是一个模型没刷出来、地表还在靠流式加载缓慢蹦入的“穿模世界”。
3. **关联资源请求**：玩家登录成功后，服务器会下发该玩家穿着什么装备、带着什么坐骑，而副本还会决定刷出哪几种特定的怪物。这些重度资源，必须在此场景切换“遮罩”尚未掀开之前，全部读入。

因此，我们的加载架构不仅仅是**加载场景**，而是**加载场景 + 并发预加载强依赖资源 + 实例化与还原现场**的连续流水线：**混合加载（Loading Sequence）**。

---

## 2. 混合加载阶段解析 (四段式运转)

核心执行模块位于 `SceneManager` 中的 `ChangeSceneRoutine` 协同程序，整个加载进度划分为四个严格递进的阶段：

### Phase 1: 预留缓冲与旧资源清洗 (0% ~ 10%)
- **行为**：UI 会在这一帧利用传入的参数拉起黑色遮罩或插画弹窗。
- **清理机制**：同步调用 `GlobalPoolManager.ClearAll()`。在载入新纪元前，彻底排空那些因旧副本使用过后挂起在缓存池里的闲置对象，并断开不必要的内存链接准备垃圾回收。

### Phase 2: 场景级骨架加载 (10% ~ 40%)
- **行为**：发起对目标地图文件（如 `MainLobby.unity`）的 `YooAsset` 请求加载。
- **本质**：这一步主要是将 Unity 场景数据字典和基础节点环境调配完成。
- **UI 体现**：这其中引擎反馈进度的幅度会用插值压缩至 30% 占比左右。

### Phase 3: 并发异步强依赖加载 (40% ~ 90%)
- **核心要义**：这是整个流程最长、也是带宽与 I/O 占用最猛的一步。
- **行为**：获取外面业务传入的转场参数装载包 `SceneTransitionParams`，遍历其内置的 `RequiredAssets<string>` (玩家、武器、主角UI表)。
- **操作方式**：发起多个并发的 `ResourceManager.Instance.LoadAssetAsync<UnityEngine.Object>`。不需要返回值接管句柄，只要其进入 `YooAsset` 的资源驻留内存层中即可。
- **阻塞确认**：运用 `WaitUntil` 等待所有发出去的 Request 被 Count 完全命中归拢。

### Phase 4: 后置装配与现场还原 (90% ~ 100%)
- **行为**：内存准备完毕后主线程进入阻塞，利用 `ResourceManager.Instance.UnloadUnused()` 释放老场景没清理干净的部分纹理和材料。通知底层各个子系统初始化坐标。
- **事件抛出**：广播最终满格 `Progress=1f` 后发送 `SceneChangeEndEvent`，由上层建筑监听此事件者（如 `LoadingModule`）收走界面，将渲染权让还给场景相机。

---

## 3. 参数解耦与 UI 交互边界

### SceneTransitionParams (数据驱动)
用专门的对象包装类取代函数多参数重载：
```csharp
public class SceneTransitionParams
{
    public string SceneName { get; set; }           // 场景Id或路径
    public List<string> RequiredAssets { get; set; }// 进入场景必须预先加载的强依赖
    public bool ShowLoading { get; set; }           // 是否打开过渡 UI 板
    public object CustomData { get; set; }          // 后处理需要的上下文（如出生点）
}
```

### 为什么不由底层 (GameRoot) 全自动展示 UI？
在实际设计中，经常会踏入一个陷阱：“既然任何场景流转都需要进度条，为什么不直接让系统底层监听这个事件并强行代办？” 
**解答（SRP - 单一职责原则）：**
1. 并非所有的 `ChangeScene` 行为都需要弹插管画。如果是小场景重置、或隐藏式预加新房间，底层绑死将导致致命缺陷。
2. **逻辑可寻根性**：由业务发起调用（例如在 `LoginModule.cs` 内部点击并且得到服务器准允的下一步），直接在发起请求的代码区块里调用 `UIManager.Instance.Open<LoadingModule>()`。它清晰表明了：是谁、出于什么状态、触发了这个 Loading。
3. **自主终结机制**：而对于**关闭 Loading** 这种确定性明确的行为（必定是场景完毕后），交由 `LoadingModule` 内部订阅 `SceneChangeEndEvent` 事件并自己请求销毁，达到了高度自治与逻辑闭环，拒绝耦合在 `GameRoot` 那种根植入级别的万能单例中。

---

## 4. 总结

该架构规范了网络时代 MMORPG / ARPG 的底层通用实践模式。它提供了一个强壮骨架，可以在未来直接无缝接入诸如**热更表单读取**、**多阶段异步实例化限制**等性能控制算法，成为连接前后端业务与引擎底层的平滑粘合剂。
