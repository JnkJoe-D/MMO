---
description: 对 SkillEditor 进行全面的架构分析与评估，按 编辑器/运行时 × Data/View/Logic 维度划分，分多步输出独立分析报告至 SkillEditor/docs 目录
---

# SkillEditor 全面分析与评估 Workflow

> **输出目录**: `Assets/SkillEditor/docs/`
> **项目根目录**: `Assets/SkillEditor/`
> **外部适配器**: `Assets/GameClient/Adapters/` (Skill 相关)

---

## 前置准备

// turbo-all

1. **确认目录结构**
   ```
   Assets/SkillEditor/
   ├── Editor/           # 编辑器侧代码
   │   ├── Core/         # 核心状态与注册表
   │   ├── Drawers/      # Inspector 绘制器(Base + Impl)
   │   ├── Enums/        # 编辑器枚举
   │   ├── Language/     # 多语言系统
   │   ├── Playback/     # 编辑器预览播放(含 Processes/)
   │   ├── Resources/    # 编辑器资源(预览角色)
   │   ├── Views/        # 视图组件(Timeline/TrackList/Toolbar)
   │   ├── SkillEditorWindow.cs
   │   ├── SkillEditorSettingsWindow.cs
   │   └── TrackObjectWrapper.cs
   ├── Runtime/          # 运行时代码
   │   ├── Attributes/   # 自定义特性
   │   ├── Data/         # 数据模型(ClipBase/TrackBase/Clips/Tracks)
   │   ├── Enums/        # 运行时枚举
   │   ├── Playback/     # 运行时播放(Core/Interfaces/Lifecycle/Processes)
   │   ├── Sample/       # 示例实现(CharSkillActor)
   │   └── Serialization/ # 序列化工具
   ├── Settings/         # 配置资源(SkillTagConfig.asset)
   ├── Test/             # 测试脚本
   └── docs/             # 分析报告输出目录(忽略已有内容)
   ```

2. **确认外部适配器文件**
   - `Assets/GameClient/Adapters/GameSkillAudioHandler.cs`
   - `Assets/GameClient/Adapters/SkillServiceFactory.cs`
   - `Assets/GameClient/Adapters/SkillProjectile.cs`
   - `Assets/GameClient/Adapters/SkillSpawnHandler.cs`

---

## 分析步骤总览

本 workflow 将分析工作拆分为 **8 个独立报告**，每个报告聚焦一个维度。分析时须逐一执行，确保每份报告内容完整、详尽后再进入下一步骤。

| 步骤 | 报告文件名 | 维度 | 聚焦范围 |
|:----:|:----------|:----:|:---------|
| 1 | `01_runtime_data_analysis.md` | 运行时 × Data | 数据模型与序列化 |
| 2 | `02_runtime_logic_analysis.md` | 运行时 × Logic | 播放核心与处理器 |
| 3 | `03_runtime_interfaces_analysis.md` | 运行时 × 接口 | 接口定义与适配器实现 |
| 4 | `04_editor_data_analysis.md` | 编辑器 × Data | 编辑器状态与数据包装 |
| 5 | `05_editor_view_analysis.md` | 编辑器 × View | 窗口/视图/Inspector 绘制 |
| 6 | `06_editor_logic_analysis.md` | 编辑器 × Logic | 编辑器预览播放与处理器 |
| 7 | `07_track_clip_impl_analysis.md` | 跨维度 | 各轨道/片段具体实现 |
| 8 | `08_architecture_dataflow_analysis.md` | 整体 | 整体架构与数据流 |

---

## 步骤 1：运行时 Data 层分析

**输出文件**: `docs/01_runtime_data_analysis.md`

### 需要阅读的文件

| 文件 | 说明 |
|:-----|:-----|
| `Runtime/Data/ClipBase.cs` | 片段基类 |
| `Runtime/Data/TrackBase.cs` | 轨道基类 |
| `Runtime/Data/Group.cs` | 分组数据 |
| `Runtime/Data/SkillTimeline.cs` | 时间线核心数据 |
| `Runtime/Data/SkillEnums.cs` | 数据层枚举 |
| `Runtime/Data/SkillTagConfig.cs` | 标签配置 |
| `Runtime/Data/ISkillClipData.cs` | 片段数据接口 |
| `Runtime/Data/Clips/*.cs` | 所有9个具体 Clip 实现 |
| `Runtime/Data/Tracks/*.cs` | 所有8个具体 Track 实现 |
| `Runtime/Enums/RuntimeEnums.cs` | 运行时枚举定义 |
| `Runtime/Attributes/SkillAttributes.cs` | 自定义特性定义 |
| `Runtime/Serialization/SerializationUtility.cs` | 序列化工具 |
| `Settings/SkillTagConfig.asset` | 标签配置资产 |

### 分析要点

1. **核心数据架构**
   - `SkillTimeline` 如何组织 Track 和 Group
   - `TrackBase` 与 `ClipBase` 的继承层次与序列化策略
   - `Group` 的分组管理机制

2. **基类设计**
   - `ClipBase` 的字段定义（startTime, duration 等核心属性）
   - `TrackBase` 的字段定义（clips 集合、trackType/displayName 等）
   - 继承关系：具体 Track/Clip → TrackBase/ClipBase

3. **ISkillClipData 接口**
   - 接口职责与实现类

4. **自定义特性**
   - `SkillAttributes.cs` 中定义的 Attribute（如 TrackColor、TrackIcon 等）
   - 特性在编辑器侧的消费方式

5. **序列化机制**
   - `SerializationUtility.cs` 的 JSON 序列化/反序列化策略
   - 多态类型处理方式（$type 字段等）
   - 编辑器 ↔ 运行时序列化一致性

6. **配置系统**
   - `SkillTagConfig` 的设计（ScriptableObject vs JSON）
   - 标签在伤害检测/生成系统中的使用

7. **枚举定义**
   - `SkillEnums.cs` 和 `RuntimeEnums.cs` 中所有枚举的语义

---

## 步骤 2：运行时 Logic 层分析

**输出文件**: `docs/02_runtime_logic_analysis.md`

### 需要阅读的文件

| 文件 | 说明 |
|:-----|:-----|
| `Runtime/Playback/Core/SkillRunner.cs` | 运行时播放控制器 |
| `Runtime/Playback/Core/ProcessContext.cs` | 处理器上下文 |
| `Runtime/Playback/Core/ProcessFactory.cs` | 处理器工厂 |
| `Runtime/Playback/Core/ProcessBase.cs` | 处理器基类 |
| `Runtime/Playback/Core/IProcess.cs` | 处理器接口 |
| `Runtime/Playback/Core/ProcessBindingAttribute.cs` | 处理器绑定特性 |
| `Runtime/Playback/Lifecycle/SkillLifecycleManager.cs` | 生命周期管理器 |
| `Runtime/Playback/VFXPoolManager.cs` | VFX 对象池管理器 |
| `Runtime/Playback/Processes/*.cs` | 所有8个运行时处理器 |
| `Runtime/Sample/CharSkillActor.cs` | 示例 Actor 实现 |

### 分析要点

1. **SkillRunner 核心流程**
   - 初始化 → 播放 → Tick → 暂停/恢复 → 结束的完整生命周期
   - 时间推进与片段激活/停用逻辑
   - 多轨道并行处理机制

2. **Process 系统架构**
   - `IProcess` 接口定义（Enter/Tick/Exit/Cleanup）
   - `ProcessBase` 基类的通用实现
   - `ProcessBindingAttribute` 如何将 Process 绑定到 Clip 类型
   - `ProcessFactory` 的反射发现与实例化机制

3. **ProcessContext 设计**
   - 上下文传递了哪些信息（Actor、ServiceFactory 等）
   - 依赖注入模式

4. **生命周期管理**
   - `SkillLifecycleManager` 如何管理多个 SkillRunner 的生命周期
   - 技能打断/排队/叠加策略

5. **VFX 对象池**
   - `VFXPoolManager` 的池化策略（预热/回收/上限）
   - 编辑器预览与运行时共享还是独立

6. **各运行时处理器详解**
   - `RuntimeAnimationProcess` → 动画播放
   - `RuntimeAudioProcess` → 音频播放
   - `RuntimeVFXProcess` → 特效管理
   - `RuntimeDamageProcess` → 伤害检测与执行
   - `RuntimeSpawnProcess` → 实体生成
   - `RuntimeEventProcess` → 自定义事件
   - `CameraProcess` → 镜头控制
   - `MovementProcess` → 角色位移

7. **数据流**
   - SkillTimeline → SkillRunner.Init → ProcessFactory → Process 实例
   - 每帧 Tick 中 SkillRunner → 各 Process 的调度流程

---

## 步骤 3：运行时接口与适配器分析

**输出文件**: `docs/03_runtime_interfaces_analysis.md`

### 需要阅读的文件

| 文件 | 说明 |
|:-----|:-----|
| `Runtime/Playback/Interfaces/ISkillActor.cs` | 技能执行者接口 |
| `Runtime/Playback/Interfaces/ISkillAnimationHandler.cs` | 动画处理接口 |
| `Runtime/Playback/Interfaces/ISkillAudioHandler.cs` | 音频处理接口 |
| `Runtime/Playback/Interfaces/ISkillDamageHandler.cs` | 伤害处理接口 |
| `Runtime/Playback/Interfaces/ISkillEventHandler.cs` | 事件处理接口 |
| `Runtime/Playback/Interfaces/ISkillSpawnHandler.cs` | 生成处理接口 |
| `Runtime/Playback/Interfaces/ISkillProjectile.cs` | 弹射物接口 |
| `Runtime/Playback/Interfaces/IServiceFactory.cs` | 服务工厂接口 |
| `Assets/GameClient/Adapters/GameSkillAudioHandler.cs` | 音频适配器 |
| `Assets/GameClient/Adapters/SkillServiceFactory.cs` | 服务工厂适配器 |
| `Assets/GameClient/Adapters/SkillProjectile.cs` | 弹射物适配器 |
| `Assets/GameClient/Adapters/SkillSpawnHandler.cs` | 生成处理适配器 |
| `Runtime/Sample/CharSkillActor.cs` | 示例 Actor |

### 分析要点

1. **接口层设计**
   - 每个接口定义的方法签名与语义
   - 接口间的依赖关系（如 ISkillSpawnHandler 依赖 ISkillProjectile）
   - DIP（依赖倒置原则）的体现

2. **IServiceFactory 模式**
   - 工厂接口如何实现服务定位
   - 运行时依赖解析策略

3. **适配器实现**
   - 每个 GameClient 适配器如何桥接 SkillEditor 接口与游戏逻辑
   - 适配器中的具体实现细节（对象池、碰撞检测等）
   - ISP（接口隔离原则）的遵守程度

4. **数据安全模式**
   - DamageData / SpawnData 等值类型结构体的使用
   - 防止外部修改内部状态的策略

---

## 步骤 4：编辑器 Data 层分析

**输出文件**: `docs/04_editor_data_analysis.md`

### 需要阅读的文件

| 文件 | 说明 |
|:-----|:-----|
| `Editor/Core/SkillEditorState.cs` | 编辑器全局状态 |
| `Editor/Core/SkillEditorEvents.cs` | 编辑器事件系统 |
| `Editor/Core/TrackRegistry.cs` | 轨道类型注册表 |
| `Editor/TrackObjectWrapper.cs` | 轨道对象包装器 |
| `Editor/Enums/EditorEnums.cs` | 编辑器枚举 |
| `Editor/Language/ILanguages.cs` | 语言接口 |
| `Editor/Language/Lan.cs` | 语言管理器 |
| `Editor/Language/LanCHS.cs` | 中文语言包 |
| `Editor/Language/LanEN.cs` | 英文语言包 |
| `Editor/Drawers/CustomDrawerAttribute.cs` | 自定义 Drawer 特性 |

### 分析要点

1. **SkillEditorState 核心状态**
   - 编辑器全局状态管理（当前 Timeline、选中对象、播放状态等）
   - 状态的存取方式（静态 vs 单例 vs 实例）
   - 状态变更通知机制

2. **事件系统**
   - `SkillEditorEvents` 定义了哪些事件
   - 事件的发布/订阅模式
   - 编辑器各组件间的通信方式

3. **TrackRegistry 注册表**
   - 轨道类型的注册与发现机制
   - 是否使用反射/特性自动注册
   - 注册表如何关联 Track → Drawer → Process

4. **TrackObjectWrapper**
   - SerializedObject 包装器的设计
   - 编辑器侧对象编辑的封装策略
   - 与 Unity SerializedProperty 的交互

5. **多语言系统**
   - `ILanguages` 接口与实现
   - 语言切换机制
   - 字符串键值管理方式

6. **CustomDrawerAttribute**
   - Drawer 发现与绑定机制
   - 与 TrackRegistry 的关系

---

## 步骤 5：编辑器 View 层分析

**输出文件**: `docs/05_editor_view_analysis.md`

### 需要阅读的文件

| 文件 | 说明 |
|:-----|:-----|
| `Editor/SkillEditorWindow.cs` | 主编辑器窗口 |
| `Editor/SkillEditorSettingsWindow.cs` | 设置窗口 |
| `Editor/Views/TimelineView.cs` | 时间线视图（核心） |
| `Editor/Views/TimelineClipInteraction.cs` | 时间线片段交互 |
| `Editor/Views/TimelineClipOperations.cs` | 时间线片段操作 |
| `Editor/Views/TimelineCoordinates.cs` | 时间线坐标系统 |
| `Editor/Views/TrackListView.cs` | 轨道列表视图 |
| `Editor/Views/ToolbarView.cs` | 工具栏视图 |
| `Editor/Drawers/Base/SkillInspectorBase.cs` | Inspector 基类 |
| `Editor/Drawers/Base/ClipDrawer.cs` | 片段 Drawer 基类 |
| `Editor/Drawers/Base/TrackDrawer.cs` | 轨道 Drawer 基类 |
| `Editor/Drawers/Impl/*.cs` | 所有7个具体 Drawer 实现 |

### 分析要点

1. **SkillEditorWindow 主窗口**
   - EditorWindow 的生命周期管理
   - 窗口布局（左侧轨道列表 + 右侧时间线 + 工具栏 + Inspector）
   - OnGUI / OnEnable / OnDisable 的关键流程
   - 数据加载/保存流程

2. **TimelineView 时间线视图**
   - 时间刻度绘制
   - 片段的可视化渲染（位置、颜色、标签、选中态）
   - 分组折叠/展开的视觉处理
   - 滚动与缩放

3. **TimelineClipInteraction 交互系统**
   - 片段的选择、拖拽、缩放交互
   - 右键菜单
   - 多选与框选

4. **TimelineClipOperations 操作系统**
   - 片段的添加/删除/复制/粘贴
   - Undo/Redo 支持
   - 对齐与吸附

5. **TimelineCoordinates 坐标系统**
   - 时间 ↔ 像素坐标的互转
   - 缩放系数管理
   - 可见区域计算

6. **TrackListView 轨道列表**
   - 轨道/分组的树形展示
   - 拖拽排序
   - 轨道添加/删除的 UI

7. **ToolbarView 工具栏**
   - 播放控制按钮
   - 时间显示/输入
   - 文件操作（新建/打开/保存）

8. **Inspector 绘制**
   - `SkillInspectorBase` 的反射驱动字段绘制
   - `ClipDrawer` / `TrackDrawer` 基类的扩展点
   - 各具体 Drawer 的特化字段渲染
   - SerializedObject 的自动更新机制

9. **设置窗口**
   - `SkillEditorSettingsWindow` 提供哪些配置项
   - 配置的持久化方式（EditorPrefs / ScriptableObject）

---

## 步骤 6：编辑器 Logic 层分析

**输出文件**: `docs/06_editor_logic_analysis.md`

### 需要阅读的文件

| 文件 | 说明 |
|:-----|:-----|
| `Editor/Playback/SkillEditorWindow.Preview.cs` | 预览控制器（partial class） |
| `Editor/Playback/EditorAudioManager.cs` | 编辑器音频管理 |
| `Editor/Playback/EditorVFXManager.cs` | 编辑器 VFX 管理 |
| `Editor/Playback/Processes/EditorAnimationProcess.cs` | 编辑器动画处理器 |
| `Editor/Playback/Processes/EditorAudioProcess.cs` | 编辑器音频处理器 |
| `Editor/Playback/Processes/EditorDamageProcess.cs` | 编辑器伤害处理器 |
| `Editor/Playback/Processes/EditorEventProcess.cs` | 编辑器事件处理器 |
| `Editor/Playback/Processes/EditorSpawnProcess.cs` | 编辑器生成处理器 |
| `Editor/Playback/Processes/EditorVFXProcess.cs` | 编辑器 VFX 处理器 |
| `Editor/TestLayerMaskJson.cs` | LayerMask 测试工具 |

### 分析要点

1. **预览播放系统**
   - `SkillEditorWindow.Preview.cs` 如何驱动编辑器内预览
   - 预览与运行时 SkillRunner 的关系（复用还是独立）
   - EditorApplication.update 的使用
   - 预览时间的精确控制

2. **编辑器专用管理器**
   - `EditorAudioManager` 的音频预览机制（AudioSource 管理）
   - `EditorVFXManager` 的 VFX 预览机制（实例化/销毁/位置跟踪）

3. **编辑器处理器 vs 运行时处理器**
   - 每个编辑器处理器与对应运行时处理器的差异
   - 编辑器处理器如何模拟运行时行为（无 Physics 等限制）
   - Process 继承体系在编辑器/运行时的统一性

4. **关键差异分析**
   - 编辑器音频：使用 Unity Editor 音频 API
   - 编辑器 VFX：直接实例化 Prefab 到 SceneView
   - 编辑器动画：使用 AnimationMode / SampleAnimation
   - 编辑器伤害：仅可视化 Gizmos（不做实际伤害）
   - 编辑器生成/事件：预览标记或日志

---

## 步骤 7：各轨道/片段具体实现分析

**输出文件**: `docs/07_track_clip_impl_analysis.md`

### 需要阅读的文件

所有 `Runtime/Data/Clips/*.cs`、`Runtime/Data/Tracks/*.cs`、对应的 `Editor/Drawers/Impl/*.cs`、
对应的 `Editor/Playback/Processes/Editor*.cs`、对应的 `Runtime/Playback/Processes/Runtime*.cs`

### 分析要点

按轨道类型逐一分析，每种轨道包含以下维度：

#### 7.1 动画轨道 (Animation)
- **Clip 数据**: `SkillAnimationClip` 的字段（animClip、speed、mask、fadeDuration 等）
- **Track 数据**: `AnimationTrack` 的特有配置
- **Drawer**: `AnimationClipDrawer` / `AnimationTrackDrawer` 的自定义字段绘制
- **编辑器预览**: `EditorAnimationProcess` 如何驱动 AnimationMode
- **运行时执行**: `RuntimeAnimationProcess` 如何调用 ISkillAnimationHandler

#### 7.2 音频轨道 (Audio)
- **Clip 数据**: `AudioClip(SkillEditor)` 的字段
- **Track 数据**: `AudioTrack`
- **Drawer**: `AudioClipDrawer`
- **编辑器预览**: `EditorAudioProcess` → `EditorAudioManager`
- **运行时执行**: `RuntimeAudioProcess` → `ISkillAudioHandler`

#### 7.3 VFX 轨道 (Visual Effects)
- **Clip 数据**: `VFXClip` 的字段（prefab、offset/rotation/scale、ISerializationCallbackReceiver）
- **Track 数据**: `VFXTrack`
- **Drawer**: `VFXClipDrawer` / `VFXTrackDrawer`
- **编辑器预览**: `EditorVFXProcess` → `EditorVFXManager`
- **运行时执行**: `RuntimeVFXProcess` → `VFXPoolManager`

#### 7.4 伤害轨道 (Damage)
- **Clip 数据**: `DamageClip` + `HitBoxShape` 的碰撞体定义
- **Track 数据**: `DamageTrack`
- **Drawer**: `DamageClipDrawer`（复杂的碰撞体编辑器）
- **编辑器预览**: `EditorDamageProcess`（Gizmos 绘制）
- **运行时执行**: `RuntimeDamageProcess` → `ISkillDamageHandler`

#### 7.5 生成轨道 (Spawn)
- **Clip 数据**: `SpawnClip` + `SpawnData`
- **Track 数据**: `SpawnTrack`
- **Drawer**: `SpawnClipDrawer`
- **编辑器预览**: `EditorSpawnProcess`
- **运行时执行**: `RuntimeSpawnProcess` → `ISkillSpawnHandler` → `ISkillProjectile`

#### 7.6 事件轨道 (Event)
- **Clip 数据**: `EventClip` 的自定义事件参数
- **Track 数据**: `EventTrack`
- **编辑器预览**: `EditorEventProcess`
- **运行时执行**: `RuntimeEventProcess` → `ISkillEventHandler`

#### 7.7 相机轨道 (Camera)
- **Clip 数据**: `CameraClip`
- **Track 数据**: `CameraTrack`
- **运行时执行**: `CameraProcess`

#### 7.8 移动轨道 (Movement)
- **Clip 数据**: `MovementClip`
- **Track 数据**: `MovementTrack`
- **运行时执行**: `MovementProcess`

---

## 步骤 8：整体架构与数据流分析

**输出文件**: `docs/08_architecture_dataflow_analysis.md`

### 分析内容

本报告基于前 7 份报告的发现，进行整体性总结。

### 分析要点

1. **整体架构图**
   - 分层架构：Data Layer → Logic Layer → View Layer
   - Editor 与 Runtime 的边界线
   - 依赖方向（Runtime 不依赖 Editor）

2. **核心设计模式**
   - 策略模式（IProcess / ProcessBase）
   - 工厂模式（ProcessFactory / IServiceFactory）
   - 观察者模式（SkillEditorEvents）
   - 适配器模式（GameClient Adapters）
   - 对象池模式（VFXPoolManager）
   - 命令/操作模式（TimelineClipOperations）

3. **编辑时数据流**
   ```
   JSON 文件  →  SerializationUtility 反序列化
              →  SkillTimeline (内存模型)
              →  SkillEditorState (编辑器状态)
              →  Views (GUI 渲染)
              →  Drawers (Inspector 绘制)
              ←  用户编辑
              →  SkillTimeline 更新
              →  SerializationUtility 序列化
              →  JSON 文件
   ```

4. **编辑器预览数据流**
   ```
   SkillEditorWindow.Preview
     → SkillEditorState (获取当前 Timeline)
     → 遍历所有 Track/Clip
     → 通过 ProcessFactory 或直接调用 EditorProcess
     → EditorProcess.Enter/Tick/Exit
       → EditorAudioManager / EditorVFXManager
       → AnimationMode.SampleAnimation (动画)
       → Gizmos (伤害可视化)
   ```

5. **运行时数据流**
   ```
   SkillTimeline (从 JSON 反序列化)
     → SkillRunner.Init(timeline, context)
       → ProcessContext 构建 (ISkillActor, IServiceFactory)
       → ProcessFactory.CreateProcesses (反射发现 + ProcessBindingAttribute)
       → 每帧 SkillRunner.Tick(deltaTime)
         → 遍历激活的 Clip
         → Process.Enter / Process.Tick / Process.Exit
           → 通过 Interface 调用适配器 (ISkillAnimationHandler 等)
     → SkillRunner.Stop / SkillLifecycleManager 回收
   ```

6. **序列化数据流**
   ```
   编辑器 SkillTimeline (C# 对象)
     → SerializationUtility.Serialize (Newtonsoft.Json + TypeNameHandling)
     → JSON 文件 (含 $type 多态类型信息)
     → SerializationUtility.Deserialize
     → 运行时 SkillTimeline (C# 对象)
   ```

7. **各子系统数据流**
   - 动画：AnimationClip → SkillAnimationClip → Process → AnimationHandler/AnimationMode
   - 音频：AudioClip → AudioClip(SkillEditor) → Process → AudioHandler/EditorAudioManager
   - VFX：Prefab → VFXClip (ISerializationCallbackReceiver) → Process → VFXPoolManager/EditorVFXManager
   - 伤害：HitBoxShape → DamageClip → DamageData → Process → DamageHandler/Gizmos
   - 生成：SpawnClip → SpawnData → Process → SpawnHandler → Projectile
   - 事件：EventClip → Process → EventHandler
   - 相机/移动：Clip → Process（暂为骨架实现）

8. **架构优缺点评估**
   - SRP 遵守程度
   - OCP 扩展能力
   - DIP 依赖倒置实践
   - ISP 接口隔离情况
   - 序列化安全性
   - 可扩展性与可维护性
   - 编辑器/运行时代码复用度
   - 潜在的改进建议

---

## 执行规则

1. **每步独立**: 每个步骤生成一份独立的分析报告，确保即使单步执行也能产出完整文档
2. **先读后写**: 每步开始时先用 `view_file` 阅读所有相关文件，分析透彻后再撰写报告
3. **客观严谨**: 不做主观美化，如实记录设计优缺点和潜在问题
4. **忽略 docs**: 分析过程中忽略 `SkillEditor/docs/` 中已有的文档内容
5. **代码引用**: 关键设计点必须附带具体的代码引用（文件名 + 行号/代码片段）
6. **图表辅助**: 使用 Mermaid 图表可视化架构、类图、数据流等
7. **中文撰写**: 所有报告使用中文
8. **分步完成**: 单次对话可能无法完成所有步骤，按步骤序号顺序执行，中断后从上次未完成的步骤继续
