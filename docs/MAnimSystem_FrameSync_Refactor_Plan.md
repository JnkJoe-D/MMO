# MAnimSystem 帧同步架构修正方案

## 一、背景与问题

### 1.1 当前设计的问题

之前的实现假设运行时需要 `ManualUpdate` 驱动动画更新，但这与帧同步的正确做法不符：

| 概念 | 错误理解 | 正确理解 |
|:---|:---|:---|
| **动画驱动** | 运行时需要手动驱动 | 始终由 Unity MonoUpdate 自动驱动 |
| **ManualUpdate** | 用于驱动动画播放 | 用于控制（速度、状态切换） |
| **Evaluate** | 运行时和编辑器都需要 | 仅编辑器预览需要 |

### 1.2 正确的帧同步流程

```
┌─────────────────────────────────────────────────────────────────┐
│                     运行时模式 (Runtime)                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  网络层收到指令 { skillId, frame }                                │
│       ↓                                                         │
│  SkillRunner.ManualUpdate(fixedDt)                              │
│       ↓                                                         │
│  Tick(fixedDt) 推进固定步长                                      │
│       ↓                                                         │
│  OnEnter → Play(clip)           ← 只发控制命令                   │
│  OnTick → 逻辑判定（伤害判定、状态检查等）                         │
│  OnExit → 切换状态/返回待机                                       │
│       ↓                                                         │
│  AnimComponent 由 Unity Update 自动驱动                          │
│  （不调用 OnUpdate，不做 Evaluate）                               │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                     编辑器模式 (Editor Preview)                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  SkillRunner.EvaluateAt(time)  ← 拖拽时间轴                      │
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

## 二、任务清单

### 任务 1: ISkillContext 增加 IsPreviewMode 属性

**文件**: `Assets/SkillEditor/Runtime/System/ISkillContext.cs`

**修改内容**:

```csharp
using UnityEngine;

namespace SkillEditor
{
    public interface ISkillContext
    {
        GameObject Owner { get; }
        
        /// <summary>
        /// 是否为编辑器预览模式。
        /// true: 编辑器预览，需要手动采样动画帧。
        /// false: 运行时模式，动画由 Unity 自动驱动。
        /// </summary>
        bool IsPreviewMode { get; }
        
        /// <summary>
        /// 获取环境特定的服务，如音频管理器、特效管理器
        /// </summary>
        T GetService<T>() where T : class;
    }
}
```

---

### 任务 2: ClipContext 实现 IsPreviewMode

**文件**: `Assets/SkillEditor/Runtime/System/SkillRunner.cs`

**修改 ClipContext 类**:

```csharp
public class ClipContext : ISkillContext
{
    public GameObject Owner { get; private set; }
    public object CurrentClipData { get; set; }
    
    /// <summary>
    /// 是否为编辑器预览模式。
    /// </summary>
    public bool IsPreviewMode { get; set; } = false;
    
    private Dictionary<System.Type, object> _services = new Dictionary<System.Type, object>();

    public ClipContext(GameObject owner)
    {
        Owner = owner;
    }

    public void RegisterService<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    public T GetService<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out object service))
        {
            return service as T;
        }
        Debug.LogError($"Service {typeof(T).Name} not found!");
        return null;
    }
}
```

**修改 SkillRunner.EvaluateAt 方法**:

在编辑器预览入口设置 `IsPreviewMode = true`:

```csharp
/// <summary>
/// 跳跃到指定时间并求值 (用于编辑器拖拽预览)
/// </summary>
public void EvaluateAt(float time)
{
    if (_context == null) _context = new ClipContext(gameObject);
    
    // 设置预览模式
    _context.IsPreviewMode = true;
    
    // ... 其余逻辑不变
}
```

**修改 SkillRunner.Tick 方法**:

在运行时 Tick 中设置 `IsPreviewMode = false`:

```csharp
/// <summary>
/// 核心推进逻辑
/// </summary>
private void Tick(float dt)
{
    if (_context != null)
    {
        // 运行时模式
        _context.IsPreviewMode = false;
    }
    
    // ... 其余逻辑不变
}
```

---

### 任务 3: AnimationClipProcessor.OnUpdate 内部判断模式

**文件**: `Assets/SkillEditor/Runtime/Logic/Processors/AnimationClipProcessor.cs`

**修改内容**:

```csharp
using UnityEngine;

namespace SkillEditor
{
    public class AnimationClipProcessor : BaseClipProcessor
    {
        public override void OnEnter(ISkillContext context)
        {
            var data = context.GetData<AnimationClip>();
            var animService = context.GetService<IAnimationService>();
            
            if (animService != null && data != null && data.animationClip != null)
            {
                // 运行时和编辑器都需要：播放动画
                animService.Play(data.animationClip, 0.1f);
            }
        }

        public override void OnUpdate(ISkillContext context, float progress)
        {
            // 运行时直接返回，不做采样
            // 动画由 Unity Update 自动驱动
            if (!context.IsPreviewMode) return;
            
            // 以下仅编辑器预览模式执行
            var animService = context.GetService<IAnimationService>();
            var data = context.GetData<AnimationClip>();
            
            if (data != null && animService != null)
            {
                // 计算绝对时间
                float time = data.StartTime + data.Duration * progress;
                
                // 编辑器预览：手动采样动画帧
                animService.Evaluate(time);
            }
        }

        public override void OnExit(ISkillContext context)
        {
            // 动画通常不需要显式停止，让其自然播放/融合到下一个
        }

        public override void OnTick(ISkillContext context, float frameTime, float prevFrameTime)
        {
            // 运行时逻辑判定（伤害判定、状态检查等）
            // 此处可根据需要添加逻辑
        }
    }
}
```

---

### 任务 4: AnimComponent 移除 UpdateMode，始终 MonoUpdate 驱动

**文件**: `Assets/GameClient/MAnimSystem/AnimComponent.cs`

**修改内容**:

#### 4.1 移除 UpdateMode 枚举和相关字段

```csharp
// 删除以下代码：
// public enum UpdateMode { Auto, Manual }
// public UpdateMode updateMode = UpdateMode.Auto;
```

#### 4.2 修改 Update 方法

```csharp
private void Update()
{
    if (!_isGraphCreated) return;
    
    // 始终自动更新，由 Unity 驱动
    UpdateInternal(Time.deltaTime);
}
```

#### 4.3 修改 ManualUpdate 方法语义

```csharp
/// <summary>
/// 设置动画播放速度。
/// 用于帧同步场景下的速度控制。
/// </summary>
/// <param name="speedScale">速度缩放因子</param>
public void SetSpeed(float speedScale)
{
    if (!_isGraphCreated) return;
    
    // 设置 Graph 的播放速度
    // 注意：这会影响所有动画的播放速度
    foreach (var layer in _layers)
    {
        layer?.SetSpeed(speedScale);
    }
}

/// <summary>
/// [已弃用] 请使用 SetSpeed 方法。
/// 保留此方法仅为向后兼容。
/// </summary>
/// <param name="deltaTime">此参数在自动模式下被忽略</param>
[System.Obsolete("请使用 SetSpeed(float speedScale) 方法。AnimComponent 始终由 Unity Update 自动驱动。")]
public void ManualUpdate(float deltaTime)
{
    // 在新架构下，此方法不再需要
    // 动画始终由 Unity Update 自动驱动
}
```

#### 4.4 保留 Evaluate 方法（编辑器专用）

```csharp
/// <summary>
/// 采样当前动画到指定时间。
/// 仅用于编辑器预览或时间轴拖拽。
/// 运行时请勿调用此方法。
/// </summary>
/// <param name="time">目标时间（秒）</param>
public void Evaluate(float time)
{
    if (!_isGraphCreated) return;

    var state = GetLayer(0).GetCurrentState();
    if (state != null)
    {
        state.Time = time;
        Graph.Evaluate(0f);
    }
}
```

---

### 任务 5: AnimLayer 增加 SetSpeed 方法

**文件**: `Assets/GameClient/MAnimSystem/AnimLayer.cs`

**新增内容**:

```csharp
/// <summary>
/// 设置当前动画的播放速度。
/// </summary>
/// <param name="speed">速度因子 (1.0 = 正常速度)</param>
public void SetSpeed(float speed)
{
    if (_targetState != null)
    {
        _targetState.Speed = speed;
    }
}
```

---

### 任务 6: IAnimationService 接口语义明确化

**文件**: `Assets/SkillEditor/Runtime/Services/IServices.cs`

**修改内容**:

```csharp
using UnityEngine;

namespace SkillEditor
{
    public interface IAnimationService
    {
        /// <summary>
        /// 播放动画片段。
        /// 运行时和编辑器都需要。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <param name="transitionDuration">过渡时间</param>
        void Play(UnityEngine.AnimationClip clip, float transitionDuration);
        
        /// <summary>
        /// 采样到指定时间。
        /// 仅编辑器预览需要，运行时请勿调用。
        /// </summary>
        /// <param name="time">目标时间（秒）</param>
        void Evaluate(float time);
        
        /// <summary>
        /// 设置动画播放速度。
        /// 用于帧同步场景下的速度控制。
        /// </summary>
        /// <param name="speedScale">速度缩放因子</param>
        void SetSpeed(float speedScale);
    }
    
    // ... 其他接口保持不变
}
```

---

### 任务 7: MAnimAnimationService 适配器重新设计

**文件**: `Assets/GameClient/MAnimSystem/MAnimAnimationService.cs` (新建)

**完整代码**:

```csharp
using UnityEngine;
using SkillEditor;

namespace Game.MAnimSystem
{
    /// <summary>
    /// MAnimSystem 的 SkillEditor 动画服务适配器。
    /// 实现 IAnimationService 接口，将 SkillEditor 的动画调用转发到 AnimComponent。
    /// 
    /// 设计说明：
    /// - Play: 运行时和编辑器都需要，触发动画播放。
    /// - Evaluate: 仅编辑器预览需要，手动采样动画帧。
    /// - SetSpeed: 用于速度控制，不影响动画驱动方式。
    /// 
    /// 动画始终由 AnimComponent 的 Unity Update 自动驱动。
    /// </summary>
    public class MAnimAnimationService : IAnimationService
    {
        /// <summary>
        /// 关联的 AnimComponent 实例。
        /// </summary>
        private AnimComponent _animComponent;
        
        /// <summary>
        /// 当前播放的动画片段。
        /// </summary>
        private AnimationClip _currentClip;
        
        /// <summary>
        /// 构造动画服务适配器。
        /// </summary>
        /// <param name="animComponent">AnimComponent 实例</param>
        public MAnimAnimationService(AnimComponent animComponent)
        {
            _animComponent = animComponent;
        }
        
        /// <summary>
        /// 播放动画片段。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <param name="transitionDuration">过渡时间</param>
        public void Play(AnimationClip clip, float transitionDuration)
        {
            if (_animComponent == null || clip == null) return;
            
            _currentClip = clip;
            _animComponent.Play(clip, transitionDuration);
        }
        
        /// <summary>
        /// 采样到指定时间。
        /// 仅编辑器预览需要，运行时请勿调用。
        /// </summary>
        /// <param name="time">目标时间（秒）</param>
        public void Evaluate(float time)
        {
            if (_animComponent == null) return;
            
            // 编辑器预览：手动采样动画帧
            _animComponent.Evaluate(time);
        }
        
        /// <summary>
        /// 设置动画播放速度。
        /// </summary>
        /// <param name="speedScale">速度缩放因子</param>
        public void SetSpeed(float speedScale)
        {
            if (_animComponent == null) return;
            
            _animComponent.SetSpeed(speedScale);
        }
        
        /// <summary>
        /// 获取当前播放的动画片段。
        /// </summary>
        /// <returns>当前动画片段</returns>
        public AnimationClip GetCurrentClip()
        {
            return _currentClip;
        }
    }
}
```

---

### 任务 8: RuntimeAnimationService 更新

**文件**: `Assets/SkillEditor/Runtime/Services/RuntimeAnimationService.cs`

**修改内容**:

```csharp
using UnityEngine;

namespace SkillEditor
{
    public class RuntimeAnimationService : IAnimationService
    {
        private Animator _animator;
        private float _originalSpeed = 1.0f;

        public RuntimeAnimationService(Animator animator)
        {
            _animator = animator;
            if (_animator != null)
            {
                _originalSpeed = _animator.speed;
            }
        }

        public void Play(UnityEngine.AnimationClip clip, float transitionDuration)
        {
            if (_animator == null || clip == null) return;
            // Animator 播放逻辑
            Debug.Log($"[RuntimeAnimation] Playing clip: {clip.name}");
        }

        public void Evaluate(float time)
        {
            // 运行时不需要 Evaluate
            // 动画由 Unity 自动驱动
        }

        public void SetSpeed(float speedScale)
        {
            if (_animator == null) return;
            _animator.speed = _originalSpeed * speedScale;
        }
    }
}
```

---

## 三、文件变更清单

| 文件 | 操作 | 变更内容 |
|------|------|----------|
| `ISkillContext.cs` | 修改 | 新增 `IsPreviewMode` 属性 |
| `SkillRunner.cs` | 修改 | ClipContext 实现 IsPreviewMode，EvaluateAt/Tick 设置模式 |
| `AnimationClipProcessor.cs` | 修改 | OnUpdate 内部判断 IsPreviewMode |
| `AnimComponent.cs` | 修改 | 移除 UpdateMode，始终 MonoUpdate 驱动，新增 SetSpeed |
| `AnimLayer.cs` | 修改 | 新增 SetSpeed 方法 |
| `IServices.cs` | 修改 | ManualUpdate 改为 SetSpeed，明确语义 |
| `MAnimAnimationService.cs` | 新建 | IAnimationService 适配器实现 |
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
   - 验证 Play 和 Evaluate 都被调用

2. **运行时帧同步流程**
   - 模拟网络指令，触发技能播放
   - 验证 Play 被调用，Evaluate 不被调用
   - 验证动画由 Unity 自动驱动

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

### 修改前

```
┌─────────────────────────────────────────────────────────────┐
│                    AnimComponent                            │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ UpdateMode: Auto / Manual                           │   │
│  │                                                     │   │
│  │ Auto:   Update() → UpdateInternal(dt)              │   │
│  │ Manual: ManualUpdate(dt) → UpdateInternal(dt)      │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### 修改后

```
┌─────────────────────────────────────────────────────────────┐
│                    AnimComponent                            │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 始终由 Unity Update 自动驱动                          │   │
│  │                                                     │   │
│  │ Update() → UpdateInternal(dt)  ← 始终执行           │   │
│  │                                                     │   │
│  │ 控制接口：                                           │   │
│  │   Play(clip, fade)     → 播放动画                   │   │
│  │   SetSpeed(scale)      → 速度控制                   │   │
│  │   Evaluate(time)       → 编辑器预览采样             │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

---

**文档日期**: 2026-02-14
