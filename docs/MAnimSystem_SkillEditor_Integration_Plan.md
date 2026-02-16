# MAnimSystem 接入 SkillEditor 实现计划

## 一、任务概述

将 MAnimSystem 扩展以支持 SkillEditor 的 `IAnimationService` 接口，实现动画服务集成。

**重要更新 (2026-02-14)**：基于帧同步架构的正确理解，对原有设计进行了重大修正。详见 [帧同步架构修正方案](./MAnimSystem_FrameSync_Refactor_Plan.md)。

---

## 二、任务清单

### 任务 1: ISkillContext 增加 IsPreviewMode 属性

**文件**: `Assets/SkillEditor/Runtime/System/ISkillContext.cs`

**状态**: 待执行

**修改内容**:

```csharp
public interface ISkillContext
{
    GameObject Owner { get; }
    
    /// <summary>
    /// 是否为编辑器预览模式。
    /// true: 编辑器预览，需要手动采样动画帧。
    /// false: 运行时模式，动画由 Unity 自动驱动。
    /// </summary>
    bool IsPreviewMode { get; }
    
    T GetService<T>() where T : class;
}
```

---

### 任务 2: ClipContext 实现 IsPreviewMode

**文件**: `Assets/SkillEditor/Runtime/System/SkillRunner.cs`

**状态**: 待执行

**修改内容**:

1. ClipContext 类新增 `IsPreviewMode` 属性
2. `EvaluateAt` 方法设置 `IsPreviewMode = true`
3. `Tick` 方法设置 `IsPreviewMode = false`

---

### 任务 3: AnimationClipProcessor.OnUpdate 内部判断模式

**文件**: `Assets/SkillEditor/Runtime/Logic/Processors/AnimationClipProcessor.cs`

**状态**: 待执行

**修改内容**:

```csharp
public override void OnUpdate(ISkillContext context, float progress)
{
    // 运行时直接返回，不做采样
    if (!context.IsPreviewMode) return;
    
    // 以下仅编辑器预览模式执行
    var animService = context.GetService<IAnimationService>();
    var data = context.GetData<AnimationClip>();
    
    if (data != null && animService != null)
    {
        float time = data.StartTime + data.Duration * progress;
        animService.Evaluate(time);
    }
}
```

---

### 任务 4: AnimComponent 移除 UpdateMode

**文件**: `Assets/GameClient/MAnimSystem/AnimComponent.cs`

**状态**: 待执行

**修改内容**:

1. 移除 `UpdateMode` 枚举和 `updateMode` 字段
2. `Update()` 方法始终执行 `UpdateInternal()`
3. `ManualUpdate` 标记为 `[Obsolete]`，推荐使用 `SetSpeed`
4. 新增 `SetSpeed(float speedScale)` 方法

**核心改动**:

```csharp
private void Update()
{
    if (!_isGraphCreated) return;
    // 始终自动更新，由 Unity 驱动
    UpdateInternal(Time.deltaTime);
}

public void SetSpeed(float speedScale)
{
    if (!_isGraphCreated) return;
    foreach (var layer in _layers)
    {
        layer?.SetSpeed(speedScale);
    }
}
```

---

### 任务 5: AnimLayer 增加 SetSpeed 方法

**文件**: `Assets/GameClient/MAnimSystem/AnimLayer.cs`

**状态**: 待执行

**新增内容**:

```csharp
public void SetSpeed(float speed)
{
    if (_targetState != null)
    {
        _targetState.Speed = speed;
    }
}
```

---

### 任务 6: IAnimationService 接口更新

**文件**: `Assets/SkillEditor/Runtime/Services/IServices.cs`

**状态**: 待执行

**修改内容**:

将 `ManualUpdate(float deltaTime)` 改为 `SetSpeed(float speedScale)`，明确语义。

```csharp
public interface IAnimationService
{
    void Play(UnityEngine.AnimationClip clip, float transitionDuration);
    void Evaluate(float time);  // 仅编辑器预览
    void SetSpeed(float speedScale);  // 速度控制
}
```

---

### 任务 7: 创建 MAnimAnimationService 适配器

**文件**: `Assets/GameClient/MAnimSystem/MAnimAnimationService.cs` (新建)

**状态**: 待执行

**核心设计**:

- `Play`: 运行时和编辑器都需要
- `Evaluate`: 仅编辑器预览需要
- `SetSpeed`: 用于速度控制

---

### 任务 8: RuntimeAnimationService 更新

**文件**: `Assets/SkillEditor/Runtime/Services/RuntimeAnimationService.cs`

**状态**: 待执行

**修改内容**:

实现新的 `IAnimationService` 接口，`Evaluate` 方法留空（运行时不需要）。

---

## 三、文件变更清单

| 文件 | 操作 | 变更内容 |
|------|------|----------|
| `ISkillContext.cs` | 修改 | 新增 `IsPreviewMode` 属性 |
| `SkillRunner.cs` | 修改 | ClipContext 实现 IsPreviewMode |
| `AnimationClipProcessor.cs` | 修改 | OnUpdate 内部判断 IsPreviewMode |
| `AnimComponent.cs` | 修改 | 移除 UpdateMode，新增 SetSpeed |
| `AnimLayer.cs` | 修改 | 新增 SetSpeed 方法 |
| `IServices.cs` | 修改 | ManualUpdate 改为 SetSpeed |
| `MAnimAnimationService.cs` | 新建 | IAnimationService 适配器 |
| `RuntimeAnimationService.cs` | 修改 | 更新接口实现 |

---

## 四、验证计划

### 4.1 单元测试

1. **AnimComponent 驱动测试**
   - 验证 Update 始终自动执行
   - 验证 SetSpeed 正确影响播放速度

2. **IsPreviewMode 测试**
   - 编辑器预览时 IsPreviewMode = true
   - 运行时 Tick 时 IsPreviewMode = false

3. **AnimationClipProcessor 测试**
   - 预览模式：OnUpdate 调用 Evaluate
   - 运行时模式：OnUpdate 不调用 Evaluate

### 4.2 集成测试

1. **编辑器预览流程**
   - 拖拽时间轴，验证动画正确采样

2. **运行时帧同步流程**
   - 模拟网络指令，触发技能播放
   - 验证 Play 被调用，Evaluate 不被调用

---

## 五、实施顺序

```
任务 1: ISkillContext 增加 IsPreviewMode
    │
    ▼
任务 2: ClipContext 实现 IsPreviewMode
    │
    ▼
任务 3: AnimationClipProcessor 内部判断
    │
    ▼
任务 4: AnimComponent 移除 UpdateMode
    │
    ▼
任务 5: AnimLayer 增加 SetSpeed
    │
    ▼
任务 6: IAnimationService 接口更新
    │
    ▼
任务 7: MAnimAnimationService 适配器
    │
    ▼
任务 8: RuntimeAnimationService 更新
    │
    ▼
验证测试
```

---

## 六、架构对比

### 修改前（错误理解）

```
AnimComponent
├── UpdateMode: Auto / Manual
├── Auto:   Update() 驱动
└── Manual: ManualUpdate(dt) 驱动
```

### 修改后（正确理解）

```
AnimComponent
├── 始终由 Unity Update 自动驱动
├── Play(clip)      → 播放动画
├── SetSpeed(scale) → 速度控制
└── Evaluate(time)  → 编辑器预览采样
```

---

**文档日期**: 2026-02-14
**更新说明**: 基于帧同步架构正确理解，重新设计实现方案
