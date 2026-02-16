# SkillEditor 架构分析报告

## 一、项目结构概览

```
SkillEditor/
├── Data/                          # 核心数据模型
│   ├── ClipBase.cs               # 片段基类
│   ├── TrackBase.cs              # 轨道基类
│   ├── TrackGroup.cs             # 轨道分组
│   └── SkillTimeline.cs          # 时间轴主数据
│
├── Runtime/                       # 运行时系统
│   ├── Attributes/
│   │   └── SkillAttributes.cs    # 属性标记
│   ├── Data/
│   │   ├── Base/SkillClip.cs     # 泛型片段基类
│   │   ├── Clips/                # 具体片段类型
│   │   └── Tracks/               # 具体轨道类型
│   ├── Logic/
│   │   ├── Base/BaseClipProcessor.cs
│   │   └── Processors/           # 片段处理器
│   ├── Services/
│   │   ├── IServices.cs          # 服务接口
│   │   ├── RuntimeAnimationService.cs
│   │   └── RuntimeVFXService.cs
│   └── System/
│       ├── ISkillContext.cs      # 上下文接口
│       └── SkillRunner.cs        # 运行时执行器
│
└── Editor/                        # 编辑器扩展
    ├── Drawers/
    │   ├── Base/                 # 绘制器基类
    │   └── Impl/                 # 具体绘制器
    └── Services/
        └── EditorAnimationService.cs

Editor/SkillEditor/                 # 编辑器窗口
├── Core/
│   ├── SerializationUtility.cs   # 序列化工具
│   ├── SkillEditorEvents.cs      # 事件总线
│   └── SkillEditorState.cs       # 状态管理
├── Views/
│   ├── TimelineView.cs           # 时间轴视图
│   ├── ToolbarView.cs            # 工具栏视图
│   └── TrackListView.cs          # 轨道列表视图
├── SkillEditorWindow.cs          # 主窗口
├── SkillEditorSettingsWindow.cs  # 设置窗口
└── TrackObjectWrapper.cs         # Inspector 包装器
```

---

## 二、类职责详解

### 2.1 数据模型层

| 类名 | 职责 | 关键成员 |
|------|------|----------|
| **SkillTimeline** | 技能时间轴主数据容器，存储技能配置 | `skillId`, `duration`, `tracks`, `groups` |
| **TrackBase** | 轨道抽象基类，管理片段集合 | `trackId`, `clips`, `CanOverlap` |
| **ClipBase** | 片段抽象基类，定义时间属性 | `startTime`, `duration`, `blendInDuration` |
| **TrackGroup** | 轨道分组，支持折叠和批量管理 | `groupId`, `trackIds`, `isCollapsed` |

### 2.2 运行时系统层

| 类名 | 职责 | 关键方法 |
|------|------|----------|
| **SkillRunner** | 运行时执行引擎，驱动片段播放 | `ManualUpdate()`, `EvaluateAt()`, `Tick()` |
| **BaseClipProcessor** | 片段处理器抽象基类 | `OnEnter()`, `OnUpdate()`, `OnExit()`, `OnTick()` |
| **ClipContext** | 运行时上下文，提供服务注册 | `GetService<T>()`, `RegisterService<T>()` |
| **ISkillContext** | 上下文接口，解耦运行时与编辑器 | `Owner`, `IsPreviewMode`, `GetService<T>()` |

### 2.3 服务层

| 接口/类名 | 职责 | 实现类 |
|-----------|------|--------|
| **IAnimationService** | 动画服务接口 | `RuntimeAnimationService`, `EditorAnimationService` |
| **IVFXService** | 特效服务接口 | `RuntimeVFXService` |
| **IAudioService** | 音频服务接口 | (待实现) |

### 2.4 编辑器核心层

| 类名 | 职责 | 关键成员 |
|------|------|----------|
| **SkillEditorWindow** | 编辑器主窗口，协调所有子视图 | `state`, `events`, `timelineView`, `trackListView` |
| **SkillEditorState** | 全局 UI 状态管理 | `zoom`, `timeIndicator`, `selectedClips`, `trackCache` |
| **SkillEditorEvents** | 事件总线，解耦视图通信 | `OnSelectionChanged`, `OnTimeIndicatorChanged` |
| **SerializationUtility** | JSON 序列化/反序列化 | `ExportToJson()`, `ImportFromJson()` |

### 2.5 编辑器视图层

| 类名 | 职责 | 代码行数 |
|------|------|----------|
| **TimelineView** | 时间轴视图，处理片段拖拽、吸附、绘制 | ~2540 行 |
| **TrackListView** | 轨道列表视图，分组管理、拖拽排序 | ~1160 行 |
| **ToolbarView** | 工具栏视图，播放控制、导入导出 | ~235 行 |

### 2.6 Inspector 包装层

| 类名 | 职责 |
|------|------|
| **ClipObject** | 片段的 ScriptableObject 包装，用于 Inspector 显示 |
| **TrackObject** | 轨道的 ScriptableObject 包装 |
| **GroupObject** | 分组的 ScriptableObject 包装 |

---

## 三、类依赖关系图

```
┌─────────────────────────────────────────────────────────────────────┐
│                          编辑器层 (Editor)                           │
├─────────────────────────────────────────────────────────────────────┤
│  SkillEditorWindow                                                   │
│       ├── SkillEditorState (状态管理)                                │
│       ├── SkillEditorEvents (事件总线)                               │
│       ├── TimelineView ──────────┐                                   │
│       ├── TrackListView ─────────┼── 依赖 SkillTimeline 数据        │
│       └── ToolbarView ───────────┘                                   │
│                                                                      │
│  Inspector 包装器: ClipObject, TrackObject, GroupObject             │
│       └── DrawerFactory ──> ClipDrawer / TrackDrawer                │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                          数据层 (Data)                               │
├─────────────────────────────────────────────────────────────────────┤
│  SkillTimeline (ScriptableObject)                                   │
│       ├── List<TrackGroup> groups                                   │
│       └── List<TrackBase> tracks                                    │
│              └── List<ClipBase> clips                               │
│                                                                      │
│  继承关系:                                                           │
│  TrackBase <── AnimationTrack, VFXTrack, DamageTrack, etc.         │
│  ClipBase <── SkillClip<T> <── AnimationClip, VFXClip, etc.        │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        运行时层 (Runtime)                            │
├─────────────────────────────────────────────────────────────────────┤
│  SkillRunner (MonoBehaviour)                                        │
│       ├── ClipContext (ISkillContext 实现)                          │
│       ├── List<ProcessorState> _states                              │
│       └── Services: IAnimationService, IVFXService                  │
│                                                                      │
│  BaseClipProcessor <── AnimationClipProcessor, VFXClipProcessor    │
│                                                                      │
│  服务实现:                                                           │
│  RuntimeAnimationService ──> AnimComponent (外部依赖)               │
│  RuntimeVFXService ──> ParticleSystem                               │
│  EditorAnimationService ──> Animator + PlayableGraph                │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 四、数据流分析

### 4.1 编辑器到运行时的数据流

```
┌──────────────┐     JSON 序列化      ┌──────────────┐
│  编辑器模式   │ ──────────────────▶ │  运行时模式   │
│ SkillTimeline │    ExportToJson()   │ SkillTimeline │
└──────────────┘                      └──────────────┘
       │                                     │
       │ 用户编辑                             │ 加载配置
       ▼                                     ▼
┌──────────────┐                      ┌──────────────┐
│ TimelineView │                      │ SkillRunner  │
│ TrackListView│                      │              │
└──────────────┘                      └──────────────┘
                                              │
                                              ▼
                                       ┌──────────────┐
                                       │ ClipProcessor│
                                       │   执行逻辑    │
                                       └──────────────┘
```

### 4.2 片段执行流程

```csharp
// SkillRunner.Tick() 核心逻辑
foreach (var state in _states)
{
    var clip = state.clip;
    bool isOverlap = (clip.StartTime < nextTime) && (clip.EndTime > prevTime);
    
    if (isOverlap)
    {
        if (!state.isRunning)
        {
            state.processor.OnEnter(context);  // 进入片段
            state.isRunning = true;
        }
        
        state.processor.OnUpdate(context, progress);  // 更新进度
        state.processor.OnTick(context, clipLocalTime, clipPrevLocalTime);  // 帧同步逻辑
        
        if (clip.EndTime <= nextTime)
        {
            state.processor.OnExit(context);  // 退出片段
            state.isRunning = false;
        }
    }
}
```

### 4.3 编辑器预览流程

```
用户拖拽时间轴 ──▶ OnTimeIndicatorChanged 事件
                          │
                          ▼
               SkillEditorWindow.OnPreviewTimeChanged()
                          │
                          ▼
                   SkillRunner.EvaluateAt(time)
                          │
                          ▼
                   遍历所有 Processor
                          │
                          ▼
                   processor.OnSample() (编辑器预览采样)
```

---

## 五、设计模式使用分析

### 5.1 模板方法模式

**位置**: `Runtime/Data/Base/SkillClip.cs`

```csharp
// 泛型约束确保每个 Clip 关联特定的 Processor
public abstract class SkillClip<TProcessor> : ClipBase 
    where TProcessor : BaseClipProcessor, new()
{
    public override BaseClipProcessor CreateProcessorInternal()
    {
        return new TProcessor();  // 子类自动创建对应处理器
    }
}
```

**优点**: 编译时类型安全，自动关联 Clip 和 Processor

### 5.2 策略模式

**位置**: `Runtime/Logic/Base/BaseClipProcessor.cs`

不同类型的片段通过不同的 Processor 实现不同的执行策略：
- `AnimationClipProcessor`: 播放动画
- `VFXClipProcessor`: 生成特效
- `DamageClipProcessor`: 执行伤害判定

### 5.3 工厂模式

**位置**: `Editor/Drawers/Base/TrackDrawer.cs`

```csharp
public static class DrawerFactory
{
    public static TrackDrawer CreateDrawer(TrackBase track)
    {
        if (track is VFXTrack) return new VFXTrackDrawer();
        if (track is AnimationTrack) return new AnimationTrackDrawer();
        return new DefaultTrackDrawer();
    }
}
```

### 5.4 观察者模式

**位置**: `Editor/Core/SkillEditorEvents.cs`

```csharp
public class SkillEditorEvents
{
    public Action OnSelectionChanged;
    public Action<float> OnTimeIndicatorChanged;
    public Action OnRepaintRequest;
    
    public void NotifySelectionChanged()
    {
        OnSelectionChanged?.Invoke();
        OnRepaintRequest?.Invoke();
    }
}
```

### 5.5 适配器模式

**位置**: `Editor/TrackObjectWrapper.cs`

将普通 C# 对象适配为 ScriptableObject，以便在 Unity Inspector 中显示：

```csharp
public class ClipObject : ScriptableObject
{
    [HideInInspector] public ClipBase clipData;
    [HideInInspector] public SkillTimeline timeline;
}
```

### 5.6 外观模式

**位置**: `Runtime/System/SkillRunner.cs`

为复杂的片段执行系统提供简单的统一接口：
- `ManualUpdate()`: 手动更新
- `EvaluateAt()`: 跳帧预览
- `Initialize()`: 初始化所有处理器

---

## 六、架构问题分析

### 6.1 严重问题

#### 问题 1: TimelineView 类过于庞大 (2540+ 行)

**位置**: `Editor/SkillEditor/Views/TimelineView.cs`

**问题描述**:
- 单一类承担了绘制、交互、吸附、拖拽、复制粘贴等多种职责
- 违反单一职责原则 (SRP)
- 难以维护和测试

**建议**: 拆分为多个专职类
```
TimelineView (协调者)
├── TimelineRenderer (绘制逻辑)
├── ClipInteractionHandler (片段交互)
├── SnapManager (吸附系统)
├── SelectionManager (选择系统)
└── ClipboardManager (复制粘贴)
```

#### 问题 2: GetAllClips() 方法存在 Bug

**位置**: `Editor/SkillEditor/SkillEditorWindow.cs`

```csharp
private List<ClipBase> GetAllClips()
{
    var list = new List<ClipBase>();
    foreach (var t in state.currentTimeline.tracks)
    {
        if (!t.isEnabled) continue;
        foreach (var c in t.clips)
        {
            if (c.isEnabled)
            {
                list.AddRange(t.clips);  // Bug: 应该只添加 c，而不是整个列表
            }
        }
    }
    return list;
}
```

**修复建议**:
```csharp
if (c.isEnabled)
{
    list.Add(c);  // 只添加当前启用的片段
}
```

#### 问题 3: EditorAnimationService 未完整实现

**位置**: `SkillEditor/Editor/Services/EditorAnimationService.cs`

```csharp
public void Play(UnityEngine.AnimationClip clip, float transitionDuration)
{
    // TODO: 完整实现 Editor 下的 PlayableGraph 预览
    Debug.Log($"[EditorAnimationService] Play {clip.name}");
}

public void Evaluate(float time)
{
    // if (!_graph.IsValid()) return;
    // _graph.Evaluate(time);  // 被注释掉了
}
```

**影响**: 编辑器预览模式下动画无法正确采样

### 6.2 中等问题

#### 问题 4: 缺少 Track-Clip 类型约束

**问题描述**: 当前设计中，任何类型的 Clip 都可以添加到任何类型的 Track，这可能导致运行时错误。

**当前实现**:
```csharp
// TrackBase.cs
public T AddClip<T>(float startTime) where T : ClipBase, new()
{
    T clip = new T();  // 没有类型检查
    clips.Add(clip);
    return clip;
}
```

**建议**: 添加类型约束机制
```csharp
// 建议添加 TrackTypeAttribute
[TrackType(typeof(AnimationClip))]
public class AnimationTrack : TrackBase { }

// 在 AddClip 时验证
public void AddClip(ClipBase clip)
{
    var allowedTypes = GetAllowedClipTypes();
    if (!allowedTypes.Contains(clip.GetType()))
        throw new InvalidOperationException("Clip type not compatible with track");
}
```

#### 问题 5: 服务注册机制不够健壮

**位置**: `Runtime/System/SkillRunner.cs` 中的 ClipContext

**问题描述**: 服务通过字典存储，但没有生命周期管理和服务依赖解析。

```csharp
public T GetService<T>() where T : class
{
    if (_services.TryGetValue(typeof(T), out object service))
        return service as T;
    Debug.LogError($"Service {typeof(T).Name} not found!");
    return null;
}
```

**建议**: 引入服务定位器模式或依赖注入框架

#### 问题 6: 序列化系统缺少版本控制

**位置**: `Editor/Core/SerializationUtility.cs`

**问题描述**: JSON 序列化没有版本号管理，未来数据结构变更可能导致旧文件无法加载。

**建议**:
```csharp
[Serializable]
public class SkillTimelineVersion
{
    public string version = "1.0";
    public int formatVersion = 1;  // 用于迁移逻辑
}

// 导入时检查版本并迁移
public static SkillTimeline ImportFromJson(string path)
{
    var timeline = ...;
    if (timeline.formatVersion < CURRENT_VERSION)
        MigrateData(timeline);
    return timeline;
}
```

### 6.3 轻微问题

#### 问题 7: 硬编码的魔术数字

**位置**: 多处

```csharp
// TimelineView.cs
private const float TIME_RULER_HEIGHT = 30f;
private const float TRACK_HEIGHT = 40f;
private const float GROUP_HEIGHT = 30f;

// TrackListView.cs
private const float TRACK_HEIGHT = 40f;  // 重复定义
```

**建议**: 提取到统一的配置类
```csharp
public static class SkillEditorConfig
{
    public const float TIME_RULER_HEIGHT = 30f;
    public const float TRACK_HEIGHT = 40f;
    public const float GROUP_HEIGHT = 30f;
}
```

#### 问题 8: 缺少单元测试

**问题描述**: 整个项目没有发现测试代码，关键逻辑如吸附计算、时间转换等缺少测试覆盖。

#### 问题 9: 注释语言不一致

**问题描述**: 部分代码使用英文注释，部分使用中文，不符合规范要求。

```csharp
// ClipBase.cs - 混合注释
/// <summary>
/// 片段基类     // 中文
/// </summary>
public abstract class ClipBase : ISkillClipData
{
    // Legacy / Blending support  // 英文
}
```

---

## 七、改进建议

### 7.1 短期改进 (1-2 周)

| 优先级 | 任务 | 说明 |
|--------|------|------|
| P0 | 修复 GetAllClips() Bug | 高优先级，影响功能正确性 |
| P0 | 完善 EditorAnimationService | 实现完整的 PlayableGraph 预览 |
| P1 | 统一配置常量 | 创建 SkillEditorConfig 类 |
| P1 | 添加关键单元测试 | 覆盖时间转换、吸附计算 |

### 7.2 中期改进 (1-2 月)

| 优先级 | 任务 | 说明 |
|--------|------|------|
| P1 | 重构 TimelineView | 拆分为多个专职类 |
| P1 | 添加 Track-Clip 类型约束 | 编译时类型安全 |
| P2 | 实现序列化版本控制 | 支持数据迁移 |
| P2 | 完善服务层 | 添加服务生命周期管理 |

### 7.3 长期改进 (3+ 月)

| 优先级 | 任务 | 说明 |
|--------|------|------|
| P2 | 引入依赖注入框架 | 如 Zenject/VContainer |
| P2 | 实现 Undo/Redo 系统 | 基于命令模式 |
| P3 | 添加扩展机制 | 支持自定义 Track/Clip 类型插件 |
| P3 | 性能优化 | 大量片段时的渲染优化 |

---

## 八、架构优势总结

| 优势 | 说明 |
|------|------|
| **清晰的分层架构** | 数据层、运行时层、编辑器层职责分明 |
| **良好的扩展性** | 通过继承 TrackBase/ClipBase 可轻松添加新类型 |
| **编辑器-运行时分离** | 编辑器代码不会打包到最终产品 |
| **服务抽象** | IAnimationService/IVFXService 支持不同环境实现 |
| **事件驱动** | SkillEditorEvents 实现了视图间的松耦合 |
| **模板方法模式** | SkillClip<TProcessor> 实现编译时类型安全 |

---

## 九、总体评价

SkillEditor 是一个功能完整的技能编辑器系统，采用了合理的分层架构和多种设计模式。核心设计理念（Clip-Track-Timeline 结构、Processor 策略模式、服务抽象）是正确的。

### 主要问题

1. **TimelineView 过于庞大** - 违反单一职责原则
2. **GetAllClips() Bug** - 影响功能正确性
3. **EditorAnimationService 未完成** - 编辑器预览受限
4. **缺少类型约束** - Track-Clip 组合无校验
5. **缺少测试覆盖** - 关键逻辑无保障

### 改进方向

通过上述改进建议，可以显著提升代码质量和可维护性。建议优先处理 P0 级别的问题，然后逐步进行架构重构。

---

**分析日期**: 2026-02-14
