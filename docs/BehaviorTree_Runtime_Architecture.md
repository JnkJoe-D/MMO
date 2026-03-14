# 行为树运行时架构与执行流程说明

## 1. 文档目标

这份文档只聚焦当前项目里“行为树运行时”相关的代码，不讲编辑器绘制细节，重点回答 3 个问题：

1. 行为树资产从哪里来，如何编译成运行时定义。
2. 行为树实例每一帧是怎么执行到角色移动/攻击/索敌上的。
3. 当前每个运行时类、每个方法分别负责什么。

本文覆盖的主要文件：

- `Assets/GameClient/AI/BehaviorTree/*.cs`
- `Assets/GameClient/AI/BehaviorTreeCharacterBindings.cs`
- `Assets/GameClient/AI/BehaviorTreeTargeting.cs`
- `Assets/GameClient/AI/BehaviorTreeCharacterAgent.cs`
- `Assets/GameClient/AI/AIInputProvider.cs`
- `Assets/GameClient/AI/BehaviorTreeRuntimePlaytestSpawner.cs`
- `Assets/GameClient/AI/BehaviorTreeRuntimeDebugHud.cs`

## 2. 当前整体链路

### 2.1 从图资产到运行时实例

完整链路如下：

1. `BehaviorTreeGraphAsset` 保存作者态图数据。
2. `BehaviorTreeGraphValidator.Validate()` 校验根节点、边、子节点数量、黑板键引用是否合法。
3. `BehaviorTreeGraphCompiler.Compile()` 把图节点和边编译成 `BehaviorTreeDefinition`。
4. `BehaviorTreeGraphAsset.CreateInstance()` 基于 `BehaviorTreeDefinition` 创建 `BehaviorTreeInstance`。
5. `BehaviorTreeCharacterAgent.TryInitialize()` 为该实例注入：
   - `owner`
   - `BehaviorTreeRuntimeBindings`
   - `BehaviorTreeBlackboard`
   - `IBehaviorTreeTargetProvider`
6. `BehaviorTreeCharacterAgent.Update()` 每帧调用 `BehaviorTreeInstance.Tick(deltaTime)`。
7. `BehaviorTreeInstance` 递归执行 `Root / Composite / Condition / Service / Action`。
8. 绑定层把节点行为转成角色行为：
   - Service 写黑板
   - Action 调用 `BehaviorTreeCharacterFacade`
   - `BehaviorTreeCharacterFacade` 再去驱动 `AIInputProvider`
   - `CharacterEntity` 和其状态机读取 `IInputProvider`，最终移动/攻击/闪避

### 2.2 当前追击测试树的真实执行路径

当前测试资产 `Assets/GameClient/GraphTools/BehaviorTreeGraph.asset` 的核心结构是：

`Root -> Service(character.sync_target, 0.25s) -> Condition(HasTarget == true) -> Action(character.chase_target)`

它的运行逻辑是：

1. `character.sync_target` 每 0.25 秒更新一次黑板的 `HasTarget / TargetPosition / TargetDistance`。
2. `Condition` 读黑板里的 `HasTarget`。
3. 条件成立后进入 `character.chase_target`。
4. `character.chase_target` 每帧直接向 `targetProvider` 取目标，并根据目标位置写移动输入。
5. 目标进入 `TargetStopDistance` 后，`character.chase_target` 返回 `Success`，并清空移动输入。
6. 角色状态机收到“无移动输入”后会转入 `Stop` 或 `Idle`。

### 2.3 这次追击异常的根因

这次实机里“敌人追到玩家旧位置后停住，随后偶尔短暂恢复追击”的根因有 2 层：

#### 根因 1：目标保留逻辑返回了旧的目标坐标

位置在 `BehaviorTreeTargetSelector.TryRetainCurrentTarget()`。

旧逻辑的问题：

- `currentTarget` 缓存了“上一次选中目标时的快照”
- 保留当前目标时，只判断“这个目标还在不在候选集合里”
- 但真正返回时，直接把旧的 `currentTarget` 返回了
- 结果是 `InstanceId` 是同一个目标，但 `Position` 还是旧位置

这会直接导致：

- 敌人追向玩家旧位置
- 到了旧位置后触发 `TargetStopDistance`
- 播放 `stop` 动画
- 之后只有在重新丢失/重新获取目标时才会短暂恢复

本次已修复：

- 保留当前目标时，会返回“候选列表里的最新目标数据”
- 不再继续复用旧快照坐标

#### 根因 2：追击动作成功后，角色会进入停步子状态

位置不在行为树内核，而在角色运动状态机：

- `GroundJogSubState.OnUpdate()` 在没有移动输入时会切到 `GroundStopSubState`
- `GroundStopSubState.OnEnter()` 会播放 `JogStopConfig / DashStopConfig`
- `GroundStopSubState.OnUpdate()` 在锁定时间没过时，即使已经重新有移动输入，也不会立即切回 `Jog`

因此你仍然可能看到：

- 敌人到达停止距离后先进入停步动画
- 玩家刚一移动时，不一定“零延迟”立刻追上

这部分是“当前角色 locomotion 设计”的结果，不是这次追旧点 bug 本身。

### 2.4 当前剩余的已知行为特征

- `character.sync_target` 是 0.25 秒一次，不是每帧；因此 `HasTarget` 等黑板值不是逐帧刷新。
- `character.chase_target` 是每帧直接查目标提供器；所以“动作取目标”和“黑板显示目标”可能存在最多 0.25 秒的感知延迟。
- 目标选择默认带 FOV、阵营、玩家/非玩家筛选；如果这些筛选过严，会看到 AI 断续丢目标。

## 3. 运行时核心数据层

## 3.1 `BehaviorTreeNodeModels.cs`

### 枚举

- `BehaviorTreeNodeKind`
  - 定义节点大类：`Root / Composite / Condition / Service / Action`
- `BehaviorTreeCompositeMode`
  - 定义组合节点策略：`Sequence / Selector / Parallel`
- `BehaviorTreeAbortMode`
  - 条件节点的中断策略：`None / Self / LowerPriority / Both`
- `BehaviorTreeComparisonOperator`
  - 条件比较方式：`IsSet / Equals / NotEquals / GreaterOrEqual / LessOrEqual`
- `BehaviorTreeConditionValueSource`
  - 条件右值来源：常量或另一个黑板键

### `BehaviorTreeNodeModelBase`

- 作用
  - 所有行为树图节点的作者态基类
- 字段
  - `Description`：作者态描述
  - `NodeKind`：由子类决定

### `BehaviorTreeRootNodeModel`

- 作用
  - 作者态根节点
- 方法
  - 构造函数：默认把标题设为 `Root`

### `BehaviorTreeCompositeNodeModel`

- 作用
  - 作者态组合节点
- 字段
  - `CompositeMode`
- 方法
  - 构造函数：默认标题为 `Composite`

### `BehaviorTreeConditionNodeModel`

- 作用
  - 作者态条件节点
- 字段
  - `BlackboardKey`
  - `Comparison`
  - `ExpectedValueSource`
  - `ExpectedBlackboardKey`
  - `ExpectedValue`
  - `ExpectedValueData`
  - `AbortMode`
- 方法
  - 构造函数：默认标题为 `Condition`
  - `OnBeforeSerialize()`
    - 将 typed value 同步为旧兼容字符串
  - `OnAfterDeserialize()`
    - 从旧兼容字符串恢复 typed value

### `BehaviorTreeServiceNodeModel`

- 作用
  - 作者态服务节点
- 字段
  - `ServiceKey`
  - `IntervalSeconds`
- 方法
  - 构造函数：默认标题为 `Service`

### `BehaviorTreeActionNodeModel`

- 作用
  - 作者态动作节点
- 字段
  - `TaskKey`
- 方法
  - 构造函数：默认标题为 `Action`

## 3.2 `BehaviorTreeChildEdgeModel.cs`

### `BehaviorTreeChildEdgeModel`

- 作用
  - 作者态父子边模型
- 字段
  - `ChildIndex`
    - 决定父节点子节点顺序

## 3.3 `BehaviorTreeBlackboardEntry.cs`

### `BehaviorTreeBlackboardValueType`

- 作用
  - 黑板值类型枚举：`Bool / Int / Float / String`

### `BehaviorTreeBlackboardEntry`

- 作用
  - 作者态和运行时定义共用的黑板条目结构
- 字段
  - `ValueType`
  - `DefaultValue`
  - `DefaultValueData`
- 方法
  - `OnBeforeSerialize()`
    - 确保 typed value 存在，并回写兼容字符串
  - `OnAfterDeserialize()`
    - 反序列化时恢复 typed value，并让 `ValueType` 和实际载荷一致

## 3.4 `BehaviorTreeValueData.cs`

### `BehaviorTreeValueData`

- 作用
  - 统一表示强类型常量和默认值
- 字段
  - `ValueType`
  - `BoolValue`
  - `IntValue`
  - `FloatValue`
  - `StringValue`
- 方法
  - `CreateDefault(valueType)`
    - 创建指定类型的默认载荷
  - `FromLegacyString(valueType, legacyValue)`
    - 从旧字符串格式恢复 typed value
  - `Clone()`
    - 深拷贝
  - `ToObject()`
    - 转为运行时对象
  - `ToLegacyString()`
    - 转为旧兼容字符串
  - `ToDisplayString()`
    - 给调试 UI 用的人类可读文本
  - `SetValueType(valueType, convertCurrentValue)`
    - 改变值类型，可选择是否尝试转换旧值
  - `SetFromObject(rawValue)`
    - 按当前 `ValueType` 把对象写进 typed value
  - `TryConvertToBool / TryConvertToInt / TryConvertToFloat`
    - 内部类型转换工具

## 3.5 `BehaviorTreeDefinition.cs`

### `BehaviorTreeDefinition`

- 作用
  - 运行时真正执行的整棵树定义
- 字段
  - `RootNodeId`
  - `Nodes`
  - `Blackboard`

### `BehaviorTreeDefinitionNode`

- 作用
  - 编译后的单个运行时节点定义
- 字段
  - 节点结构信息：`NodeId / ParentNodeId / ChildIndex / Title / NodeKind / Children`
  - 节点行为信息：`CompositeMode / AbortMode / Comparison`
  - 条件信息：`BlackboardKey / ExpectedValueSource / ExpectedBlackboardKey / ExpectedValue / ExpectedValueData`
  - 服务信息：`ServiceKey / IntervalSeconds`
  - 动作信息：`TaskKey`
- 方法
  - `OnBeforeSerialize()`
    - 把 typed expected value 写回旧兼容字符串
  - `OnAfterDeserialize()`
    - 从旧兼容字符串恢复 typed expected value

## 4. 图资产校验与编译

## 4.1 `BehaviorTreeGraphValidator.cs`

### `Validate(graphAsset)`

- 作用
  - 在编译前验证图结构和基本语义
- 主要检查项
  - 是否有且仅有一个 root
  - 边端点是否存在
  - 非 root 节点是否有多个父节点
  - Root / Condition / Service 的子节点数量是否合法
  - Action 节点是否错误拥有子节点
  - 条件节点黑板键是否合法
  - 条件节点对另一个黑板键比较时，该键是否存在
  - Service 的 `IntervalSeconds` 是否大于 0
  - Action / Service 的键是否为空

## 4.2 `BehaviorTreeGraphCompiler.cs`

### `Compile(graphAsset)`

- 作用
  - 把作者态图转换为运行时 `BehaviorTreeDefinition`
- 主要步骤
  - 调用 `BehaviorTreeGraphValidator.Validate()`
  - 按 `OutputNodeId` 对边分组
  - 重建 `parentByNodeId`
  - 拷贝黑板定义
  - 遍历所有行为树节点，生成 `BehaviorTreeDefinitionNode`
  - 写入 `ParentNodeId / ChildIndex / Children`
  - 根据节点类型写入 `CompositeMode / Condition / Service / Action` 数据
  - 把结果回写到 `graphAsset.CompiledDefinition`

### `ResolveExpectedValue(graphAsset, conditionNode)`

- 作用
  - 计算条件节点编译后的 `ExpectedValueData`
- 行为
  - 优先使用 typed value
  - 若节点仍有旧字符串，则从旧字符串恢复
  - 若引用黑板键存在，则把 expected value 的类型同步到该黑板键类型

## 4.3 `BehaviorTreeGraphAsset.cs`

### 核心属性

- `CompiledDefinition`
  - 编译产物缓存
- `BehaviorNodes`
  - 图里的所有行为树节点
- `ChildEdges`
  - 图里的所有子边
- `BlackboardEntries`
  - 图里的黑板条目
- `RootNode`
  - 当前 root 节点

### 方法

- `EnsureRootNode()`
  - 没有 root 时自动创建一个
- `CompileDefinition()`
  - 先同步 typed value，再调用编译器
- `CreateInstance(owner, runtimeBindings, blackboard, forceRecompile)`
  - 根据当前编译产物创建 `BehaviorTreeInstance`
- `SynchronizeTypedValues()`
  - 保证黑板默认值和条件常量的 typed value 与类型一致
- `GetOrderedChildEdges(parentNodeId)`
  - 返回指定父节点的有序子边
- `CanMoveChildEdge(parentNodeId, edgeId, direction)`
  - 查询边是否还能上移或下移
- `MoveChildEdge(parentNodeId, edgeId, direction)`
  - 按相对方向调整子顺序
- `MoveChildEdgeToIndex(parentNodeId, edgeId, targetIndex)`
  - 直接移动到指定索引
- `RemoveChildBranch(parentNodeId, edgeId)`
  - 删除某条子分支；若子树不再被任何父节点引用，则递归删整棵子树
- `RemoveNodeBranch(nodeId)`
  - 递归删除节点及其无主子树
- `NormalizeChildOrder(parentNodeId)`
  - 在删边或移动后重排 `ChildIndex`
- `HasCompiledDefinition()`
  - 判断编译缓存是否可用

## 5. 黑板运行时

## 5.1 `BehaviorTreeBlackboard.cs`

### `BehaviorTreeBlackboardChange`

- 作用
  - 表示一次黑板值变更
- 字段
  - `Key`
  - `OldValue`
  - `NewValue`
  - `ValueType`

### `BehaviorTreeBlackboard`

- 作用
  - 行为树运行时黑板容器
- 内部数据
  - `definitions`
    - 黑板键定义
  - `values`
    - 黑板运行时实际值
- 事件
  - `ValueChanged`

### 方法

- 构造函数
  - 空构造：创建空黑板
  - `BehaviorTreeBlackboard(entries)`：按给定定义初始化
- `Initialize(entries)`
  - 清空现有定义和值，再按新定义初始化
- `RegisterEntries(entries, preserveExistingValues)`
  - 批量注册黑板条目
- `RegisterEntry(entry, preserveExistingValue)`
  - 注册单个黑板条目
- `ResetToDefaults()`
  - 全部值恢复为定义里的默认值
- `ResetValue(key)`
  - 单个键恢复默认值
- `Contains(key)`
  - 判断键是否存在
- `TryGetDefinition(key, out entry)`
  - 获取某个键的定义副本
- `GetRegisteredValueType(key)`
  - 获取某个键声明的类型
- `TryGetRawValue(key, out value)`
  - 获取原始对象值
- `GetRawValueOrDefault(key, defaultValue)`
  - 获取原始对象值，失败返回默认值
- `TryGetValueData(key, out valueData)`
  - 获取 typed value 副本
- `TryGetValue<T>(key, out value)`
  - 获取强类型值
- `GetValueOrDefault<T>(key, defaultValue)`
  - 获取强类型值，失败返回默认值
- `SetValue(key, value)`
  - 写入一个值，并按定义类型归一化
- `SetValueInternal(key, value, raiseEvent)`
  - 内部设置实现；只在值真正变化时触发事件
- `NormalizeValue(key, value)`
  - 按黑板定义类型进行归一化
- `CloneEntry(entry)`
  - 克隆黑板定义
- `InferValueType(value)`
  - 当定义不存在时，按对象类型推断值类型
- `GetDefaultValue(valueType)`
  - 获取某种类型的默认值
- `TryConvertValue(sourceValue, targetType, out convertedValue)`
  - 通用强转工具
- `TryConvertToBool / TryConvertToInt / TryConvertToFloat`
  - 内部转换工具
- `ValuesEqual(left, right)`
  - 值相等判断，浮点数做容差比较

## 6. 行为树执行内核

## 6.1 `BehaviorTreeRuntime.cs`

### 枚举与接口

- `BehaviorTreeNodeStatus`
  - `Success / Failure / Running`
- `BehaviorTreeNodeStopReason`
  - `Completed / Aborted / Reset`
- `IBehaviorTreeActionHandler`
  - 动作节点生命周期接口：`OnEnter / Tick / OnExit`
- `IBehaviorTreeServiceHandler`
  - 服务节点生命周期接口：`OnEnter / Tick / OnExit`
- `IBehaviorTreeRuntimeBindings`
  - 行为树运行时与具体业务行为之间的抽象桥

### `BehaviorTreeRuntimeBindings`

- 作用
  - 管理 `TaskKey -> 动作处理器` 和 `ServiceKey -> 服务处理器`

### 方法

- `RegisterAction(key, handler)`
  - 注册简单委托动作
- `RegisterActionHandler(key, handler)`
  - 注册完整生命周期动作处理器
- `UnregisterAction(key)`
  - 注销动作
- `RegisterService(key, handler)`
  - 注册简单委托服务
- `RegisterServiceHandler(key, handler)`
  - 注册完整生命周期服务处理器
- `UnregisterService(key)`
  - 注销服务
- `EnterAction(context, node)`
  - 调用动作处理器的 `OnEnter`
- `TickAction(context, node)`
  - 调用动作处理器的 `Tick`
- `ExitAction(context, node, lastStatus, stopReason)`
  - 调用动作处理器的 `OnExit`
- `EnterService(context, node)`
  - 调用服务处理器的 `OnEnter`
- `TickService(context, node)`
  - 调用服务处理器的 `Tick`
- `ExitService(context, node, stopReason)`
  - 调用服务处理器的 `OnExit`

### `BehaviorTreeExecutionContext`

- 作用
  - 执行上下文；封装 owner、blackboard、tick 时间、当前节点、节点内存

### 方法

- 构造函数
  - 绑定 owner 和 blackboard
- `TryGetOwner<T>(out owner)`
  - 取出强类型 owner
- `TryGetBlackboardValue<T>(key, out value)`
  - 读黑板
- `SetBlackboardValue(key, value)`
  - 写黑板
- `GetOrCreateNodeMemory<T>(node)`
  - 获取或创建某个节点的用户态内存
- `TryGetNodeMemory<T>(node, out memory)`
  - 尝试获取节点用户内存
- `ClearNodeMemory(node)`
  - 清理节点用户内存

### `BehaviorTreeNodeRuntimeSnapshot`

- 作用
  - 给调试系统读的节点运行态快照
- 字段
  - `NodeId`
  - `LastStatus`
  - `IsRunning`
  - `ActiveChildIndex`
  - `ActiveDuration`
  - `ServiceElapsedTime`
  - `LastVisitedTick`

### `BehaviorTreeInstance`

- 作用
  - 运行中的一棵行为树

### 构造与公开方法

- 构造函数
  - 建立 `nodesById`
  - 建立父子关系表
  - 建立深度表
  - 准备 blackboard
  - 订阅黑板变更事件
- `Tick(deltaTime)`
  - 每帧入口；推进时间并从根节点开始递归求值
- `Stop()`
  - 停止所有运行中的节点，并重置当前树状态
- `Reset(resetBlackboard)`
  - 停树、清空运行态，可选重置黑板
- `IsNodeRunning(nodeId)`
  - 查询某节点是否在运行
- `TryGetNodeState(nodeId, out snapshot)`
  - 读取单个节点快照
- `GetNodeStates()`
  - 枚举全部节点快照
- `GetOrCreateNodeMemory<T>(nodeId)`
  - 按节点 ID 获取/创建用户内存
- `TryGetNodeMemory<T>(nodeId, out memory)`
  - 按节点 ID 取节点内存
- `ClearNodeMemory(nodeId)`
  - 按节点 ID 清理内存
- `Dispose()`
  - 停止运行并解绑事件

### 主要内部方法

- `CreateSnapshot(nodeId, state)`
  - 生成调试快照
- `HandleBlackboardValueChanged(change)`
  - 转发黑板值变化事件
- `EvaluateNode(node)`
  - 所有节点求值的总入口
- `EvaluateRoot(node)`
  - 执行 root 的唯一子节点
- `EvaluateComposite(node)`
  - 根据 `CompositeMode` 分发到 `Sequence / Selector / Parallel`
- `EvaluateSequence(node)`
  - 顺序执行子节点；遇 `Failure` 立即失败，遇 `Running` 停在当前子节点
- `EvaluateSelector(node)`
  - 按优先级挑一个可运行子节点；支持高优先级条件抢占
- `ResolveSelectorStartIndex(children, activeIndex)`
  - 处理 `LowerPriority / Both` 抢占策略
- `EvaluateParallel(node)`
  - 并行跑全部子节点；任意失败则失败，只要有运行中则返回 `Running`
- `EvaluateCondition(node)`
  - 条件成立则执行子节点，否则失败
- `EvaluateService(node)`
  - 维护服务生命周期和间隔触发，再执行唯一子节点
- `EvaluateAction(node)`
  - 维护动作生命周期并执行动作
- `EvaluateConditionPredicate(node)`
  - 真正的条件比较逻辑
- `IsSet(actualValue)`
  - 条件 `IsSet` 的判断
- `TryResolveExpectedValue(node, out expectedValue)`
  - 解析条件右值：常量或黑板键
- `AreValuesEqual(actualValue, expectedValue)`
  - `Equals / NotEquals` 用的等值比较
- `TryCompareNumeric(actualValue, expectedValue, out comparison)`
  - 数值比较
- `TryConvertToDouble / TryConvertToInt / TryConvertToFloat`
  - 数值转换工具
- `FinalizeExitedNodes()`
  - 本帧没有再运行的节点会在这里统一退出
- `PromoteRunningSet()`
  - 把“本帧运行集合”提升为“下一帧的上帧运行集合”
- `StopRunningNodes(stopReason)`
  - 停掉所有运行中的节点
- `StopNode(node, state, stopReason)`
  - 按节点类型停止节点
- `StopActionNode(node, state, lastStatus, stopReason)`
  - 动作节点退出
- `StopServiceNode(node, state, stopReason)`
  - 服务节点退出
- `GetRuntimeState(nodeId)`
  - 获取或创建内部运行态
- `TryGetRuntimeState(nodeId, out state)`
  - 尝试获取内部运行态
- `GetChildren(node)`
  - 枚举某节点子节点
- `TryGetFirstChild(node, out childNode)`
  - 取唯一或第一个子节点
- `TryGetNode(nodeId, out node)`
  - 从字典找节点
- `BuildParentLookup()`
  - 构建父关系表
- `BuildDepthLookup()`
  - 构建深度表
- `CalculateNodeDepth(nodeId)`
  - 计算节点深度
- `GetExitOrder(nodeIds)`
  - 深节点优先退出，避免父节点先退
- `ThrowIfDisposed()`
  - 防止释放后继续使用

### `BehaviorTreeNodeRuntimeState`

- 作用
  - 内部运行态缓存，不对外公开
- 字段
  - `ActiveChildIndex`
  - `ActiveDuration`
  - `ServiceElapsedTime`
  - `HasExecutedService`
  - `IsActionActive`
  - `IsServiceActive`
  - `LastStatus`
  - `LastVisitedTick`
  - `UserMemory`

## 7. 角色绑定层

## 7.1 `BehaviorTreeCharacterBindings.cs`

### 常量类

- `BehaviorTreeCharacterBlackboardKeys`
  - 定义和角色行为有关的黑板键
- `BehaviorTreeCharacterTaskKeys`
  - 定义动作任务键
- `BehaviorTreeCharacterServiceKeys`
  - 定义服务键

### `BehaviorTreeTargetData`

- 作用
  - 运行时目标快照
- 构造函数
  - 接收位置、名字、实例 ID、是否玩家控制、阵营 ID

### `IBehaviorTreeTargetProvider`

- 作用
  - 抽象目标来源
- 方法
  - `TryGetTarget(out targetData)`

### `IBehaviorTreeCharacterFacade`

- 作用
  - 把行为树逻辑和角色系统解耦
- 属性
  - 当前输入、状态、位置、朝向
- 方法
  - `SetMovement`
  - `ClearMovement`
  - `TriggerBasicAttack`
  - `TriggerSpecialAttack`
  - `TriggerUltimate`
  - `TriggerEvade`

### `BehaviorTreePlayerTargetProvider`

- 作用
  - 只把本地玩家当成目标
- 方法
  - 构造函数
  - `TryGetTarget(out targetData)`
    - 若本地玩家存在且不是 owner，则返回玩家信息
  - `ResolveLocalPlayerCharacter()`
    - 兼容 `PlayerManager` 与 `CharcterManager`

### `BehaviorTreeCharacterFacade`

- 作用
  - 行为树和 `CharacterEntity + AIInputProvider` 之间的适配层

### 方法

- 构造函数
  - 绑定 `CharacterEntity` 和 `AIInputProvider`
- `SetMovement(direction, dashHeld)`
  - 写入 AI 输入
- `ClearMovement()`
  - 清空移动与 dash
- `TriggerBasicAttack()`
  - 只有角色处于可普攻状态时才发起
- `TriggerSpecialAttack()`
  - 只有不在技能态且有技能配置时才发起
- `TriggerUltimate()`
  - 只有不在技能态且有大招配置时才发起
- `TriggerEvade()`
  - 只有角色可闪避时才发起

### `BehaviorTreeCharacterBindingsFactory`

- `CreateDefault(facade, targetProvider)`
  - 生成一套默认绑定：
    - `character.sync_state`
    - `character.sync_target`
    - `character.idle`
    - `character.move`
    - `character.chase_target`
    - `character.basic_attack`
    - `character.special_attack`
    - `character.ultimate`
    - `character.evade`

### `SyncCharacterStateServiceHandler`

- 作用
  - 每次触发时把角色状态写进黑板
- 方法
  - 构造函数
  - `OnEnter()`
    - 目前空实现
  - `Tick()`
    - 更新移动、地面/空中、技能态、闪避态、当前状态名等
  - `OnExit()`
    - 目前空实现

### `SyncCharacterTargetServiceHandler`

- 作用
  - 把目标提供器结果写进黑板
- 方法
  - 构造函数
  - `OnEnter()`
    - 目前空实现
  - `Tick()`
    - 无目标时写 `HasTarget = false` 和清零/极大值
    - 有目标时写入位置、距离、名字、实例 ID
  - `OnExit()`
    - 目前空实现

### `CharacterIdleActionHandler`

- 作用
  - 让角色持续保持无移动输入
- 方法
  - 构造函数
  - `OnEnter()`
    - 清空移动
  - `Tick()`
    - 持续清空移动并返回 `Running`
  - `OnExit()`
    - 再次清空移动

### `CharacterMoveActionHandler`

- 作用
  - 从黑板读 `MoveX / MoveY / DashHeld` 并驱动移动
- 方法
  - 构造函数
  - `OnEnter()`
    - 目前空实现
  - `Tick()`
    - 没方向则 `Failure`
    - 有方向则写入移动并返回 `Running`
  - `OnExit()`
    - 清空移动

### `CharacterChaseTargetActionHandler`

- 作用
  - 每帧直接从 `targetProvider` 取目标并朝目标移动
- 方法
  - 构造函数
  - `OnEnter()`
    - 目前空实现
  - `Tick()`
    - 无目标则 `Failure`
    - 在 `TargetStopDistance` 内则清空移动并 `Success`
    - 否则按目标方向写移动并返回 `Running`
  - `OnExit()`
    - 清空移动

### `TriggerCharacterCommandActionHandler`

- 作用
  - 包装“触发一次技能/闪避，再等待状态机进入并退出目标状态”的动作
- 方法
  - 构造函数
  - `OnEnter()`
    - 触发命令，记录是否成功、是否观察到激活态
  - `Tick()`
    - 未成功触发则 `Failure`
    - 若还在目标激活态则 `Running`
    - 若曾进入过激活态且现在已退出，则 `Success`
    - 否则给一个短暂 grace tick
  - `OnExit()`
    - 清理节点内存

## 8. 索敌与目标选择层

## 8.1 `BehaviorTreeTargeting.cs`

### 枚举与结构

- `BehaviorTreeTargetSelectionMode`
  - `LocalPlayerOnly / LocalPlayerPreferred / ClosestCharacter`
- `BehaviorTreeTargetFactionFilter`
  - `Any / SameFaction / DifferentFaction`
- `BehaviorTreeTargetControlFilter`
  - `Any / PlayerOnly / NonPlayerOnly`
- `BehaviorTreeTargetSelectionOptions`
  - 目标筛选参数集合
- `BehaviorTreeTargetMetadata`
  - 注册在角色上的阵营与是否玩家控制信息

### `BehaviorTreeCharacterRegistry`

- 作用
  - 场景里所有 `CharacterEntity` 的注册表
- 方法
  - `Register(character)`
  - `Unregister(character)`
  - `SetMetadata(character, metadata)`
  - `ClearMetadata(character)`
  - `TryGetMetadata(character, out metadata)`

### `BehaviorTreeSceneCharacterTargetProvider`

- 作用
  - 基于场景角色注册表实时选择目标

### 方法

- 构造函数
  - 绑定 owner 和选择参数
- `TryGetTarget(out targetData)`
  - 调用 `BehaviorTreeTargetSelector` 选目标，并缓存当前目标
- `EnumerateCandidates()`
  - 枚举场景里所有候选角色
- `GetOwnerFactionId()`
  - 获取 owner 阵营
- `GetFactionId(character, isPlayerControlled)`
  - 先查注册元数据，查不到时按是否玩家给兜底阵营
- `ResolveLocalPlayerCharacter()`
  - 兼容两套本地玩家入口

### `BehaviorTreeTargetSelector`

- 作用
  - 纯算法层；不依赖 MonoBehaviour 状态，只接收参数和候选列表

### 方法

- `TrySelectTarget(ownerPosition, ownerForward, ownerFactionId, options, candidates, currentTarget, out selectedTarget)`
  - 总入口；先过滤，再尝试保留旧目标，再按模式选新目标
- `TryRetainCurrentTarget(ownerPosition, options, allCandidates, currentTarget, out selectedTarget)`
  - 尝试保留当前目标
  - 本次已修正：保留时返回“候选列表中的最新目标数据”，不再复用旧坐标
- `PassesFilters(...)`
  - 统一筛选：阵营、控制类型、最小距离、FOV
- `SelectClosest(candidates)`
  - 选最近目标
- `SelectLocalPlayer(candidates, fallbackToClosest)`
  - 优先本地玩家
- `ComputeHorizontalSqrDistance(source, target)`
  - 计算水平距离平方
- `IsWithinDistance(sqrDistance, maxDistance)`
  - 最大距离判定
- `IsFiniteDistance(value)`
  - 检查是否是有效有限数
- `PassesMinDistance(sqrDistance, minDistance)`
  - 最小距离判定
- `PassesFieldOfView(ownerPosition, ownerForward, targetPosition, fieldOfViewDegrees)`
  - 视野角判定
- `PassesFactionFilter(ownerFactionId, filter, targetFactionId)`
  - 阵营判定
- `PassesControlFilter(filter, isPlayerControlled)`
  - 玩家/非玩家判定

## 9. 角色代理与输入层

## 9.1 `BehaviorTreeCharacterAgent.cs`

### 作用

- 行为树在角色上的运行时宿主组件
- 负责“拿图、建实例、每帧 tick、绑定目标系统、管理输入代理”

### 属性

- `Character`
  - 当前 `CharacterEntity`
- `InputProvider`
  - 当前 `AIInputProvider`
- `Instance`
  - 当前行为树实例
- `Blackboard`
  - 当前运行时黑板
- `BehaviorTree`
  - 当前行为树资产
- `CurrentCharacterStateName`
  - 便于调试查看角色状态机状态名

### 方法

- `Awake()`
  - 缓存 `CharacterEntity`
  - 获取或自动补 `AIInputProvider`
  - 把 AI 输入挂回角色
  - 注册目标元数据
- `Start()`
  - 若开启自动初始化，则尝试初始化行为树
- `Update()`
  - 若实例还没建出来则继续尝试
  - 实例存在时每帧 `Tick`
- `TryInitialize(graphOverride, targetProvider)`
  - 真正的初始化入口
  - 决定使用哪张树
  - 重置旧实例
  - 构造 `BehaviorTreeCharacterFacade`
  - 构造默认 `BehaviorTreeSceneCharacterTargetProvider`
  - 构造默认绑定
  - 创建行为树实例
- `StopTree(clearInput)`
  - 停树，可选清空输入
- `OnDestroy()`
  - 注销目标元数据并释放实例
- `DisposeInstance()`
  - 内部释放实例并清空输入
- `RegisterTargetMetadata()`
  - 把自己注册到目标系统，包含阵营和玩家控制标记
- `ClearTargetMetadata()`
  - 从目标系统移除自己

## 9.2 `AIInputProvider.cs`

### 作用

- 行为树写入的“AI 输入代理”
- 对外实现 `IInputProvider`，对内只是保存移动与按键状态，并触发对应事件

### 方法

- `GetMovementDirection()`
  - 读取当前移动输入
- `HasMovementInput()`
  - 是否有移动输入
- `GetActionState(type)`
  - 目前只处理 dash 按住状态
- `SetMovementDirection(direction)`
  - 直接设置移动输入
- `SetMovement(direction, isDashHeld)`
  - 同时设置移动和 dash
- `ClearMovement()`
  - 清空移动输入
- `SetDashHeld(isHeld)`
  - 设置 dash 按住状态
- `ResetInputState()`
  - 清空所有输入态
- `ResetState()`
  - `ResetInputState()` 的别名
- `TriggerSwitchNext / TriggerSwitchPre / TriggerEvade / TriggerBasicAttack / TriggerBasicAttackCancel / TriggerBasicAttackHoldStart / TriggerBasicAttackHold / TriggerBasicAttackHoldCancel / TriggerSpecialAttack / TriggerUltimate / TriggerGameplayInteract`
  - 触发对应输入事件，让 `CharacterEntity` 像收到玩家输入一样响应

## 10. 测试与调试组件

## 10.1 `BehaviorTreeRuntimePlaytestSpawner.cs`

### 作用

- 运行时实机测试辅助器
- 等玩家出生后，生成一个 AI 敌人并初始化行为树

### 方法

- `Start()`
  - 若 `spawnOnStart` 打开，则自动开始生成协程
- `SpawnNow()`
  - 手动销毁旧敌人并重新生成
- `SpawnRoutine()`
  - 等待本地玩家
  - 校验 prefab/config/tree
  - 计算生成位置和朝向
  - 实例化敌人
  - 禁用 `LocalPlayerInputProvider`
  - 初始化 `CharacterEntity`
  - 添加或获取 `BehaviorTreeCharacterAgent`
  - 调用 `TryInitialize()`
- `ResolveLocalPlayer()`
  - 兼容两套本地玩家入口
- `DisablePlayerInputProviders(targetObject)`
  - 关闭 prefab 上自带的玩家输入组件，避免和 AI 输入冲突

## 10.2 `BehaviorTreeRuntimeDebugHud.cs`

### 作用

- 运行时蓝色调试 HUD
- 实时显示：
  - 行为树状态
  - 当前节点
  - Tick 数
  - 角色状态
  - 输入值
  - 黑板值
  - 当前运行节点
  - 场景中的目标连线

### 方法

- `Update()`
  - 监听开关键
  - 刷新跟踪中的 agent
  - 绘制世界空间目标线
- `OnGUI()`
  - 绘制蓝色调试面板
- `OnDisable()`
  - 清理运行时生成的面板贴图
- `RefreshNow()`
  - 手动强制刷新 agent 列表
- `RefreshAgents(force)`
  - 刷新当前跟踪的 agent 列表
- `IsAgentUsable(agent)`
  - 判断 agent 是否可显示
- `DrawWorldDebug()`
  - 从黑板读目标位置并画 `Debug.DrawLine`
- `EnsureStyles()`
  - 初始化蓝色 HUD 样式
- `BuildAgentDebugText(agent)`
  - 拼装单个 AI 的调试文字
- `CharacterStateToText(agent, instance)`
  - 写入树状态、当前节点、角色状态、输入等
- `BlackboardToText(blackboard)`
  - 写入黑板值列表
- `RunningNodesToText(instance)`
  - 写入当前运行节点列表
- `ResolveCurrentNodeLabel(instance)`
  - 解析当前节点标题
- `ResolveNodeLabel(instance, nodeId)`
  - 按 `nodeId` 查标题和节点类型
- `FormatValue(value)`
  - 调试文本格式化

## 11. 实机排查建议

### 11.1 当前追击测试建议

如果你要继续用 `BehaviorTreeGraph.asset` 做实机测试，建议优先看这几个指标：

- `HasTarget`
- `TargetDistance`
- `TargetHorizontalDistance`
- `TargetPositionX/Y/Z`
- `CurrentState`
- `Current Node`
- `Running Nodes`

这些值已经可以通过 `BehaviorTreeRuntimeDebugHud` 直接看到。

### 11.2 推荐测试方式

1. 在场景里放一个空物体，挂 `BehaviorTreeRuntimeDebugHud`
2. `Play`
3. 观察蓝色面板
4. 先确认：
   - `Current Node` 是否稳定停在 `Action (Action)` 或 `Condition (Condition)`
   - `HasTarget` 是否稳定
   - `TargetPosition` 是否跟着玩家持续变化
5. 若仍有断续追击，再对照：
   - `CurrentState` 是否切进了 `GroundStopSubState`
   - `HasTarget` 是否因为筛选条件变成 `false`

## 12. 后续建议

### 建议 1

如果这棵树主要用于“持续追击”，可以把 `character.sync_target` 的间隔从 `0.25` 降到更小，甚至直接设为每帧语义的服务。

### 建议 2

若希望“站定后玩家一动就更快重新跟上”，要继续看角色 locomotion 层的：

- `GroundStopSubState`
- `GroundJogSubState`

这不是行为树内核 bug，而是当前角色移动状态机的设计结果。

### 建议 3

后面若要做真正敌人 AI，建议把“追击停止距离”“攻击距离”“攻击冷却”“目标可见性”全部显式黑板化，再通过条件节点控制攻击/追击切换，不要只靠单个 `chase_target` 动作。
