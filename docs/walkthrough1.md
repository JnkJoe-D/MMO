# SimpleAnimancer 系统改进工作总结

## 概述

本次改进对 SimpleAnimancer 动画系统进行了全面优化，解决了过渡系统的核心缺陷，并提升了性能和代码质量。

---

## 完成的工作

### 1. AnimState 基类优化

**文件**: `Assets/GameClient/MAnimSystem/AnimState.cs`

- **Playable 字段语义明确化**
  - 将 `_playable` 重命名为 `_playableCache`
  - 明确注释说明：子类应维护自己的具体类型 Playable 字段作为主存储
  - 解决了基类和子类 Playable 字段冗余的困惑

- **新增时间归一化 API**
  - `NormalizedTime` 属性：获取/设置归一化播放时间 (0.0 ~ 1.0)
  - `IsPaused` 属性：获取/设置暂停状态
  - `Pause()` 方法：暂停播放
  - `Resume()` 方法：恢复播放

### 2. AnimLayer 过渡系统重构

**文件**: `Assets/GameClient/MAnimSystem/AnimLayer.cs`

- **中断列表法实现**
  - 新增 `FadingState` 结构体，追踪所有淡出状态
  - 新增 `_fadingStates` 列表，管理多状态过渡
  - 被中断的状态自动加速淡出（2倍速）
  - 权重归一化确保总和始终为 1.0

- **解决的问题**
  - A→B 过渡中切换到 C，A 权重不再卡住
  - 连续切换 A→B→C→D，所有状态正确过渡
  - 无状态丢失，无权重异常

### 3. AnimLayer 状态缓存机制

**文件**: `Assets/GameClient/MAnimSystem/AnimLayer.cs`

- **Dictionary 缓存实现**
  - 新增 `_clipStateCache` 字典，缓存 AnimationClip → ClipState 映射
  - `Play(clip)` 优先返回缓存实例
  - 缓存上限 32 个状态，超出时自动清理最久未使用的状态
  - 新增 `ClearCache()` 方法手动清除缓存

- **性能提升**
  - 避免重复创建 ClipState
  - 减少频繁 GC 分配

### 4. AnimLayer 状态清理机制

**文件**: `Assets/GameClient/MAnimSystem/AnimLayer.cs`

- **延迟清理队列实现**
  - 新增 `_pendingCleanup` 字典，追踪待清理状态
  - 淡出完成的状态标记为待清理
  - 延迟 2 秒后自动销毁（缓存状态除外）
  - 新增 `DisconnectState()` 和 `DestroyState()` 方法

- **解决的内存泄漏**
  - 旧状态不再永久驻留内存
  - 端口正确回收复用

### 5. LinearMixerState 阈值自动排序

**文件**: `Assets/GameClient/MAnimSystem/LinearMixerState.cs`

- **插入排序实现**
  - `Add(clip, threshold)` 自动按阈值排序
  - 新增 `ReorderMixerPorts()` 方法重新连接端口
  - 新增 `GetThreshold()` 方法获取阈值

- **解决的问题**
  - 用户无需手动按顺序添加
  - 插值计算始终正确

### 6. BlendTreeState2D 数组预分配优化

**文件**: `Assets/GameClient/MAnimSystem/BlendTreeState2D.cs`

- **预分配缓冲区实现**
  - 新增 `_weightBuffer` 数组，初始容量 8
  - 按需自动扩容（2倍）
  - 新增 `GetPosition()` 方法获取 2D 坐标

- **性能提升**
  - 消除每帧 `new float[count]` 的 GC 分配

### 7. MixerState 字段重命名

**文件**: `Assets/GameClient/MAnimSystem/MixerState.cs`

- 将 `_mixer` 重命名为 `_mixerPlayable`
- 与 `ClipState._clipPlayable` 命名风格一致
- 更新所有引用

### 8. 测试脚本更新

**文件**: `Assets/GameClient/MAnimSystem/SimpleAnimancerTest.cs`

- **新增测试用例**
  - 按 F：频繁切换测试（验证中断列表法）
  - 按 G：状态缓存验证
  - 按 H：归一化时间 API 测试
  - 按 P：暂停/恢复测试

- **改进演示**
  - 1D 混合器演示阈值乱序添加
  - 日志输出更详细

---

## 测试结果

| 测试项 | 结果 |
|--------|------|
| 基础播放与过渡 | ✅ 通过 |
| 事件触发 (OnEnd/OnFadeComplete) | ✅ 通过 |
| 频繁切换测试 (20次/50ms间隔) | ✅ 通过，无权重卡住 |
| 状态缓存验证 | ✅ 通过，同一 Clip 返回相同实例 |
| 归一化时间 API | ✅ 通过 |
| 暂停/恢复功能 | ✅ 通过 |
| 1D 混合器阈值排序 | ✅ 通过，乱序添加后正确插值 |
| 2D 混合器 | ✅ 通过 |

---

## 架构改进对比

### 过渡系统

```
旧实现：
┌─────────────────────────────────────┐
│  _currentState ──→ _targetState     │
│  (只追踪2个状态，中间状态丢失)        │
└─────────────────────────────────────┘

新实现：
┌─────────────────────────────────────┐
│  _fadingStates[0] ──→ 淡出中        │
│  _fadingStates[1] ──→ 淡出中(中断)   │
│  _fadingStates[2] ──→ 淡出中(中断)   │
│  _targetState     ──→ 淡入中        │
│  (追踪所有状态，无丢失)              │
└─────────────────────────────────────┘
```

### 状态生命周期

```
旧实现：
Play(clip) → 创建 ClipState → 连接 → [永久存在]

新实现：
Play(clip) → 检查缓存 → 命中：返回缓存实例
                        未命中：创建并缓存
         → 淡出完成 → 标记待清理 → 延迟2秒 → 销毁
```

---

## 注意事项

1. **缓存状态不会被清理**
   - 通过 `Play(clip)` 播放的状态会被缓存
   - 直接 `Play(state)` 播放的状态不会被缓存

2. **中断加速倍率可配置**
   - 当前设置为 2 倍速 (`INTERRUPT_SPEED_MULTIPLIER`)
   - 可根据需求调整

3. **缓存大小限制**
   - 当前上限 32 个状态 (`MAX_CACHE_SIZE`)
   - 超出时自动清理最久未使用的非当前播放状态

4. **清理延迟时间**
   - 当前设置为 2 秒 (`CLEANUP_DELAY`)
   - 可根据需求调整

---

## 后续可扩展方向

1. ~~**多层混合支持**~~ ✅ 已完成
   - 添加 `AnimationLayerMixerPlayable`
   - 支持 AvatarMask 实现上下半身分层

2. **动画事件系统**
   - 支持 AnimationClip 内嵌事件
   - 支持关键帧回调

3. ~~**动画遮罩**~~ ✅ 已完成
   - 支持 Additive 混合模式
   - 支持部分骨骼遮罩

---

## Layer 系统实现记录 (2026-02-13)

### 概述

实现了完整的多层动画混合系统，支持 AvatarMask、Additive 模式和层权重淡入淡出。

### 架构设计

```
┌───────────────────────────────────────────────────────────────┐
│                      AnimComponent                             │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │              AnimationLayerMixerPlayable                 │  │
│  │         (管理所有层的混合、Mask、Additive)                │  │
│  └─────────────────────────────────────────────────────────┘  │
│              ▲              ▲              ▲                   │
│              │              │              │                   │
│  ┌───────────┴───┐  ┌──────┴──────┐  ┌───┴───────────┐       │
│  │   Layer 0     │  │   Layer 1   │  │   Layer 2     │       │
│  │  (Base)       │  │ (UpperBody) │  │  (Effects)    │       │
│  │  Weight: 1.0  │  │  Weight: w  │  │  Weight: w    │       │
│  │  Mask: null   │  │  Mask: ...  │  │  Additive     │       │
│  │  ┌─────────┐  │  │  ┌────────┐ │  │  ┌────────┐   │       │
│  │  │ Mixer   │  │  │  │ Mixer  │ │  │  │ Mixer  │   │       │
│  │  │(状态混合)│  │  │  │        │ │  │  │        │   │       │
│  │  └─────────┘  │  │  └────────┘ │  │  └────────┘   │       │
│  └───────────────┘  └─────────────┘  └───────────────┘       │
└───────────────────────────────────────────────────────────────┘
```

### 完成的工作

#### 1. AnimLayer 层属性扩展

**文件**: `Assets/GameClient/MAnimSystem/AnimLayer.cs`

- **新增层属性**
  - `Weight`: 层权重 (0 ~ 1)
  - `Mask`: AvatarMask 骨骼遮罩
  - `IsAdditive`: 叠加模式开关

- **新增层淡入淡出**
  - `StartFade(float targetWeight, float duration)`: 层权重淡入淡出
  - `UpdateLayerFade(float deltaTime)`: 内部更新方法

- **构造函数扩展**
  - 新增 `AnimationLayerMixerPlayable` 参数
  - 自动同步初始权重到 LayerMixer

#### 2. AnimComponent 多层管理

**文件**: `Assets/GameClient/MAnimSystem/AnimComponent.cs`

- **新增字段**
  - `_layerMixer`: AnimationLayerMixerPlayable 实例
  - `_layers`: List<AnimLayer> 层列表
  - `LayerCount`: 层数量属性

- **新增索引器**
  - `this[int index]`: 懒创建层

- **新增方法**
  - `GetLayer(int index)`: 获取或创建指定层
  - `CreateLayer(int index)`: 内部创建层方法
  - `Play(clip, layerIndex, fadeDuration)`: 在指定层播放

- **图连接重构**
  ```
  旧: AnimLayer.Mixer -> AnimationPlayableOutput
  新: Layer[0].Mixer ─┐
      Layer[1].Mixer ─┼──> LayerMixer -> AnimationPlayableOutput
      Layer[2].Mixer ─┘
  ```

#### 3. 测试用例

**文件**: `Assets/GameClient/MAnimSystem/Test1.cs`

- **新增测试资源字段**
  - `upperBodyClip`: 上半身动画
  - `breatheClip`: 叠加动画
  - `upperBodyMask`: 上半身遮罩

- **新增测试按键**
  - U: 上半身层测试（带 AvatarMask）
  - I: 叠加层测试（Additive）
  - O: 层淡入淡出测试
  - L: 动态创建层测试
  - M: 多层同时播放测试

### API 使用示例

```csharp
// 基础层播放
animComponent.Play(walkClip);

// 上半身层播放（带 Mask）
var upperLayer = animComponent[1];
upperLayer.Mask = upperBodyMask;
upperLayer.Play(attackClip);

// 叠加层播放
var additiveLayer = animComponent[2];
additiveLayer.IsAdditive = true;
additiveLayer.Play(breatheClip);

// 层淡入淡出
upperLayer.StartFade(0f, 0.25f);  // 淡出
upperLayer.StartFade(1f, 0.25f);  // 淡入

// 在指定层播放
animComponent.Play(clip, layerIndex: 1, fadeDuration: 0.25f);
```

### 关键实现细节

| 功能 | Unity API |
|------|-----------|
| 创建层混合器 | `AnimationLayerMixerPlayable.Create(graph, inputCount)` |
| 设置 AvatarMask | `layerMixer.SetLayerMaskFromAvatarMask(index, mask)` |
| 设置叠加模式 | `layerMixer.SetLayerAdditive(index, true)` |
| 设置层权重 | `layerMixer.SetInputWeight(index, weight)` |

### 测试结果

| 测试项 | 结果 |
|--------|------|
| 基础层播放 | ✅ 通过 |
| 上半身层（AvatarMask） | ✅ 通过 |
| 叠加层（Additive） | ✅ 通过 |
| 层淡入淡出 | ✅ 通过 |
| 动态创建层 | ✅ 通过 |
| 多层同时播放 | ✅ 通过 |

### 文件变更清单

| 文件 | 变更类型 | 行数变化 |
|------|----------|----------|
| AnimLayer.cs | 修改 | +110 行 |
| AnimComponent.cs | 重写 | +80 行 |
| Test1.cs | 修改 | +140 行 |

---

## Bug 修复记录 (2026-02-13)

### 问题描述

快速切换动画时，有时会出现 Mixer 所有输入端口权重都为 0 的情况，导致动画"消失"。

### 根本原因

1. **归一化逻辑缺陷**：只处理 `totalWeight > 1` 的情况，未处理 `totalWeight < 1`
2. **重复加入淡出列表**：同一状态可能被多次加入 `_fadingStates`，导致权重被重复减少

### 修复内容

#### 1. 归一化逻辑修复

**文件**: `AnimLayer.cs`

```csharp
// 修复前：只处理 > 1 的情况
if (totalWeight > 1.001f && totalFadeOutWeight > 0.001f)

// 修复后：处理所有非 1 的情况
if (totalWeight < 0.001f) { /* 异常处理 */ }
if (Mathf.Abs(totalWeight - 1f) > 0.001f && totalFadeOutWeight > 0.001f)
```

#### 2. 防止重复加入淡出列表

新增 `AddToFadingStates` 方法：
- 检查状态是否已存在
- 已存在则更新速度（取较大值）
- 不存在才添加新记录

#### 3. 新增测试用例

- 按 **N** 键：权重归一化验证测试
- 30 次快速切换，每次验证权重总和是否为 1

### 文件变更

| 文件 | 变更类型 |
|------|----------|
| AnimLayer.cs | 修改（归一化逻辑、新增方法） |
| Test1.cs | 新增测试用例 |

---

## 文件变更清单

| 文件 | 变更类型 | 行数变化 |
|------|----------|----------|
| AnimState.cs | 修改 | +47 行 |
| AnimLayer.cs | 重写 | +210 行 |
| ClipState.cs | 修改 | +1 行 |
| MixerState.cs | 修改 | +5 行 |
| LinearMixerState.cs | 重写 | +60 行 |
| BlendTreeState2D.cs | 重写 | +30 行 |
| Test1.cs | 重写 | +140 行 |

---

**改进日期**: 2026-02-13

---

## 帧同步架构修正记录 (2026-02-14)

### 概述

基于对帧同步架构的正确理解，对 MAnimSystem 和 SkillEditor 进行了重大修正。核心变化是将动画驱动模式从"手动驱动"改为"始终自动驱动"。

### 问题分析

之前的错误理解：
- 运行时需要 `ManualUpdate` 驱动动画更新
- `Evaluate` 运行时和编辑器都需要

正确的理解：
- 动画始终由 Unity MonoUpdate 自动驱动
- `ManualUpdate` 应改为 `SetSpeed`，用于速度控制
- `Evaluate` 仅编辑器预览需要

### 完成的工作

#### 1. ISkillContext 增加 IsPreviewMode 属性

**文件**: `Assets/SkillEditor/Runtime/System/ISkillContext.cs`

```csharp
public interface ISkillContext
{
    GameObject Owner { get; }
    bool IsPreviewMode { get; }  // 新增
    T GetService<T>() where T : class;
}
```

#### 2. ClipContext 实现 IsPreviewMode

**文件**: `Assets/SkillEditor/Runtime/System/SkillRunner.cs`

- ClipContext 新增 `IsPreviewMode` 属性
- `EvaluateAt()` 设置 `IsPreviewMode = true`（编辑器预览）
- `ManualUpdate()` 和 `Tick()` 设置 `IsPreviewMode = false`（运行时）

#### 3. AnimationClipProcessor 内部判断模式

**文件**: `Assets/SkillEditor/Runtime/Logic/Processors/AnimationClipProcessor.cs`

```csharp
public override void OnUpdate(ISkillContext context, float progress)
{
    // 运行时直接返回，不做采样
    if (!context.IsPreviewMode) return;
    
    // 以下仅编辑器预览模式执行
    animService.Evaluate(time);
}
```

#### 4. AnimComponent 移除 UpdateMode

**文件**: `Assets/GameClient/MAnimSystem/AnimComponent.cs`

- 移除 `UpdateMode` 枚举和 `updateMode` 字段
- `Update()` 始终执行 `UpdateInternal()`
- 新增 `SetSpeed(float speedScale)` 方法

#### 5. AnimLayer 增加 SetSpeed 方法

**文件**: `Assets/GameClient/MAnimSystem/AnimLayer.cs`

```csharp
public void SetSpeed(float speed)
{
    if (_targetState != null)
    {
        _targetState.Speed = speed;
    }
}
```

#### 6. IAnimationService 接口更新

**文件**: `Assets/SkillEditor/Runtime/Services/IServices.cs`

- `ManualUpdate(float deltaTime)` → `SetSpeed(float speedScale)`
- 明确语义：速度控制而非驱动更新

#### 7. 创建 MAnimAnimationService 适配器

**文件**: `Assets/GameClient/MAnimSystem/MAnimAnimationService.cs` (新建)

- 实现 `IAnimationService` 接口
- 将 SkillEditor 调用转发到 AnimComponent

#### 8. RuntimeAnimationService 更新

**文件**: `Assets/SkillEditor/Runtime/Services/RuntimeAnimationService.cs`

- 实现新的 `IAnimationService` 接口
- `Evaluate()` 方法留空（运行时不需要）

### 架构对比

```
修改前（错误）                    修改后（正确）
─────────────────────────────────────────────────────
UpdateMode: Auto / Manual    →    始终 MonoUpdate 驱动
ManualUpdate(dt) 驱动动画    →    SetSpeed(scale) 控制速度
运行时也需要 Evaluate        →    仅编辑器预览需要 Evaluate
```

### 运行时流程

```
网络层收到指令 { skillId, frame }
    ↓
SkillRunner.ManualUpdate(fixedDt)
    ↓
Tick(fixedDt) 推进固定步长
    ↓
OnEnter → Play(clip)           ← 只发控制命令
OnTick → 逻辑判定
OnExit → 切换状态
    ↓
AnimComponent 由 Unity Update 自动驱动
（不调用 OnUpdate，不做 Evaluate）
```

### 文件变更清单

| 文件 | 变更类型 | 说明 |
|------|----------|------|
| ISkillContext.cs | 修改 | 新增 IsPreviewMode |
| SkillRunner.cs | 修改 | ClipContext 实现 IsPreviewMode |
| AnimationClipProcessor.cs | 修改 | OnUpdate 内部判断 |
| AnimComponent.cs | 重写 | 移除 UpdateMode，新增 SetSpeed |
| AnimLayer.cs | 修改 | 新增 SetSpeed |
| IServices.cs | 修改 | ManualUpdate → SetSpeed |
| MAnimAnimationService.cs | 新建 | 适配器实现 |
| RuntimeAnimationService.cs | 重写 | 更新接口实现 |

### 相关文档

- [帧同步架构修正方案](./MAnimSystem_FrameSync_Refactor_Plan.md)
- [SkillEditor 集成计划](./MAnimSystem_SkillEditor_Integration_Plan.md)

---

**修正日期**: 2026-02-14
