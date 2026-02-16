# 技能编辑器优化方向与方案

基于当前架构和代码分析，以下是针对技能编辑器的潜在优化方向。

## 1. 精度与时间控制优化 (Precision & Time Control)

**目标**: 在保持灵活性的同时，支持严格的游戏帧同步需求（如格斗/动作游戏）。

### 方案 A: 引入“帧吸附模式” (Frame Snapping Mode)
*   **描述**: 在工具栏增加一个下拉菜单 [ 自由模式 | 30 FPS | 60 FPS ]。
*   **实现**:
    *   在 `SkillEditorState` 中增加 `float frameRate` 和 `bool useFrameSnap`。
    *   修改 `TimelineView.SnapTime` 方法：当开启帧吸附时，无视动态网格，强制 `return Mathf.Round(time * frameRate) / frameRate;`。
    *   修改渲染逻辑，在标尺上绘制固定的帧刻度线（Frame 0, 1, 2...）而非秒数。

### 方案 B: 逻辑层改用帧数存储 (Integer Frames)
*   **描述**: 将数据层的 `float startTime` 改为 `int startFrame`。
*   **优点**: 彻底根除浮点误差，保证绝对确定性。
*   **代价**: 重构成本高，需要大量修改 `ClipBase` 及所有序列化数据。
*   **建议**: 除非是新立项的硬核格斗游戏，否则不推荐重构现有项目，建议采用 **方案 A**（UI层限制）。

---

## 2. 交互体验优化 (UX Improvements)

**目标**: 提升操作手感，减少误操作，提高制作效率。

### 2.1 缩放体验优化
*   **当前**: 缩放中心似乎是固定的或跟随左侧。
*   **优化**: 
    1.  **鼠标中心缩放**: `Ctrl + 滚轮` 时，保持鼠标指向的时间点在屏幕上的位置不变。
    2.  **快捷键**: 引入 `F` 键 (Focus)，自动缩放视图以容纳当前选中的片段或整个 Timeline。

### 2.2 框选优化
*   **当前**: 也就是基础的点选。
*   **优化**:
    *   **框选 (Marquee Selection)**: 实现鼠标拖拽画框，批量选中多个片段。
    *   **多选拖拽**: 允许同时拖动多个选中的片段，并保持它们之间的相对位置（已在代码中看见 `DragMode.MoveClip` 对 `draggingClip` 的处理，需确认是否支持 `state.selectedClips` 的批量更新）。

### 2.3 磁吸功能增强
*   **优化**: 增加磁吸开关按钮。
    *   **Magnet Toggle**: 允许用户暂时关闭自动吸附（按住 Ctrl 临时关闭）。
    *   **Snap Target**: 允许用户独立勾选“吸附到片段”、“吸附到光标”、“吸附到网格”。

---

## 3. 性能优化 (Performance)

**目标**: 当 Timeline 包含数百个片段或几十条轨道时保持流畅。

### 3.1 渲染剔除 (UI Culling)
*   **现状**: `TimelineView` 目前虽然有简单的 `continue` 判断，但遍历逻辑可能在大量 Clip 时仍有开销。
*   **优化**: 
    *   **视锥剔除**: 仅计算和绘制位于 `scrollOffset` 和 `scrollOffset + viewWidth` 之间的片段。
    *   **轨道剔除**: 仅绘制 Y 轴在屏幕可见范围内的轨道。

### 3.2 绘制优化
*   **优化**: 减少 `GUI.Label` 和 `EditorGUI.DrawRect` 的调用。对于密集的刻度线，可以考虑使用 `GL` 底层绘制或一张平铺的 Texture，大幅降低 DrawCall (Editor GUI 也是有开销的)。

---

## 4. 架构与稳定性 (Architecture & Reliability)

**目标**: 提高系统的健壮性，防止数据损坏。

### 4.1 撤销系统 (Undo/Redo) 加固
*   **现状**: 依赖 `Undo.RegisterCompleteObjectUndo`。
*   **优化**: 对于拖拽等连续操作，应使用 `Undo.RecordObject` 并配合 `Undo.CollapseUndoOperations` (Group)，避免每移动一像素产生一个撤销步。

### 4.2 脏标记管理 (Dirty Flag)
*   **优化**: 确保所有修改操作（包括右键菜单删除、快捷键复制粘贴）都正确调用 `EditorUtility.SetDirty`，防止修改丢失。目前代码中已较好地处理了这一点，需保持。

### 4.3 异常处理与日志
*   **优化**: 在 `SkillRunner.EvaluateAt` 和 `Processor` 中增加 `try-catch` 块。如果是用户编写的脚本（如自定义特效逻辑）报错，不应导致整个编辑器崩溃或预览卡死。

---

## 5. 功能扩展 (Features)

### 5.1 曲线编辑器集成
*   **方案**: 某些特效属性（如透明度渐变、位移曲线）需要曲线控制。可以集成 Unity 内置的 `AnimationCurve` 绘制接口，在 Clip 下方扩展出曲线编辑区。

### 5.2 嵌套 Timeline (Sub-Timeline)
*   **方案**: 允许一个 Clip 引用另一个 `SkillTimeline` 资源，实现技能的模块化复用（例如通用的受击动作序列）。

---

## 推荐实施路线图

1.  **P0 (高优)**: 实施 **1. 方案 A (帧吸附模式)**。这能直接回应你对“步长/精度”的关注，对操作手感提升最明显。
3.  **P2**: 随着特效变复杂，实施 **3.1 渲染剔除**。

---

## 6. 逻辑一致性与帧同步 (Logic Consistency & Frame Sync)

**目标**: 解决 Unity Update 不稳定导致的逻辑穿透问题，实现严格的“帧同步”逻辑更新。

### 6.1 运行时更新机制升级 (SkillRunner Upgrade)
*   **当前**: `CurrentTime += deltaTime` (Simple Accumulator)。
*   **优化**: 引入蓄水池算法 (Accumulator / Fixed Step)。
    *   **Logic Step**: 定义固定的 `LOGIC_STEP` (如 0.033f)。
    *   **Accumulator**: 在 `ManualUpdate(dt)` 中累加 `dt` 到 `accumulator`。
    *   **While Loop**: `while (accumulator >= LOGIC_STEP)` 执行 `TickProcessors`，每次只推进 `LOGIC_STEP` 时间。
    *   **Interpolation**: (可选) 使用 `accumulator / LOGIC_STEP` 作为 alpha 值，对可视层对象（如模型位置）进行插值，消除视觉抖动。

### 6.2 处理器逻辑重构 (Processor Refactor)
*   **当前**: 判断 `if (Start <= Current && End > Current)`。
*   **优化**: 
    *   改为**区间扫描判定**。
    *   `TickProcessors` 传入 `(prevTime, currentTime)`。
    *   判断 `if (ClipEnd > prevTime && ClipStart <= currentTime)`。
    *   这样即使一次 Update 跨越了多个 Logic Step，也能精确捕捉到夹在中间的短片段。

### 6.3 推荐实施步骤
1.  **Refactor SkillRunner**: 修改 `ManualUpdate` 为蓄水池模式。
2.  **Define UpdateMode**: 在 `SkillRunner` 中增加枚举 `UpdateMode { Free, FrameLocked }`，允许编辑器预览保持 Free 模式，游戏运行使用 FrameLocked 模式。
