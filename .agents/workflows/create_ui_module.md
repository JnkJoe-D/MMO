---
description: 基于 MVC 框架创建 GameClient UI 模块及微件的标准指南
---

# 🎮 游戏 UI 模块开发指南 (UI Workflow)

本指南规范了在 `GameClient` 中基于一套标准的 UGUI + MVC 表现层框架，开发 **普通界面 (Panel)**、**通用弹窗 (Widget)** 和 **内嵌元件 (Element/Item)** 的标准作业流程。

---

## 🏗️ 1. 基本架构认知

所有 UI 均按职责划分为三部分 (MVC)：
- **[Model]**: `UIModel` 衍生类，纯数据容器，**不持有任何 Unity 相关引用**。用于面板的数据驱动防丢失以及状态记录。
- **[View]**: `UIView` / `UIWidget` / `UIItem` 衍生类。职责是绑定预制体 (`Prefab`) 上的所有组件并初始化默认视图与表现，**不包含任何业务逻辑**。
- **[Module] (Controller)**: `UIModuleBase` 衍生类，持有 View 和 Model。负责：
  - 发送/接收网络 `Event` 与 `TCP/UDP` 包
  - 注册 View 里的按钮事件并更新 Model 数据
  - 执行 `UIManager.Instance.Open / Close` 等流转调用

> 💡 **核心原则**: Module 可以随意抛弃，只要 Model 在，UI 数据状态就永远都在。

---

## 🛠️ 2. 全屏/窗口面板 (Panel Module) 流程
适用场景：主界面、登录背景、注册窗口、背包界面等。

// turbo-all

1. **创建脚本目录**
   在 `Assets/GameClient/UI/Modules/` 下新建功能文件夹，如 `LevelSelect`。

2. **编写 Model**
   创建 `LevelSelectModel.cs`，继承自 `UIModel`。声明公共属性用于存储所需数据。
   ```csharp
   using Game.UI;
   namespace Game.UI.Modules.LevelSelect {
       public class LevelSelectModel : UIModel {
           public int CurrentSelectedLevelId { get; set; } = 1;
           // ... 其他数据
       }
   }
   ```

3. **创建预制体与自动绑定 View**
   - 在 Unity 中制作界面 `LevelSelectPanel.prefab`。根节点**必须**挂载 `Canvas`、`GraphicRaycaster` 等标准 UI 组件。
   - 使用自定义工具 `Tools > UI > Auto Bind Window`。选中 Prefab 并生成 `LevelSelectView.cs`，保存至脚本目录。

4. **编写 Module**
   创建 `LevelSelectModule.cs`，并添加 `[UIPanel]` 特性声明它所在的层级 (Layer) 和关联预制体。
   ```csharp
   using Game.UI;
   using UnityEngine;

   namespace Game.UI.Modules.LevelSelect {
       [UIPanel(ViewPrefab = "Assets/.../LevelSelectPanel.prefab", Layer = UILayer.Window)]
       public class LevelSelectModule : UIModule<LevelSelectView, LevelSelectModel> {
           protected override void OnCreate() {
               base.OnCreate();
               // 1. 订阅网络和本地事件
               // 2. 将 View.xxxBtn 与自身业务逻辑作绑定
               // 3. 将 Model 数据同步给 View
           }

           protected override void OnRemove() {
               base.OnRemove();
               // ! 务必解除按钮点击和事件订阅
           }
       }
   }
   ```
   **调用方式**：`UIManager.Instance.Open<LevelSelectModule>();`

---

## 📦 3. 全局微件弹窗 (Widget Module) 流程
适用场景：系统通知框 MessageBox、跑马灯、断线重连浮窗。

与 Panel 完全一致，但区别在于特性与用途：

- **特性声明**：确保其 `Layer = UILayer.Dialog` 或更高（如 `Top`）。
- **传递参数**：Widget 在被 `Open` 时通常需要立刻呈现不同数据，此时**直接构建数据对象传入 `Open` 参数**，利用基类机制在打开瞬间初始化 Model。

**调用示例**：
```csharp
UIManager.Instance.Open<MessageBoxModule>(new MessageBoxModel {
    Title = "错误",
    Content = "网络已断开"
});
```
Widget 打开极快，适用于随用随弹的全局共享节点。

---

## 🧩 4. 无 Canvas 嵌入式元件 (Element / Item) 流程
（*注：当前框架暂未大规模集成，属前瞻性规范*）

适用场景：背包里的一个个道具格子 (ItemView)、排行榜里的一条一条玩家数据 (RankItemView)。

它们**不参与** `UIManager` 的导航栈和层级调度，因此不需要层级 `Canvas`。它们是被高层 **Panel** 动态实例化在 `ScrollRect` 或 `LayoutGroup` 里的部件。

### 开发规范：
1. **轻量化基类**：创建或继承类似于 `UIItemBase<TData>` 的轻量脚本（直接挂在 Item 预制件上）。
2. **无需 Module**：Item 没有复杂的系统业务交互能力。直接在 `UIItem` 里对外暴露一个 `SetData(Model)` 或 `Refresh()` 方法。
3. **由 Panel 控制**：宿主 `Panel Module` 负责向后台请求包含 100 个道具的数组数据，然后在一个 Loop 循环里基于 `Item Prefab` 出生 100 个节点，并调用它们的 `SetData()`。点击某个格子时的判定，可以通过事件抛回给宿主的 `Module`。
