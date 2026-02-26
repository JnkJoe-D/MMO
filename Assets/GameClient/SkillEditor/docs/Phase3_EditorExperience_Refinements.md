# Skill Editor Phase 3: Editor Experience Refinements (Context & Gizmos)

在技能编辑器的 Phase 3 (可视化与预览体验优化) 阶段，我们积累了一系列提升框架健壮性和扩展性的关键开发经验，主要集中在 **运行时环境模拟（Context 预热）** 和 **编辑器状态驱动绘制（Inspector 变动响应）** 两个方向。

## 1. 静态 Context 的概念与预热策略 (Prewarming ProcessContext)

### 背景与问题
原本只在 `SkillRunner.Play()` 被调用后才会真正生成并注入 `ProcessContext`。这就导致编辑器在「未播放（刚启动、停止）」的状态下（即静态预览状态），底层的各种 `Drawer` (如 `DamageClipDrawer`) 无法获取到诸如 `ISkillActor` 这类需要动态解析的实体。由此出现了“没播放时不画 Gizmos”或“空引用报错”的严重缺陷。

### 解决方案
通过引入 **“运行时环境解耦预热”** 模式解决：
1. **Runner 暴露底层状态注入点**：在 `SkillRunner` 中增加 `PrewarmContext(ProcessContext initialContext)` 方法。它的职责是允许调用方在不改变状态机 (`SkillRunnerState.Stopped`) 的前提下强行塞入一个临时 Context。
2. **全局状态透传**：在全编辑器共享的 `SkillEditorState` 中增加 `PreviewContext => previewRunner?.Context` 全局 Getter。
3. **编辑器加载生命周期接管**：在 `SkillEditorWindow.InitPreview()`（即窗口启动或更换预览模型时触发的方法）内部构造一个仅供预览用的 `SkillServiceFactory` 和 `ProcessContext`，并调用 `PrewarmContext`。
4. **加载保底回退**：考虑到用户可能首次打开并没有关联预览预制体，引入了“默认预览预制体”的概念并将路径配置打通了 `EditorPrefs` 和 `SkillEditorSettingsWindow`，保证预热链路上必然存在合法的 `GameObject` 实例。

### 经验总结
在制作重度解耦（依赖倒置）的时序类编辑器时，**编辑器的“停止态”不等于原件的“销毁态”**。为了让各种基于绑定对象、骨骼乃至特效锚点定位的编辑器辅助线 (Gizmo) 正常工作，你必须为编辑器准备一套能被全局访问到的 **Dummy Context（虚拟上下文）** 并维护它的生命周期。

---

## 2. 基于事件驱动的原生 Inspector 响应机制 (OnInspectorChange & OnSceneView Repaint)

### 背景与问题
Unity 的原生 Inspector 面板（借助 `[CustomEditor]` 渲染原生界面）是技能编辑器数据修改的主阵地（如调整 `HitBoxShape` 半径、位置偏移）。
但在编辑器中调整数据时，如果没有显式触发 Scene 视图重绘的方法调用，用户在 Inspector 拖拽的数据变化就不会即时反映在 Scene 视图的辅助线上。之前经常出现由于没有焦点切换导致 Scene 没刷新的阻断感。

### 解决方案
通过 **事件总线桥接** 和 **编辑器状态脏标记 (Dirty Flag / Change Check)** 来桥联双端。
1. **拦截属性变动**：在 `SkillInspectorBase` (统一接管所有自定类的 Inspector 渲染基类) 的 `OnInspectorGUI` 中，用 `EditorGUI.BeginChangeCheck()` 和 `EditorGUI.EndChangeCheck()` 包裹所有反射出来的 GUI 渲染内容。
2. **事件总线抛出变动**：一旦捕获到改动 (`EndChangeCheck` 返回 true)，就强制触发 `events.OnInspectorChanged?.Invoke()` 并且把改动后的数据回写回 Timeline 数据层以确保 Undo 系统能拦截到。
3. **Scene 刷新订阅**：Timeline 系统作为事件核心（具体在 `SkillEditorWindow` 中），从它的 `OnEnable` 就通过 `events.OnInspectorChanged += RepaintScene` 订阅了这个事件，并在处理函数里通过 `SceneView.RepaintAll()` 进行主动重绘响应。

### 经验总结
对于“所见即所得”的自制多窗口协作型编辑器，**数据层（Model）、面板视图（Inspector/Property Editor）、场景视图（Scene/Timeline）之间绝对不能直接强耦合调用刷新逻辑**。必须使用一套纯粹的 `EventBus` (在这个架构里是 `SkillEditorEvents`) 把“用户改了数据”这一事件广播出去。

---

## 3. 2D/Cylindrical 在全空间 (3D) 底层下的物理降维

### 背景与问题
最初的实现中，碰撞盒判定大量依赖于球心判定（`OverlapSphere`）。由于 `Vector3.Angle` 和 `Vector3.Distance` 天生囊括全立体维度，结果原本想实现二维切面体验的“扇形 (Sector)”和“环形 (Ring)”变成了锥形和实心球壳，与传统动作地面游戏的做法背道而驰。

### 解决方案
利用 **正方形初筛 (OverlapBox) + 局部坐标降维过滤 (Vector2 XZ Planar Calculation + Y Height Cutoff)** 解决。
1. 将 Broad-phase 修改为宽厚的 `OverlapBox`。
2. 用 `Quaternion.Inverse * offset` 把物理碰撞点强制逆转回施法者 / 附着点的**局部空间系**。
3. 进行暴力高度裁切 `abs(local.y) < height/2`。
4. 在局部坐标系下取投影系的 `Vector2(local.x, local.z)` 重做角度计算和平面向心半径距离计算。
5. 在 Gizmo 层应用纯数学建模方法（顶面弧线、连接棱、交叉半圆球顶），通过立体点阵线辅助用户还原 3D 转 2D 柱体裁切的抽象形态。
