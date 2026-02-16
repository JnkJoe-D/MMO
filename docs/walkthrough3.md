# SkillEditor 多 Runner 架构重构工作总结

## 概述

基于对帧同步架构的正确理解和 ActionEditor 的参考分析，对 SkillEditor 进行了重大架构重构。核心变化是将 Runner 从 MonoBehaviour 改为纯 C# 类，实现了服务按需创建机制，解耦了技能系统与动画系统。

**重要更新**：修正了服务管理机制，所有服务都是 Transient（瞬态），每次调用都创建新实例，支持多轨道并行（分层动画、多特效等）。

---

## 完成的工作

### 1. 创建核心架构文件

| 文件 | 说明 |
|------|------|
| SkillSystemManager.cs | Unity 生命周期单例，分发 Update 事件 |
| SkillDriver.cs | 驱动器单例，管理所有 Runner 和对象池 |
| ServiceFactory.cs | 服务工厂，按需创建服务（每次调用都创建新实例） |
| SkillRunner.cs | 新版 Runner（纯 C# 类），管理单个角色的技能实例 |
| SkillInstance.cs | 技能实例，表示一次技能播放的运行时状态 |
| SkillContext.cs | 新版上下文，持有 Owner 引用，按需创建服务 |
| ObjectPool.cs | 对象池，复用 SkillInstance 和 SkillContext |
| RuntimeAnimationService.cs | 动画服务实现，支持层索引 |
| RuntimeAnimatorService.cs | Animator 备用服务实现 |
| RuntimeAudioService.cs | 音频服务实现 |

### 2. 更新接口

| 文件 | 修改内容 |
|------|----------|
| ISkillContext.cs | 新增专用方法：GetAnimationService(layerIndex)、GetVFXService()、GetAudioService() |

### 3. 架构变更

```
修改前                              修改后
─────────────────────────────────────────────────────────────
SkillRunner : MonoBehaviour    →   SkillRunner : 纯 C# 类
每角色挂载 Runner               →   每角色独立 Runner，由 SkillDriver 统一驱动
手动注册服务                   →   ServiceFactory 按需创建（每次调用新实例）
全局服务缓存                   →   无缓存，支持多轨道并行
无对象池                       →   SkillInstancePool + SkillContextPool
AnimComponent 实现接口         →   AnimComponent 保持独立，适配器桥接
```

---

## 新架构数据流向分析

### 1. 整体架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                    Unity 生命周期层                              │
├─────────────────────────────────────────────────────────────────┤
│  SkillSystemManager (单例 MonoBehaviour)                        │
│       │                                                         │
│       ├── Update()    → SkillDriver.Instance.Update(dt)         │
│       └── FixedUpdate() → SkillDriver.Instance.FixedUpdate(dt)  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    驱动层 (Singleton)                            │
├─────────────────────────────────────────────────────────────────┤
│  SkillDriver                                                    │
│       │                                                         │
│       ├── _runners: List<SkillRunner>                          │
│       ├── _runnersByOwner: Dictionary<int, SkillRunner>        │
│       ├── _instancePool: SkillInstancePool                     │
│       ├── _contextPool: SkillContextPool                       │
│       │                                                         │
│       ├── PlaySkill(skillData, owner) → SkillInstance          │
│       ├── StopSkill(instanceId)                                │
│       ├── PauseOwner(owner) / ResumeOwner(owner)               │
│       └── RemoveOwner(owner)                                   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    服务工厂层                                    │
├─────────────────────────────────────────────────────────────────┤
│  ServiceFactory (静态类)                                        │
│       │                                                         │
│       ├── CreateAnimationService(owner, layerIndex)            │
│       │       └── 每次调用都创建新实例                           │
│       │                                                         │
│       ├── CreateVFXService(owner)                              │
│       │       └── 每次调用都创建新实例                           │
│       │                                                         │
│       └── CreateAudioService(owner)                            │
│               └── 每次调用都创建新实例                           │
│                                                                 │
│  设计原则：所有服务都是 Transient（瞬态）                         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    运行层 (Per-Owner)                            │
├─────────────────────────────────────────────────────────────────┤
│  SkillRunner (纯 C# 类)                                         │
│       │                                                         │
│       ├── Owner: GameObject                                    │
│       ├── _activeSkills: List<SkillInstance>                   │
│       ├── _pendingSkills: Queue<SkillInstance>                 │
│       │                                                         │
│       ├── Play(skillData) → SkillInstance                      │
│       ├── Stop(skillInstance)                                  │
│       ├── Pause() / Resume()                                   │
│       └── Update(deltaTime)                                    │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    实例层 (Per-Skill)                            │
├─────────────────────────────────────────────────────────────────┤
│  SkillInstance                                                  │
│       │                                                         │
│       ├── InstanceId: int                                      │
│       ├── Data: SkillTimeline                                  │
│       ├── CurrentTime: float                                   │
│       ├── IsFinished: bool                                     │
│       ├── _owner: GameObject                                   │
│       ├── _context: SkillContext                               │
│       ├── _processors: List<ProcessorState>                    │
│       │                                                         │
│       ├── Initialize(instanceId, data, owner)                  │
│       ├── Start(contextPool)                                   │
│       ├── Tick(deltaTime)                                      │
│       ├── Evaluate(time)      // 编辑器预览                     │
│       └── Dispose()                                            │
└─────────────────────────────────────────────────────────────────┘
```

### 2. 播放技能数据流

```
用户调用: SkillDriver.Instance.PlaySkill(skillData, owner)
    │
    ├── 1. 获取或创建 Runner
    │       │
    │       └── GetRunner(owner)
    │               │
    │               ├── 检查 _runnersByOwner 缓存
    │               │       ├── 已存在 → 返回缓存的 Runner
    │               │       └── 不存在 → 创建新 Runner
    │               │
    │               └── 新建 Runner(owner, instancePool, contextPool)
    │
    └── 2. 创建并启动技能实例
            │
            ├── 从对象池获取 SkillInstance
            │       └── _instancePool.Get()
            │
            ├── 初始化实例（传入 owner）
            │       └── instance.Initialize(instanceId, skillData, owner)
            │
            └── 加入待启动队列
                    └── _pendingSkills.Enqueue(instance)

注意：不再预先创建服务！
服务在 Processor.OnEnter() 调用时按需创建。
```

### 3. 每帧更新数据流

```
Unity Update: SkillSystemManager.Update()
    │
    └── SkillDriver.Instance.Update(deltaTime)
            │
            └── 遍历所有 Runner
                    │
                    └── runner.Update(deltaTime)
                            │
                            ├── 1. 处理待启动的技能
                            │       │
                            │       └── while (_pendingSkills.Count > 0)
                            │               │
                            │               ├── instance = _pendingSkills.Dequeue()
                            │               ├── instance.Start(_contextPool)
                            │               │       └── _context.SetOwner(_owner)
                            │               └── _activeSkills.Add(instance)
                            │
                            └── 2. 更新所有活动技能
                                    │
                                    └── for each instance in _activeSkills
                                            │
                                            ├── instance.Tick(deltaTime)
                                            │       │
                                            │       ├── 更新 CurrentTime
                                            │       │
                                            │       └── 区间扫描所有 Processor
                                            │               │
                                            │               ├── 检查时间重叠
                                            │               │       isOverlap = (clip.StartTime < CurrentTime) 
                                            │               │                  && (clip.EndTime > prevTime)
                                            │               │
                                            │               ├── 进入片段
                                            │               │       if (!isRunning) → OnEnter()
                                            │               │               │
                                            │               │               └── context.GetAnimationService(layerIndex)
                                            │               │                       │
                                            │               │                       └── 每次创建新实例！
                                            │               │
                                            │               ├── 更新片段
                                            │               │       OnUpdate() + OnTick()
                                            │               │
                                            │               └── 退出片段
                                            │                       if (clip.EndTime <= CurrentTime) → OnExit()
                                            │
                                            └── 检查是否完成
                                                    if (IsFinished)
                                                    ├── instance.Dispose()
                                                    ├── _instancePool.Return(instance)
                                                    └── _activeSkills.RemoveAt(i)
```

### 4. 服务按需创建流程

```
Processor.OnEnter(context)
    │
    └── context.GetAnimationService(layerIndex)
            │
            └── ServiceFactory.CreateAnimationService(owner, layerIndex)
                    │
                    ├── 获取 AnimComponent
                    │       owner.GetComponent<AnimComponent>()
                    │
                    └── 创建服务实例
                            new RuntimeAnimationService(anim, layerIndex)
                            │
                            └── 每次调用都创建新实例！

特点：
- 服务不缓存，每次调用都创建新实例
- 支持多轨道并行（分层动画、多特效）
- 服务无状态，状态由组件管理
```

### 5. 多轨道并行执行流程

```
场景：技能包含分层动画 + 多特效

技能数据：
├── AnimationTrack1 (上半身动画)
│       └── AnimationClip (Attack_Upper.anim)
│
├── AnimationTrack2 (下半身动画)
│       └── AnimationClip (Run_Lower.anim)
│
├── VFXTrack1 (火焰特效)
│       └── VFXClip (Fire.prefab)
│
└── VFXTrack2 (烟雾特效)
        └── VFXClip (Smoke.prefab)

执行流程：
├── Track1.OnEnter() → GetAnimationService(0) → 新实例 A
├── Track2.OnEnter() → GetAnimationService(1) → 新实例 B（不同层！）
├── VFXTrack1.OnEnter() → GetVFXService() → 新实例 C
└── VFXTrack2.OnEnter() → GetVFXService() → 新实例 D

结果：每个轨道独立的服务实例，互不干扰！
```

### 6. 多角色并行执行流程

```
场景：3个角色同时播放技能

SkillDriver.Update(deltaTime)
    │
    ├── Runner1.Update(deltaTime)        // 角色1
    │       │
    │       ├── SkillInstance1.Tick()    // 技能A
    │       │       ├── Processor1.OnEnter()
    │       │       │       └── GetAnimationService() → 新实例
    │       │       ├── Processor1.OnUpdate()
    │       │       └── Processor1.OnTick()
    │       │
    │       └── SkillInstance2.Tick()    // 技能B
    │               └── ...
    │
    ├── Runner2.Update(deltaTime)        // 角色2
    │       │
    │       └── SkillInstance3.Tick()    // 技能C
    │               └── ...
    │
    └── Runner3.Update(deltaTime)        // 角色3
            │
            └── SkillInstance4.Tick()    // 技能D
                    └── ...

特点：
- 每个角色有独立的 Runner
- 每个 Runner 管理该角色的所有技能实例
- 所有 Runner 在同一帧顺序执行
- 每个 Processor 调用时独立创建服务实例
```

---

## 依赖关系说明

### 单向依赖设计

```
┌─────────────────────────────────────────────────────────────────┐
│  AnimComponent (动画系统)                                        │
│       │                                                         │
│       └── 独立存在，不知道 SkillEditor                           │
└─────────────────────────────────────────────────────────────────┘
                              ▲
                              │ (技能系统主动获取)
                              │
┌─────────────────────────────────────────────────────────────────┐
│  SkillEditor (技能系统)                                          │
│       │                                                         │
│       ├── ServiceFactory 从 owner 获取 AnimComponent            │
│       └── 创建 RuntimeAnimationService 适配器                    │
└─────────────────────────────────────────────────────────────────┘
```

### AnimComponent 保持独立

```csharp
// AnimComponent 不需要实现任何 SkillEditor 的接口
// 它只提供动画播放能力，不知道技能系统的存在
public class AnimComponent : MonoBehaviour
{
    public void Play(AnimationClip clip, int layerIndex, float fadeDuration) { }
    public void SetSpeed(float speed) { }
    public void Evaluate(float time) { }
}
```

### 适配器负责桥接

```csharp
// RuntimeAnimationService 是适配器
// 它知道 AnimComponent 和 IAnimationService 两者
// 每次调用都创建新实例，支持分层动画
public class RuntimeAnimationService : IAnimationService
{
    private readonly AnimComponent _anim;
    private readonly int _layerIndex;
    
    public RuntimeAnimationService(AnimComponent anim, int layerIndex = 0)
    {
        _anim = anim;
        _layerIndex = layerIndex;
    }
    
    public void Play(AnimationClip clip, float transitionDuration)
    {
        _anim.Play(clip, _layerIndex, transitionDuration);
    }
}
```

---

## 服务生命周期说明

### 所有服务都是 Transient（瞬态）

| 服务类型 | 生命周期 | 原因 |
|----------|----------|------|
| IAnimationService | Transient | 支持分层动画，每个轨道独立 |
| IVFXService | Transient | 每个特效轨道独立 |
| IAudioService | Transient | 每个音频轨道独立 |

### 为什么不缓存服务？

```
假设缓存服务：

技能数据：
├── VFXTrack1 (火焰特效) → GetVFXService() → 缓存实例 A
└── VFXTrack2 (烟雾特效) → GetVFXService() → 返回缓存实例 A ❌

结果：两个轨道共用同一个服务，无法独立播放！
```

### 正确的做法

```
每次调用都创建新实例：

技能数据：
├── VFXTrack1 (火焰特效) → GetVFXService() → 新实例 A
└── VFXTrack2 (烟雾特效) → GetVFXService() → 新实例 B

结果：每个轨道独立的服务实例，互不干扰！
```

---

## 使用示例

### 播放技能

```csharp
public class Character : MonoBehaviour
{
    [SerializeField] private SkillTimeline _skillData;
    
    public void PlaySkill()
    {
        // 传入技能数据和目标角色
        // 服务在 Processor 执行时按需创建
        var instance = SkillDriver.Instance.PlaySkill(_skillData, gameObject);
        
        if (instance != null)
        {
            Debug.Log($"Playing skill, instanceId: {instance.InstanceId}");
        }
    }
    
    public void StopAllSkills()
    {
        SkillDriver.Instance.RemoveOwner(gameObject);
    }
    
    public void PauseSkills()
    {
        SkillDriver.Instance.PauseOwner(gameObject);
    }
    
    public void ResumeSkills()
    {
        SkillDriver.Instance.ResumeOwner(gameObject);
    }
}
```

### Processor 中使用服务

```csharp
public class AnimationClipProcessor : BaseClipProcessor
{
    public override void OnEnter(ISkillContext context)
    {
        var data = context.GetData<SkillAnimationClip>();
        if (data == null || data.animationClip == null) return;
        
        // 每次调用都创建新的服务实例
        // layerIndex 可从 data 中获取，支持分层动画
        var animService = context.GetAnimationService(data.layerIndex);
        animService?.Play(data.animationClip, 0.1f);
    }
}
```

### 帧同步场景

```csharp
public class FrameSyncManager
{
    private const float FIXED_DT = 1f / 30f;
    
    public void OnFrameUpdate()
    {
        // 使用固定时间步长
        SkillDriver.Instance.FixedUpdate(FIXED_DT);
    }
}
```

---

## 对比总结

| 方面 | 修改前 | 修改后 |
|------|--------|--------|
| **Runner 类型** | MonoBehaviour | 纯 C# 类 |
| **单例模式** | 无 | SkillSystemManager + SkillDriver |
| **多角色支持** | 每角色挂载 Runner | 每角色独立 Runner，统一驱动 |
| **服务创建** | 手动注册 + 全局缓存 | 按需创建，每次调用新实例 |
| **服务生命周期** | Singleton | Transient |
| **多轨道并行** | ❌ 不支持 | ✅ 支持（分层动画、多特效） |
| **对象池** | 无 | SkillInstancePool + SkillContextPool |
| **依赖方向** | AnimComponent 实现接口 | AnimComponent 独立，适配器桥接 |

---

## 相关文档

- [SkillEditor 架构分析报告](./SkillEditor_Architecture_Analysis.md)
- [SkillEditor 重构方案](./SkillEditor_Refactor_Plan.md)
- [MAnimSystem 帧同步架构修正方案](./MAnimSystem_FrameSync_Refactor_Plan.md)

---

**重构日期**: 2026-02-14
**更新说明**: 修正服务管理机制，所有服务改为 Transient，支持多轨道并行
