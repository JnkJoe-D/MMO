---
description: 基于 MVC 框架创建 GameClient UI 模块及微件的标准指南
---

# 🎮 游戏 UI 开发指南 (UI Workflow)

本指南规范了在 `GameClient` 中基于标准的 UGUI + MVC 表现层框架，开发 **独立架构面板 (Panel)** 与 **内置复用微件 (Widget/Item)** 的标准作业流程。

---

## 🏗️ 1. 基本架构认知

框架中所有的 UI 组件和界面严格受分为两大类，绝不混淆：

- **第一类：独立面板 (MVC Panel)**  
  受 `UIManager` 全权管理（拥有独占或叠层渲染能力）。不论它是全屏的主城界面、弹出的登录小框，还是全局系统的 `MessageBox` 通知，**只要它被 UIManager 调用 Open / Close 管理，它就必须是一套完整的 MVC 三件套**（`UIModule`, `UIModel`, `UIView`）。

- **第二类：嵌套微件 (UIWidget)**  
  完全**不受** `UIManager` 管理。它们用于构成面板内部的复杂列表元素，例如背包里成百上千的**独立道具格子** (ItemView)、排行榜的一行玩家数据、或者是属性面板栏的某一块动态区域。它们只有呈现数据的能力，所有网络逻辑和交互控制都要抛回给它的宿主 (Panel Module)。

---

## 🛠️ 2. 创建独立面板 (Panel) - MVC 工作流
*适用场景：主界面、登录背景、注册窗口、全局通知弹窗(MessageBox) 等。*

// turbo-all

1. **创建脚本目录**
   在 `Assets/GameClient/UI/Modules/` 下新建功能文件夹（如 `LevelSelect`）。

2. **编写 Model**
   创建 `LevelSelectModel.cs`，继承自 `UIModel`。声明公共属性用于存储纯数值状态。
   ```csharp
   using Game.UI;
   namespace Game.UI.Modules.LevelSelect {
       public class LevelSelectModel : UIModel {
           public int CurrentSelectedLevelId { get; set; } = 1;
       }
   }
   ```

3. **创建预制体与自动生成 View**
   - 在 Unity 中制作 `LevelSelectPanel.prefab`。根节点必须挂载 `Canvas`、`GraphicRaycaster` 以及默认附加上的 `UIView`。
   - 打开自定义工具 `Tools > UI > Auto Bind Window`，选中该 Prefab 并自动生成带节点引用绑定代码的 `LevelSelectView.cs`。

4. **编写核心控制枢纽 Module**
   创建 `LevelSelectModule.cs`。通过 `[UIPanel]` 特性声明层级 (Layer) 和所用 prefab 的读取路径（它决定了该面板在多层覆盖时的视觉遮挡表现：如 Window 层被 Dialog 层遮挡）。
   ```csharp
   using Game.UI;
   using UnityEngine;

   namespace Game.UI.Modules.LevelSelect {
       // 特性指明了层级，如普通窗口为 UILayer.Window，警告弹窗为 UILayer.Dialog
       [UIPanel(ViewPrefab = "Assets/Resources/.../LevelSelectPanel.prefab", Layer = UILayer.Window)]
       public class LevelSelectModule : UIModule<LevelSelectView, LevelSelectModel> {
           protected override void OnCreate() {
               base.OnCreate();
               // 这里进行 View 按钮委托绑定、网络发包注册，并将 Model 数据推给 View 显示
           }
           protected override void OnRemove() {
               base.OnRemove();
               // 切记注销事件！
           }
       }
   }
   ```
   **调用方式**：通过 `UIManager.Instance.Open<LevelSelectModule>();` 打开，关闭调用 `Close`。

---

## 🧩 3. 创建嵌套微件 (Widget / Item) 工作流
*适用场景：滚动列表格子、背包道具、血条 UI、可循环复用的图文块组合。*

1. **不需要独立的目录和 Model/Module**
   一般将这部分代码存放在业务 `Panel` 自身的目录下，或者作为公共的 Common 组件。

2. **继承 UIWidget 基类并声明组件**
   不走 MVC，只直接继承 `UIWidget` 基类并挂载在对应的 Prefab 根部。使用手动拖拽连线或内部的寻址来获得引用。
   ```csharp
   using Game.UI;
   using UnityEngine;
   using UnityEngine.UI;
   using TMPro;

   namespace Game.UI.Modules.Inventory {
       public class InventoryItemWidget : UIWidget {
           [SerializeField] private Image _iconBg;
           [SerializeField] private TMP_Text _countText;
           
           // 交互抛出，不要在这里发包
           public System.Action<int> OnItemClick;

           // 从宿主 Panel 接收纯粹的展示数据
           public void SetData(int itemId, int count, Sprite iconSprite) {
               _countText.text = count.ToString();
               _iconBg.sprite = iconSprite;
           }

           public void Start() {
               GetComponent<Button>().onClick.AddListener(() => OnItemClick?.Invoke(123)); // 示例抛出
           }
       }
   }
   ```

3. **由宿主(Panel Module)进行管理调度**
   `UIManager` 对此类微件一无所知。创建和销毁、填数据的工作全权由掌控外部 `Panel` 列表框的那个宿主 `Module` 或 `View` 来控制。宿主可以借助 Unity 官方的对象池、或者简单地 Instantiate 数组去生成它们。
