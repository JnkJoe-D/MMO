# 技能编辑器播放架构 (Skill Editor Playback Architecture)

本文档描述了当前技能编辑器的运行时播放架构，包括核心循环、时间管理、以及 Editor 与 Runtime 的交互方式。

## 核心组件 (Core Components)

| 组件 | 职责 | 关键类 |
| :--- | :--- | :--- |
| **SkillRunner** | 运行时核心驱动器，负责时间管理、Clip 生命周期调度 | `SkillRunner.cs` |
| **SkillEditorWindow** | 编辑器播放控制，负责预览模型的生命周期管理与驱动 | `SkillEditorWindow.cs` |
| **ClipProcessor** | 具体逻辑处理器，执行每个 Clip 的具体行为 (动画/特效/伤害等) | `BaseClipProcessor.cs` |
| **Services** | 外部系统抽象层，解耦具体功能实现 (如 Animator, VFX) | `IServices.cs` |

## 1. 播放驱动循环 (Playback Loop)

播放逻辑由 `SkillRunner` 统一驱动，支持两种更新模式：

### 1.1 Update Loop (`ManualUpdate`)

`SkillRunner.ManualUpdate(float deltaTime)` 是唯一的驱动入口。

- **Auto Mode**: 在 Runtime 游戏运行时，由 `Update()` 自动调用 `ManualUpdate(Time.deltaTime)`.
- **Manual Mode**: 在 Editor 预览时，由 `SkillEditorWindow.Update()` 计算 `EditorApplication.timeSinceStartup` 的差值后手动调用。

### 1.2 时间步进策略 (Time Step Strategy)

`SkillRunner` 内部实现了蓄水池 (Accumulator) 算法，支持两种步进模式 (`TimeStepMode`)：

1.  **Variable (自由模式)**:
    - 直接透传 `deltaTime` 给 `Tick()`。
    - 适用于常规游戏逻辑，平滑但非确定性。

2.  **Fixed (固定/帧锁定模式)**:
    - 使用 `Accumulator` 累积时间。
    - 按照固定的 `fixedDt = 1.0 / FrameRate` 进行切片。
    - 循环调用 `Tick(fixedDt)` 直到消耗完累积时间。
    - **用途**: 确保逻辑执行的确定性 (Determinism)，模拟服务器帧同步环境。

## 2. 逻辑调度 (Logic Dispatch)

### 2.1 初始化 (`Initialize`)
- 清空当前状态。
- 遍历 `activeClips`，为每个 Clip 创建对应的 `Processor` 实例。
- 维护 `ProcessorState` (Clip + Processor + IsRunning)。

### 2.2 Tick 核心 (`Tick`)
`Tick(float dt)` 负责推进时间并更新所有 Processor 的状态。它采用**区间扫描 (Interval Scanning)** 算法：

1.  **时间推移**: `CurrentTime` -> `NextTime (Current + dt)`。
2.  **区间判定**: 遍历所有 Clip，检查 `[Start, End)` 与当前时间片 `[PrevTime, NextTime]` 是否有交集。
3.  **生命周期事件**:
    - **OnEnter**: Clip 刚进入时间片 (之前未运行 && 现在重叠)。
    - **OnUpdate**: 持续重叠。传递 `progress (0~1)`。
    - **OnTick**: 持续重叠。传递 `localTime` 和 `prevLocalTime` (用于逻辑帧判定)。
    - **OnExit**: Clip 结束或不再重叠。

### 2.3 预览跳转 (`EvaluateAt`)
用于编辑器下拖拽时间轴时的“瞬移”预览：
- **不触发 Tick**: 避免触发中间过程的副作用 (如伤害判定)。
- **强制覆盖时间**: 直接设置 `CurrentTime = targetTime`。
- **状态采样**: 调用 `Processor.OnSample(time)` (通常回退到 `OnUpdate`) 以刷新画面表现 (如动画姿态)。

## 3. 服务层交互 (Service Interaction)

为了解耦 Runtime 与 Editor，SkillRunner 通过 `ClipContext` 提供服务定位器：

- **Context**: 每个 `Processor` 接收一个 `ISkillContext`。
- **Registration**: 外部 (如 EditorWindow) 注册具体服务 (如 `EditorAnimationService`) 到 Context。
- **Usage**: Processor 通过 `context.GetService<IAnimationService>()` 获取服务。

### 关键服务
- `IAnimationService`: 封装 `Animator` 操作 (`Play`, `Evaluate`, `ManualUpdate`).
- `IVFXService`: 封装粒子特效生成与生命周期管理.

## 4. 编辑器预览流程 (Editor Preview Flow)

1.  **EnsurePreviewRunner**:
    - 检查预览模型 (Preview Model)。
    - 挂载或获取 `SkillRunner` 组件。
    - 注入服务 (`EditorAnimationService`, `RuntimeVFXService`)。
    - 注入当前 Timeline 数据 (`activeClips`)。

2.  **Update**:
    - 计算 `dt`。
    - 调用 `_previewRunner.ManualUpdate(dt)`。
    - 同步 UI 进度条 (`state.timeIndicator`).
    - 处理循环播放逻辑.

3.  **Timeline Drag**:
    - 触发 `OnPreviewTimeChanged`.
    - 调用 `_previewRunner.EvaluateAt(time)`.

## 类图关系 (Simple Class Diagram)

```mermaid
graph TD
    Window[SkillEditorWindow] -->|Drives| Runner[SkillRunner]
    Window -->|Injects| Service[Services (Anim/VFX)]
    Runner -->|Updates| Processor[BaseClipProcessor]
    Runner -->|Uses| Context[ClipContext]
    Processor -->|Calls| Context
    Context -->|Locates| Service
    
    subgraph "Runtime Core"
    Runner
    Processor
    Context
    end
```
