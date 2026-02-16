# MAnimSystem 帧同步架构修正工作总结

## 概述

基于对帧同步架构的正确理解，对 MAnimSystem 和 SkillEditor 进行了重大修正。核心变化是将动画驱动模式从"手动驱动"改为"始终自动驱动"，明确了运行时和编辑器预览的不同处理方式。

---

## 完成的工作

### 1. ISkillContext 增加 IsPreviewMode 属性

**文件**: `Assets/SkillEditor/Runtime/System/ISkillContext.cs`

- 新增 `IsPreviewMode` 属性
- 用于区分编辑器预览模式和运行时模式

```csharp
public interface ISkillContext
{
    GameObject Owner { get; }
    bool IsPreviewMode { get; }  // 新增
    T GetService<T>() where T : class;
}
```

### 2. ClipContext 实现 IsPreviewMode

**文件**: `Assets/SkillEditor/Runtime/System/SkillRunner.cs`

- ClipContext 类实现 `IsPreviewMode` 属性
- `EvaluateAt()` 方法设置 `IsPreviewMode = true`（编辑器预览入口）
- `ManualUpdate()` 和 `Tick()` 方法设置 `IsPreviewMode = false`（运行时入口）
- `NotifyServicesUpdate()` 调用 `SetSpeed()` 替代 `ManualUpdate()`

### 3. AnimationClipProcessor 内部判断模式

**文件**: `Assets/SkillEditor/Runtime/Logic/Processors/AnimationClipProcessor.cs`

- `OnUpdate()` 方法内部判断 `IsPreviewMode`
- 运行时直接返回，不做采样
- 仅编辑器预览模式执行 `Evaluate()`

```csharp
public override void OnUpdate(ISkillContext context, float progress)
{
    if (!context.IsPreviewMode) return;  // 运行时不采样
    // 编辑器预览：手动采样动画帧
    animService.Evaluate(time);
}
```

### 4. AnimComponent 移除 UpdateMode

**文件**: `Assets/GameClient/MAnimSystem/AnimComponent.cs`

- 移除 `UpdateMode` 枚举
- 移除 `updateMode` 字段
- `Update()` 方法始终执行 `UpdateInternal()`
- 新增 `SetSpeed(float speedScale)` 方法用于速度控制

### 5. AnimLayer 增加 SetSpeed 方法

**文件**: `Assets/GameClient/MAnimSystem/AnimLayer.cs`

- 新增 `SetSpeed(float speed)` 方法
- 设置当前动画的播放速度

### 6. IAnimationService 接口更新

**文件**: `Assets/SkillEditor/Runtime/Services/IServices.cs`

- `ManualUpdate(float deltaTime)` → `SetSpeed(float speedScale)`
- 明确语义：速度控制而非驱动更新
- 添加详细的 XML 注释说明各方法的用途

### 7. 创建 MAnimAnimationService 适配器

**文件**: `Assets/GameClient/MAnimSystem/MAnimAnimationService.cs` (新建)

- 实现 `IAnimationService` 接口
- 将 SkillEditor 的动画调用转发到 AnimComponent
- 包含详细的 XML 注释说明设计意图

### 8. RuntimeAnimationService 更新

**文件**: `Assets/SkillEditor/Runtime/Services/RuntimeAnimationService.cs`

- 实现新的 `IAnimationService` 接口
- `Evaluate()` 方法留空（运行时不需要）
- `SetSpeed()` 设置 Animator 的播放速度

---

## 架构对比

### 修改前（错误理解）

```
AnimComponent
├── UpdateMode: Auto / Manual
├── Auto:   Update() 驱动
└── Manual: ManualUpdate(dt) 驱动

IAnimationService
├── Play(clip, duration)
├── Evaluate(time)      ← 运行时和编辑器都需要
└── ManualUpdate(dt)    ← 驱动动画更新
```

### 修改后（正确理解）

```
AnimComponent
├── 始终由 Unity Update 自动驱动
├── Play(clip)      → 播放动画
├── SetSpeed(scale) → 速度控制
└── Evaluate(time)  → 编辑器预览采样

IAnimationService
├── Play(clip, duration)   ← 运行时和编辑器都需要
├── Evaluate(time)         ← 仅编辑器预览
└── SetSpeed(speedScale)   ← 速度控制
```

---

## 运行时帧同步流程

```
┌─────────────────────────────────────────────────────────────────┐
│                     运行时模式 (Runtime)                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  网络层收到指令 { skillId, frame }                                │
│       ↓                                                         │
│  SkillRunner.ManualUpdate(fixedDt)                              │
│       ↓                                                         │
│  _context.IsPreviewMode = false                                 │
│       ↓                                                         │
│  Tick(fixedDt) 推进固定步长                                      │
│       ↓                                                         │
│  OnEnter → Play(clip)           ← 只发控制命令                   │
│  OnUpdate → 直接返回             ← 不做采样                      │
│  OnTick → 逻辑判定（伤害判定等）                                  │
│  OnExit → 切换状态/返回待机                                       │
│       ↓                                                         │
│  AnimComponent 由 Unity Update 自动驱动                          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                     编辑器模式 (Editor Preview)                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  SkillRunner.EvaluateAt(time)  ← 拖拽时间轴                      │
│       ↓                                                         │
│  _context.IsPreviewMode = true                                  │
│       ↓                                                         │
│  OnEnter → Play(clip)                                           │
│  OnUpdate → Evaluate(time)  ← 手动采样动画帧                     │
│  OnExit → 停止/清理                                              │
│                                                                 │
│  特点：时间可跳跃，需要手动采样                                    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 测试结果

| 测试项 | 结果 |
|--------|------|
| ISkillContext.IsPreviewMode | ✅ 接口定义正确 |
| ClipContext 实现 | ✅ 属性正确设置 |
| AnimationClipProcessor 判断 | ✅ 运行时不采样 |
| AnimComponent 驱动 | ✅ 始终 MonoUpdate |
| SetSpeed 功能 | ✅ 速度控制正常 |
| IAnimationService 接口 | ✅ 语义明确 |
| MAnimAnimationService 适配器 | ✅ 转发正确 |
| RuntimeAnimationService | ✅ 接口实现完整 |

---

## 文件变更清单

| 文件 | 变更类型 | 行数变化 |
|------|----------|----------|
| ISkillContext.cs | 修改 | +7 行 |
| SkillRunner.cs | 修改 | +15 行 |
| AnimationClipProcessor.cs | 重写 | +20 行 |
| AnimComponent.cs | 重写 | -30 行 |
| AnimLayer.cs | 修改 | +13 行 |
| IServices.cs | 修改 | +15 行 |
| MAnimAnimationService.cs | 新建 | +75 行 |
| RuntimeAnimationService.cs | 重写 | +25 行 |

---

## 相关文档

- [帧同步架构修正方案](./MAnimSystem_FrameSync_Refactor_Plan.md)
- [SkillEditor 集成计划](./MAnimSystem_SkillEditor_Integration_Plan.md)

---

## 注意事项

1. **AnimComponent 始终自动驱动**
   - 不再需要手动调用 `ManualUpdate()`
   - 动画由 Unity Update 自动更新

2. **Evaluate 仅用于编辑器预览**
   - 运行时请勿调用 `Evaluate()`
   - 仅在编辑器拖拽时间轴时使用

3. **SetSpeed 用于速度控制**
   - 用于帧同步场景下的速度调整
   - 不影响动画驱动方式

4. **IsPreviewMode 判断**
   - 编辑器预览时为 `true`
   - 运行时为 `false`
   - Processor 可据此决定是否采样

---

**修正日期**: 2026-02-14
