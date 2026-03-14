using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Game.AI
{
    /// <summary>
    /// 节点执行结果。
    /// </summary>
    public enum BehaviorTreeNodeStatus
    {
        Success,
        Failure,
        Running
    }

    /// <summary>
    /// 节点停止原因。
    /// </summary>
    public enum BehaviorTreeNodeStopReason
    {
        Completed,
        Aborted,
        Reset
    }

    /// <summary>
    /// 动作节点生命周期接口。
    /// </summary>
    public interface IBehaviorTreeActionHandler
    {
        void OnEnter(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node);
        BehaviorTreeNodeStatus Tick(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node);
        void OnExit(
            BehaviorTreeExecutionContext context,
            BehaviorTreeDefinitionNode node,
            BehaviorTreeNodeStatus lastStatus,
            BehaviorTreeNodeStopReason stopReason);
    }

    /// <summary>
    /// 服务节点生命周期接口。
    /// </summary>
    public interface IBehaviorTreeServiceHandler
    {
        void OnEnter(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node);
        void Tick(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node);
        void OnExit(
            BehaviorTreeExecutionContext context,
            BehaviorTreeDefinitionNode node,
            BehaviorTreeNodeStopReason stopReason);
    }

    /// <summary>
    /// 运行时绑定接口，负责把节点键映射到真实业务逻辑。
    /// </summary>
    public interface IBehaviorTreeRuntimeBindings
    {
        void EnterAction(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node);
        BehaviorTreeNodeStatus TickAction(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node);
        void ExitAction(
            BehaviorTreeExecutionContext context,
            BehaviorTreeDefinitionNode node,
            BehaviorTreeNodeStatus lastStatus,
            BehaviorTreeNodeStopReason stopReason);

        void EnterService(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node);
        void TickService(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node);
        void ExitService(
            BehaviorTreeExecutionContext context,
            BehaviorTreeDefinitionNode node,
            BehaviorTreeNodeStopReason stopReason);
    }

    /// <summary>
    /// 默认运行时绑定表。
    /// </summary>
    public sealed class BehaviorTreeRuntimeBindings : IBehaviorTreeRuntimeBindings
    {
        private readonly Dictionary<string, Func<BehaviorTreeExecutionContext, BehaviorTreeDefinitionNode, BehaviorTreeNodeStatus>> actionDelegates =
            new Dictionary<string, Func<BehaviorTreeExecutionContext, BehaviorTreeDefinitionNode, BehaviorTreeNodeStatus>>(StringComparer.Ordinal);
        private readonly Dictionary<string, Action<BehaviorTreeExecutionContext, BehaviorTreeDefinitionNode>> serviceDelegates =
            new Dictionary<string, Action<BehaviorTreeExecutionContext, BehaviorTreeDefinitionNode>>(StringComparer.Ordinal);
        private readonly Dictionary<string, IBehaviorTreeActionHandler> actionHandlers =
            new Dictionary<string, IBehaviorTreeActionHandler>(StringComparer.Ordinal);
        private readonly Dictionary<string, IBehaviorTreeServiceHandler> serviceHandlers =
            new Dictionary<string, IBehaviorTreeServiceHandler>(StringComparer.Ordinal);

        /// <summary>注册委托型动作处理器。</summary>
        public void RegisterAction(string key, Func<BehaviorTreeExecutionContext, BehaviorTreeDefinitionNode, BehaviorTreeNodeStatus> handler)
        {
            if (!string.IsNullOrWhiteSpace(key) && handler != null)
            {
                actionDelegates[key] = handler;
            }
        }

        /// <summary>注册完整生命周期动作处理器。</summary>
        public void RegisterActionHandler(string key, IBehaviorTreeActionHandler handler)
        {
            if (!string.IsNullOrWhiteSpace(key) && handler != null)
            {
                actionHandlers[key] = handler;
            }
        }

        /// <summary>注销某个动作处理器。</summary>
        public bool UnregisterAction(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            bool removedDelegate = actionDelegates.Remove(key);
            bool removedHandler = actionHandlers.Remove(key);
            return removedDelegate || removedHandler;
        }

        /// <summary>注册委托型服务处理器。</summary>
        public void RegisterService(string key, Action<BehaviorTreeExecutionContext, BehaviorTreeDefinitionNode> handler)
        {
            if (!string.IsNullOrWhiteSpace(key) && handler != null)
            {
                serviceDelegates[key] = handler;
            }
        }

        /// <summary>注册完整生命周期服务处理器。</summary>
        public void RegisterServiceHandler(string key, IBehaviorTreeServiceHandler handler)
        {
            if (!string.IsNullOrWhiteSpace(key) && handler != null)
            {
                serviceHandlers[key] = handler;
            }
        }

        /// <summary>注销某个服务处理器。</summary>
        public bool UnregisterService(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            bool removedDelegate = serviceDelegates.Remove(key);
            bool removedHandler = serviceHandlers.Remove(key);
            return removedDelegate || removedHandler;
        }

        /// <summary>调用动作处理器的进入逻辑。</summary>
        public void EnterAction(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node)
        {
            if (node != null && !string.IsNullOrWhiteSpace(node.TaskKey) && actionHandlers.TryGetValue(node.TaskKey, out IBehaviorTreeActionHandler handler))
            {
                handler.OnEnter(context, node);
            }
        }

        /// <summary>调用动作处理器的 Tick。</summary>
        public BehaviorTreeNodeStatus TickAction(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.TaskKey))
            {
                return BehaviorTreeNodeStatus.Failure;
            }

            if (actionHandlers.TryGetValue(node.TaskKey, out IBehaviorTreeActionHandler actionHandler))
            {
                return actionHandler.Tick(context, node);
            }

            return actionDelegates.TryGetValue(node.TaskKey, out Func<BehaviorTreeExecutionContext, BehaviorTreeDefinitionNode, BehaviorTreeNodeStatus> actionDelegate)
                ? actionDelegate(context, node)
                : BehaviorTreeNodeStatus.Failure;
        }

        /// <summary>调用动作处理器的退出逻辑。</summary>
        public void ExitAction(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node, BehaviorTreeNodeStatus lastStatus, BehaviorTreeNodeStopReason stopReason)
        {
            if (node != null && !string.IsNullOrWhiteSpace(node.TaskKey) && actionHandlers.TryGetValue(node.TaskKey, out IBehaviorTreeActionHandler handler))
            {
                handler.OnExit(context, node, lastStatus, stopReason);
            }
        }

        /// <summary>调用服务处理器的进入逻辑。</summary>
        public void EnterService(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node)
        {
            if (node != null && !string.IsNullOrWhiteSpace(node.ServiceKey) && serviceHandlers.TryGetValue(node.ServiceKey, out IBehaviorTreeServiceHandler handler))
            {
                handler.OnEnter(context, node);
            }
        }

        /// <summary>调用服务处理器的 Tick。</summary>
        public void TickService(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.ServiceKey))
            {
                return;
            }

            if (serviceHandlers.TryGetValue(node.ServiceKey, out IBehaviorTreeServiceHandler handler))
            {
                handler.Tick(context, node);
            }
            else if (serviceDelegates.TryGetValue(node.ServiceKey, out Action<BehaviorTreeExecutionContext, BehaviorTreeDefinitionNode> serviceDelegate))
            {
                serviceDelegate(context, node);
            }
        }

        /// <summary>调用服务处理器的退出逻辑。</summary>
        public void ExitService(BehaviorTreeExecutionContext context, BehaviorTreeDefinitionNode node, BehaviorTreeNodeStopReason stopReason)
        {
            if (node != null && !string.IsNullOrWhiteSpace(node.ServiceKey) && serviceHandlers.TryGetValue(node.ServiceKey, out IBehaviorTreeServiceHandler handler))
            {
                handler.OnExit(context, node, stopReason);
            }
        }
    }

    /// <summary>
    /// 行为树执行上下文，封装 owner、blackboard 和节点内存访问能力。
    /// </summary>
    public sealed class BehaviorTreeExecutionContext
    {
        /// <summary>
        /// 构造运行时上下文。
        /// </summary>
        /// <param name="owner">行为树 owner。</param>
        /// <param name="blackboard">运行时黑板。</param>
        internal BehaviorTreeExecutionContext(object owner, BehaviorTreeBlackboard blackboard)
        {
            Owner = owner;
            Blackboard = blackboard ?? new BehaviorTreeBlackboard();
        }

        public object Owner { get; }
        public BehaviorTreeInstance Instance { get; internal set; }
        public BehaviorTreeBlackboard Blackboard { get; }
        public float DeltaTime { get; internal set; }
        public float ElapsedTime { get; internal set; }
        public int TickCount { get; internal set; }
        public string CurrentNodeId { get; internal set; } = string.Empty;
        public BehaviorTreeNodeStatus CurrentTreeStatus { get; internal set; } = BehaviorTreeNodeStatus.Failure;

        /// <summary>尝试把 owner 取为指定类型。</summary>
        public bool TryGetOwner<T>(out T owner)
        {
            if (Owner is T typedOwner)
            {
                owner = typedOwner;
                return true;
            }

            owner = default;
            return false;
        }

        /// <summary>尝试读取黑板值。</summary>
        public bool TryGetBlackboardValue<T>(string key, out T value) => Blackboard.TryGetValue(key, out value);
        /// <summary>写入黑板值。</summary>
        public void SetBlackboardValue(string key, object value) => Blackboard.SetValue(key, value);

        /// <summary>获取或创建某个节点的用户内存。</summary>
        public T GetOrCreateNodeMemory<T>(BehaviorTreeDefinitionNode node) where T : class, new()
        {
            return node == null || Instance == null ? null : Instance.GetOrCreateNodeMemory<T>(node.NodeId);
        }

        /// <summary>尝试读取某个节点的用户内存。</summary>
        public bool TryGetNodeMemory<T>(BehaviorTreeDefinitionNode node, out T memory) where T : class
        {
            if (node == null || Instance == null)
            {
                memory = null;
                return false;
            }

            return Instance.TryGetNodeMemory(node.NodeId, out memory);
        }

        /// <summary>清理某个节点的用户内存。</summary>
        public void ClearNodeMemory(BehaviorTreeDefinitionNode node)
        {
            if (node == null || Instance == null)
            {
                return;
            }

            Instance.ClearNodeMemory(node.NodeId);
        }
    }

    /// <summary>
    /// 单个节点的运行态快照，供调试面板读取。
    /// </summary>
    public readonly struct BehaviorTreeNodeRuntimeSnapshot
    {
        /// <summary>
        /// 构造节点运行态快照。
        /// </summary>
        public BehaviorTreeNodeRuntimeSnapshot(
            string nodeId,
            BehaviorTreeNodeStatus lastStatus,
            bool isRunning,
            int activeChildIndex,
            float activeDuration,
            float serviceElapsedTime,
            int lastVisitedTick)
        {
            NodeId = nodeId ?? string.Empty;
            LastStatus = lastStatus;
            IsRunning = isRunning;
            ActiveChildIndex = activeChildIndex;
            ActiveDuration = activeDuration;
            ServiceElapsedTime = serviceElapsedTime;
            LastVisitedTick = lastVisitedTick;
        }

        public string NodeId { get; }
        public BehaviorTreeNodeStatus LastStatus { get; }
        public bool IsRunning { get; }
        public int ActiveChildIndex { get; }
        public float ActiveDuration { get; }
        public float ServiceElapsedTime { get; }
        public int LastVisitedTick { get; }
    }

    /// <summary>
    /// 一棵运行中的行为树实例。
    /// </summary>
    public sealed class BehaviorTreeInstance : IDisposable
    {
        private readonly BehaviorTreeDefinition definition;
        private readonly IBehaviorTreeRuntimeBindings bindings;
        private readonly Dictionary<string, BehaviorTreeDefinitionNode> nodesById;
        private readonly Dictionary<string, string> parentByNodeId =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> nodeDepthById =
            new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, BehaviorTreeNodeRuntimeState> runtimeStates =
            new Dictionary<string, BehaviorTreeNodeRuntimeState>(StringComparer.Ordinal);
        private readonly HashSet<string> runningNodeIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> runningNodeIdsThisTick =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> evaluationStack =
            new HashSet<string>(StringComparer.Ordinal);

        private bool disposed;

        /// <summary>
        /// 构造行为树实例并完成节点索引、黑板和上下文初始化。
        /// </summary>
        public BehaviorTreeInstance(
            BehaviorTreeDefinition treeDefinition,
            object owner = null,
            IBehaviorTreeRuntimeBindings runtimeBindings = null,
            BehaviorTreeBlackboard blackboard = null)
        {
            definition = treeDefinition ?? new BehaviorTreeDefinition();
            bindings = runtimeBindings ?? new BehaviorTreeRuntimeBindings();
            nodesById = definition.Nodes
                .Where(node => node != null && !string.IsNullOrWhiteSpace(node.NodeId))
                .GroupBy(node => node.NodeId)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            BuildParentLookup();
            BuildDepthLookup();

            BehaviorTreeBlackboard resolvedBlackboard = blackboard ?? new BehaviorTreeBlackboard(definition.Blackboard);
            if (blackboard != null)
            {
                resolvedBlackboard.RegisterEntries(definition.Blackboard, preserveExistingValues: true);
            }

            Context = new BehaviorTreeExecutionContext(owner, resolvedBlackboard)
            {
                Instance = this
            };
            Context.Blackboard.ValueChanged += HandleBlackboardValueChanged;
        }

        public event Action<BehaviorTreeBlackboardChange> BlackboardValueChanged;

        public BehaviorTreeExecutionContext Context { get; }
        public BehaviorTreeDefinition Definition => definition;
        public BehaviorTreeNodeStatus CurrentStatus { get; private set; } = BehaviorTreeNodeStatus.Failure;
        public bool IsValid => !string.IsNullOrWhiteSpace(definition.RootNodeId) && nodesById.ContainsKey(definition.RootNodeId);
        public IEnumerable<string> RunningNodeIds => runningNodeIds;

        /// <summary>推进一帧行为树执行。</summary>
        public BehaviorTreeNodeStatus Tick(float deltaTime)
        {
            ThrowIfDisposed();

            Context.DeltaTime = deltaTime < 0f ? 0f : deltaTime;
            Context.ElapsedTime += Context.DeltaTime;
            Context.TickCount++;
            Context.CurrentNodeId = string.Empty;

            runningNodeIdsThisTick.Clear();
            evaluationStack.Clear();

            BehaviorTreeNodeStatus tickStatus = TryGetNode(definition.RootNodeId, out BehaviorTreeDefinitionNode rootNode)
                ? EvaluateNode(rootNode)
                : BehaviorTreeNodeStatus.Failure;

            CurrentStatus = tickStatus;
            Context.CurrentTreeStatus = tickStatus;

            FinalizeExitedNodes();
            PromoteRunningSet();
            return tickStatus;
        }

        /// <summary>停止整棵树并重置当前树状态。</summary>
        public void Stop()
        {
            ThrowIfDisposed();
            StopRunningNodes(BehaviorTreeNodeStopReason.Reset);
            CurrentStatus = BehaviorTreeNodeStatus.Failure;
            Context.CurrentTreeStatus = CurrentStatus;
            Context.CurrentNodeId = string.Empty;
        }

        /// <summary>重置整棵树，可选重置黑板。</summary>
        public void Reset(bool resetBlackboard = true)
        {
            ThrowIfDisposed();
            Stop();
            runtimeStates.Clear();

            if (resetBlackboard)
            {
                Context.Blackboard.Initialize(definition.Blackboard);
            }

            Context.DeltaTime = 0f;
            Context.ElapsedTime = 0f;
            Context.TickCount = 0;
        }

        /// <summary>判断某个节点当前是否处于 Running。</summary>
        public bool IsNodeRunning(string nodeId)
        {
            return !string.IsNullOrWhiteSpace(nodeId) && runningNodeIds.Contains(nodeId);
        }

        /// <summary>尝试读取某个节点的运行态快照。</summary>
        public bool TryGetNodeState(string nodeId, out BehaviorTreeNodeRuntimeSnapshot snapshot)
        {
            if (!TryGetRuntimeState(nodeId, out BehaviorTreeNodeRuntimeState state))
            {
                snapshot = default;
                return false;
            }

            snapshot = CreateSnapshot(nodeId, state);
            return true;
        }

        /// <summary>枚举全部节点的运行态快照。</summary>
        public IEnumerable<BehaviorTreeNodeRuntimeSnapshot> GetNodeStates()
        {
            foreach (KeyValuePair<string, BehaviorTreeNodeRuntimeState> pair in runtimeStates.OrderBy(pair => pair.Key))
            {
                yield return CreateSnapshot(pair.Key, pair.Value);
            }
        }

        /// <summary>按节点 ID 获取或创建用户内存。</summary>
        public T GetOrCreateNodeMemory<T>(string nodeId) where T : class, new()
        {
            BehaviorTreeNodeRuntimeState state = GetRuntimeState(nodeId);
            if (state.UserMemory is T typedMemory)
            {
                return typedMemory;
            }

            T memory = new T();
            state.UserMemory = memory;
            return memory;
        }

        /// <summary>按节点 ID 尝试读取用户内存。</summary>
        public bool TryGetNodeMemory<T>(string nodeId, out T memory) where T : class
        {
            if (TryGetRuntimeState(nodeId, out BehaviorTreeNodeRuntimeState state) && state.UserMemory is T typedMemory)
            {
                memory = typedMemory;
                return true;
            }

            memory = null;
            return false;
        }

        /// <summary>按节点 ID 清理用户内存。</summary>
        public void ClearNodeMemory(string nodeId)
        {
            if (TryGetRuntimeState(nodeId, out BehaviorTreeNodeRuntimeState state))
            {
                state.UserMemory = null;
            }
        }

        /// <summary>释放行为树实例。</summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            StopRunningNodes(BehaviorTreeNodeStopReason.Reset);
            Context.Blackboard.ValueChanged -= HandleBlackboardValueChanged;
            disposed = true;
        }

        /// <summary>根据内部状态构造一个对外快照。</summary>
        private BehaviorTreeNodeRuntimeSnapshot CreateSnapshot(string nodeId, BehaviorTreeNodeRuntimeState state)
        {
            return new BehaviorTreeNodeRuntimeSnapshot(
                nodeId,
                state.LastStatus,
                runningNodeIds.Contains(nodeId),
                state.ActiveChildIndex,
                state.ActiveDuration,
                state.ServiceElapsedTime,
                state.LastVisitedTick);
        }

        /// <summary>转发黑板值变化事件。</summary>
        private void HandleBlackboardValueChanged(BehaviorTreeBlackboardChange change)
        {
            BlackboardValueChanged?.Invoke(change);
        }

        /// <summary>节点求值总入口。</summary>
        private BehaviorTreeNodeStatus EvaluateNode(BehaviorTreeDefinitionNode node)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.NodeId))
            {
                return BehaviorTreeNodeStatus.Failure;
            }

            if (!evaluationStack.Add(node.NodeId))
            {
                return BehaviorTreeNodeStatus.Failure;
            }

            BehaviorTreeNodeRuntimeState state = GetRuntimeState(node.NodeId);
            state.LastVisitedTick = Context.TickCount;
            Context.CurrentNodeId = node.NodeId;

            BehaviorTreeNodeStatus status = node.NodeKind switch
            {
                BehaviorTreeNodeKind.Root => EvaluateRoot(node),
                BehaviorTreeNodeKind.Composite => EvaluateComposite(node),
                BehaviorTreeNodeKind.Condition => EvaluateCondition(node),
                BehaviorTreeNodeKind.Service => EvaluateService(node),
                BehaviorTreeNodeKind.Action => EvaluateAction(node),
                _ => BehaviorTreeNodeStatus.Failure
            };

            state.LastStatus = status;
            state.ActiveDuration = status == BehaviorTreeNodeStatus.Running
                ? state.ActiveDuration + Context.DeltaTime
                : 0f;

            if (status == BehaviorTreeNodeStatus.Running)
            {
                runningNodeIdsThisTick.Add(node.NodeId);
            }
            else if (node.NodeKind is not BehaviorTreeNodeKind.Action and not BehaviorTreeNodeKind.Service)
            {
                state.ActiveChildIndex = -1;
            }

            evaluationStack.Remove(node.NodeId);
            return status;
        }

        /// <summary>执行根节点，直接转发给唯一子节点。</summary>
        private BehaviorTreeNodeStatus EvaluateRoot(BehaviorTreeDefinitionNode node)
        {
            return TryGetFirstChild(node, out BehaviorTreeDefinitionNode childNode)
                ? EvaluateNode(childNode)
                : BehaviorTreeNodeStatus.Failure;
        }

        /// <summary>根据组合模式分发到不同执行策略。</summary>
        private BehaviorTreeNodeStatus EvaluateComposite(BehaviorTreeDefinitionNode node)
        {
            return node.CompositeMode switch
            {
                BehaviorTreeCompositeMode.Selector => EvaluateSelector(node),
                BehaviorTreeCompositeMode.Parallel => EvaluateParallel(node),
                _ => EvaluateSequence(node)
            };
        }

        /// <summary>执行 Sequence：前面全成功才继续，遇失败立刻失败。</summary>
        private BehaviorTreeNodeStatus EvaluateSequence(BehaviorTreeDefinitionNode node)
        {
            List<BehaviorTreeDefinitionNode> children = GetChildren(node).ToList();
            if (children.Count == 0)
            {
                return BehaviorTreeNodeStatus.Failure;
            }

            BehaviorTreeNodeRuntimeState state = GetRuntimeState(node.NodeId);
            int startIndex = state.ActiveChildIndex >= 0 && state.ActiveChildIndex < children.Count
                ? state.ActiveChildIndex
                : 0;

            for (int index = startIndex; index < children.Count; index++)
            {
                BehaviorTreeNodeStatus childStatus = EvaluateNode(children[index]);
                if (childStatus == BehaviorTreeNodeStatus.Running)
                {
                    state.ActiveChildIndex = index;
                    return BehaviorTreeNodeStatus.Running;
                }

                if (childStatus == BehaviorTreeNodeStatus.Failure)
                {
                    state.ActiveChildIndex = -1;
                    return BehaviorTreeNodeStatus.Failure;
                }
            }

            state.ActiveChildIndex = -1;
            return BehaviorTreeNodeStatus.Success;
        }

        /// <summary>执行 Selector：从前到后找第一个成功或运行中的子节点。</summary>
        private BehaviorTreeNodeStatus EvaluateSelector(BehaviorTreeDefinitionNode node)
        {
            List<BehaviorTreeDefinitionNode> children = GetChildren(node).ToList();
            if (children.Count == 0)
            {
                return BehaviorTreeNodeStatus.Failure;
            }

            BehaviorTreeNodeRuntimeState state = GetRuntimeState(node.NodeId);
            int startIndex = state.ActiveChildIndex >= 0 && state.ActiveChildIndex < children.Count
                ? state.ActiveChildIndex
                : 0;

            startIndex = ResolveSelectorStartIndex(children, startIndex);

            for (int index = startIndex; index < children.Count; index++)
            {
                BehaviorTreeNodeStatus childStatus = EvaluateNode(children[index]);
                if (childStatus == BehaviorTreeNodeStatus.Running)
                {
                    state.ActiveChildIndex = index;
                    return BehaviorTreeNodeStatus.Running;
                }

                if (childStatus == BehaviorTreeNodeStatus.Success)
                {
                    state.ActiveChildIndex = -1;
                    return BehaviorTreeNodeStatus.Success;
                }
            }

            state.ActiveChildIndex = -1;
            return BehaviorTreeNodeStatus.Failure;
        }

        /// <summary>处理 Selector 在高优先级条件满足时的重新起点评估。</summary>
        private int ResolveSelectorStartIndex(IReadOnlyList<BehaviorTreeDefinitionNode> children, int activeIndex)
        {
            if (activeIndex <= 0 || activeIndex >= children.Count)
            {
                return activeIndex;
            }

            for (int index = 0; index < activeIndex; index++)
            {
                BehaviorTreeDefinitionNode child = children[index];
                if (child == null || child.NodeKind != BehaviorTreeNodeKind.Condition)
                {
                    continue;
                }

                if (child.AbortMode is not BehaviorTreeAbortMode.LowerPriority and not BehaviorTreeAbortMode.Both)
                {
                    continue;
                }

                if (EvaluateConditionPredicate(child))
                {
                    return index;
                }
            }

            return activeIndex;
        }

        /// <summary>执行 Parallel：任一失败则失败，只要有运行中则返回 Running。</summary>
        private BehaviorTreeNodeStatus EvaluateParallel(BehaviorTreeDefinitionNode node)
        {
            List<BehaviorTreeDefinitionNode> children = GetChildren(node).ToList();
            if (children.Count == 0)
            {
                return BehaviorTreeNodeStatus.Failure;
            }

            bool hasRunningChild = false;
            foreach (BehaviorTreeDefinitionNode child in children)
            {
                BehaviorTreeNodeStatus childStatus = EvaluateNode(child);
                if (childStatus == BehaviorTreeNodeStatus.Failure)
                {
                    return BehaviorTreeNodeStatus.Failure;
                }

                if (childStatus == BehaviorTreeNodeStatus.Running)
                {
                    hasRunningChild = true;
                }
            }

            return hasRunningChild ? BehaviorTreeNodeStatus.Running : BehaviorTreeNodeStatus.Success;
        }

        /// <summary>先判断条件，再执行条件节点的唯一子节点。</summary>
        private BehaviorTreeNodeStatus EvaluateCondition(BehaviorTreeDefinitionNode node)
        {
            if (!EvaluateConditionPredicate(node))
            {
                return BehaviorTreeNodeStatus.Failure;
            }

            return TryGetFirstChild(node, out BehaviorTreeDefinitionNode childNode)
                ? EvaluateNode(childNode)
                : BehaviorTreeNodeStatus.Success;
        }

        /// <summary>维护服务生命周期和间隔 Tick，再执行服务节点子节点。</summary>
        private BehaviorTreeNodeStatus EvaluateService(BehaviorTreeDefinitionNode node)
        {
            BehaviorTreeNodeRuntimeState state = GetRuntimeState(node.NodeId);
            if (!state.IsServiceActive)
            {
                bindings.EnterService(Context, node);
                state.IsServiceActive = true;
                state.ServiceElapsedTime = 0f;
                state.HasExecutedService = false;
            }

            float intervalSeconds = node.IntervalSeconds <= 0f ? 0f : node.IntervalSeconds;
            if (!state.HasExecutedService)
            {
                bindings.TickService(Context, node);
                state.HasExecutedService = true;
                state.ServiceElapsedTime = 0f;
            }
            else if (intervalSeconds <= 0f)
            {
                bindings.TickService(Context, node);
            }
            else
            {
                state.ServiceElapsedTime += Context.DeltaTime;
                while (state.ServiceElapsedTime >= intervalSeconds)
                {
                    bindings.TickService(Context, node);
                    state.ServiceElapsedTime -= intervalSeconds;
                }
            }

            BehaviorTreeNodeStatus childStatus = TryGetFirstChild(node, out BehaviorTreeDefinitionNode childNode)
                ? EvaluateNode(childNode)
                : BehaviorTreeNodeStatus.Failure;

            if (childStatus != BehaviorTreeNodeStatus.Running)
            {
                StopServiceNode(node, state, BehaviorTreeNodeStopReason.Completed);
            }

            return childStatus;
        }

        /// <summary>维护动作生命周期并调用动作处理器。</summary>
        private BehaviorTreeNodeStatus EvaluateAction(BehaviorTreeDefinitionNode node)
        {
            BehaviorTreeNodeRuntimeState state = GetRuntimeState(node.NodeId);
            if (!state.IsActionActive)
            {
                bindings.EnterAction(Context, node);
                state.IsActionActive = true;
            }

            BehaviorTreeNodeStatus status = bindings.TickAction(Context, node);
            if (status != BehaviorTreeNodeStatus.Running)
            {
                StopActionNode(node, state, status, BehaviorTreeNodeStopReason.Completed);
            }

            return status;
        }

        /// <summary>执行条件节点的真实比较逻辑。</summary>
        private bool EvaluateConditionPredicate(BehaviorTreeDefinitionNode node)
        {
            if (string.IsNullOrWhiteSpace(node.BlackboardKey) || !Context.Blackboard.TryGetRawValue(node.BlackboardKey, out object actualValue))
            {
                return false;
            }

            if (!TryResolveExpectedValue(node, out object expectedValue))
            {
                return node.Comparison == BehaviorTreeComparisonOperator.IsSet && IsSet(actualValue);
            }

            return node.Comparison switch
            {
                BehaviorTreeComparisonOperator.IsSet => IsSet(actualValue),
                BehaviorTreeComparisonOperator.Equals => AreValuesEqual(actualValue, expectedValue),
                BehaviorTreeComparisonOperator.NotEquals => !AreValuesEqual(actualValue, expectedValue),
                BehaviorTreeComparisonOperator.GreaterOrEqual => TryCompareNumeric(actualValue, expectedValue, out int comparison) && comparison >= 0,
                BehaviorTreeComparisonOperator.LessOrEqual => TryCompareNumeric(actualValue, expectedValue, out int comparison) && comparison <= 0,
                _ => false
            };
        }

        /// <summary>判断一个值是否可视为“已设置”。</summary>
        private static bool IsSet(object actualValue)
        {
            return actualValue switch
            {
                null => false,
                string stringValue => !string.IsNullOrWhiteSpace(stringValue),
                _ => true
            };
        }

        /// <summary>解析条件节点右值，可来自常量或另一个黑板键。</summary>
        private bool TryResolveExpectedValue(BehaviorTreeDefinitionNode node, out object expectedValue)
        {
            if (node.Comparison == BehaviorTreeComparisonOperator.IsSet)
            {
                expectedValue = null;
                return false;
            }

            if (node.ExpectedValueSource == BehaviorTreeConditionValueSource.BlackboardKey)
            {
                return Context.Blackboard.TryGetRawValue(node.ExpectedBlackboardKey, out expectedValue);
            }

            expectedValue = node.ExpectedValueData?.ToObject();
            return true;
        }

        /// <summary>判断两个值是否相等。</summary>
        private static bool AreValuesEqual(object actualValue, object expectedValue)
        {
            switch (actualValue)
            {
                case null:
                    return expectedValue == null;
                case bool boolValue:
                    return expectedValue is bool expectedBool && boolValue == expectedBool;
                case int intValue:
                    return TryConvertToInt(expectedValue, out int expectedInt) && intValue == expectedInt;
                case float floatValue:
                    return TryConvertToFloat(expectedValue, out float expectedFloat)
                           && Math.Abs(floatValue - expectedFloat) <= 0.0001f;
                default:
                    return string.Equals(actualValue.ToString(), expectedValue?.ToString() ?? string.Empty, StringComparison.Ordinal);
            }
        }

        /// <summary>尝试进行数值比较。</summary>
        private static bool TryCompareNumeric(object actualValue, object expectedValue, out int comparison)
        {
            comparison = 0;

            if (!TryConvertToDouble(actualValue, out double actualNumber) ||
                !TryConvertToDouble(expectedValue, out double expectedNumber))
            {
                return false;
            }

            comparison = actualNumber.CompareTo(expectedNumber);
            return true;
        }

        /// <summary>尝试把对象转换为 double。</summary>
        private static bool TryConvertToDouble(object value, out double number)
        {
            switch (value)
            {
                case int intValue:
                    number = intValue;
                    return true;
                case float floatValue:
                    number = floatValue;
                    return true;
                case double doubleValue:
                    number = doubleValue;
                    return true;
                default:
                    return double.TryParse(
                        value?.ToString(),
                        NumberStyles.Float | NumberStyles.AllowThousands,
                        CultureInfo.InvariantCulture,
                        out number);
            }
        }

        /// <summary>尝试把对象转换为 int。</summary>
        private static bool TryConvertToInt(object value, out int number)
        {
            switch (value)
            {
                case int intValue:
                    number = intValue;
                    return true;
                case float floatValue:
                    number = (int)floatValue;
                    return true;
                default:
                    return int.TryParse(value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number);
            }
        }

        /// <summary>尝试把对象转换为 float。</summary>
        private static bool TryConvertToFloat(object value, out float number)
        {
            switch (value)
            {
                case float floatValue:
                    number = floatValue;
                    return true;
                case int intValue:
                    number = intValue;
                    return true;
                default:
                    return float.TryParse(
                        value?.ToString(),
                        NumberStyles.Float | NumberStyles.AllowThousands,
                        CultureInfo.InvariantCulture,
                        out number);
            }
        }

        /// <summary>统一处理本帧退出运行态的节点。</summary>
        private void FinalizeExitedNodes()
        {
            List<BehaviorTreeDefinitionNode> exitedNodes = GetExitOrder(
                runningNodeIds.Where(nodeId => !runningNodeIdsThisTick.Contains(nodeId)));

            foreach (BehaviorTreeDefinitionNode node in exitedNodes)
            {
                if (!TryGetRuntimeState(node.NodeId, out BehaviorTreeNodeRuntimeState state))
                {
                    continue;
                }

                StopNode(node, state, BehaviorTreeNodeStopReason.Aborted);
            }
        }

        /// <summary>把本帧运行集合提升为下一帧的“上帧运行集合”。</summary>
        private void PromoteRunningSet()
        {
            runningNodeIds.Clear();
            foreach (string nodeId in runningNodeIdsThisTick)
            {
                runningNodeIds.Add(nodeId);
            }

            runningNodeIdsThisTick.Clear();
        }

        /// <summary>停止所有仍处于运行态的节点。</summary>
        private void StopRunningNodes(BehaviorTreeNodeStopReason stopReason)
        {
            List<BehaviorTreeDefinitionNode> activeNodes = GetExitOrder(runningNodeIds);
            foreach (BehaviorTreeDefinitionNode node in activeNodes)
            {
                if (TryGetRuntimeState(node.NodeId, out BehaviorTreeNodeRuntimeState state))
                {
                    StopNode(node, state, stopReason);
                }
            }

            runningNodeIds.Clear();
            runningNodeIdsThisTick.Clear();
            evaluationStack.Clear();
        }

        /// <summary>按节点类型执行统一退出逻辑。</summary>
        private void StopNode(BehaviorTreeDefinitionNode node, BehaviorTreeNodeRuntimeState state, BehaviorTreeNodeStopReason stopReason)
        {
            if (node == null || state == null)
            {
                return;
            }

            switch (node.NodeKind)
            {
                case BehaviorTreeNodeKind.Action:
                    StopActionNode(node, state, state.LastStatus, stopReason);
                    break;
                case BehaviorTreeNodeKind.Service:
                    StopServiceNode(node, state, stopReason);
                    break;
            }

            state.ActiveChildIndex = -1;
            state.ActiveDuration = 0f;
            state.UserMemory = null;
        }

        /// <summary>退出动作节点。</summary>
        private void StopActionNode(
            BehaviorTreeDefinitionNode node,
            BehaviorTreeNodeRuntimeState state,
            BehaviorTreeNodeStatus lastStatus,
            BehaviorTreeNodeStopReason stopReason)
        {
            if (!state.IsActionActive)
            {
                return;
            }

            bindings.ExitAction(Context, node, lastStatus, stopReason);
            state.IsActionActive = false;
            state.ActiveDuration = 0f;
        }

        /// <summary>退出服务节点。</summary>
        private void StopServiceNode(
            BehaviorTreeDefinitionNode node,
            BehaviorTreeNodeRuntimeState state,
            BehaviorTreeNodeStopReason stopReason)
        {
            if (!state.IsServiceActive)
            {
                return;
            }

            bindings.ExitService(Context, node, stopReason);
            state.IsServiceActive = false;
            state.ServiceElapsedTime = 0f;
            state.HasExecutedService = false;
            state.ActiveDuration = 0f;
        }

        /// <summary>获取或创建某个节点的内部运行态。</summary>
        private BehaviorTreeNodeRuntimeState GetRuntimeState(string nodeId)
        {
            if (!runtimeStates.TryGetValue(nodeId, out BehaviorTreeNodeRuntimeState state))
            {
                state = new BehaviorTreeNodeRuntimeState();
                runtimeStates[nodeId] = state;
            }

            return state;
        }

        /// <summary>尝试获取某个节点的内部运行态。</summary>
        private bool TryGetRuntimeState(string nodeId, out BehaviorTreeNodeRuntimeState state)
        {
            return runtimeStates.TryGetValue(nodeId, out state);
        }

        /// <summary>枚举某个节点的全部子节点。</summary>
        private IEnumerable<BehaviorTreeDefinitionNode> GetChildren(BehaviorTreeDefinitionNode node)
        {
            if (node?.Children == null)
            {
                yield break;
            }

            foreach (string childId in node.Children)
            {
                if (TryGetNode(childId, out BehaviorTreeDefinitionNode childNode))
                {
                    yield return childNode;
                }
            }
        }

        /// <summary>尝试获取某个节点的第一个子节点。</summary>
        private bool TryGetFirstChild(BehaviorTreeDefinitionNode node, out BehaviorTreeDefinitionNode childNode)
        {
            childNode = GetChildren(node).FirstOrDefault();
            return childNode != null;
        }

        /// <summary>按节点 ID 查询定义节点。</summary>
        private bool TryGetNode(string nodeId, out BehaviorTreeDefinitionNode node)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                node = null;
                return false;
            }

            return nodesById.TryGetValue(nodeId, out node);
        }

        /// <summary>构建节点到父节点的查找表。</summary>
        private void BuildParentLookup()
        {
            foreach (BehaviorTreeDefinitionNode node in nodesById.Values)
            {
                if (!string.IsNullOrWhiteSpace(node.ParentNodeId))
                {
                    parentByNodeId[node.NodeId] = node.ParentNodeId;
                }
            }

            foreach (BehaviorTreeDefinitionNode node in nodesById.Values)
            {
                if (node.Children == null)
                {
                    continue;
                }

                for (int childIndex = 0; childIndex < node.Children.Count; childIndex++)
                {
                    string childId = node.Children[childIndex];
                    if (!string.IsNullOrWhiteSpace(childId) && !parentByNodeId.ContainsKey(childId))
                    {
                        parentByNodeId[childId] = node.NodeId;
                    }
                }
            }
        }

        /// <summary>构建节点深度缓存，供退出顺序排序使用。</summary>
        private void BuildDepthLookup()
        {
            foreach (string nodeId in nodesById.Keys)
            {
                nodeDepthById[nodeId] = CalculateNodeDepth(nodeId);
            }
        }

        /// <summary>计算某个节点的深度。</summary>
        private int CalculateNodeDepth(string nodeId)
        {
            int depth = 0;
            string currentNodeId = nodeId;
            int safeGuard = nodesById.Count + 1;

            while (safeGuard-- > 0 &&
                   parentByNodeId.TryGetValue(currentNodeId, out string parentNodeId) &&
                   !string.IsNullOrWhiteSpace(parentNodeId))
            {
                depth++;
                currentNodeId = parentNodeId;
            }

            return depth;
        }

        /// <summary>按“子节点先于父节点”生成退出顺序。</summary>
        private List<BehaviorTreeDefinitionNode> GetExitOrder(IEnumerable<string> nodeIds)
        {
            return nodeIds
                .Where(nodeId => TryGetNode(nodeId, out _))
                .Distinct(StringComparer.Ordinal)
                .Select(nodeId => nodesById[nodeId])
                .OrderByDescending(node => nodeDepthById.TryGetValue(node.NodeId, out int depth) ? depth : 0)
                .ToList();
        }

        /// <summary>若实例已释放，则抛出异常。</summary>
        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(BehaviorTreeInstance));
            }
        }

        private sealed class BehaviorTreeNodeRuntimeState
        {
            public int ActiveChildIndex = -1;
            public float ActiveDuration;
            public float ServiceElapsedTime;
            public bool HasExecutedService;
            public bool IsActionActive;
            public bool IsServiceActive;
            public BehaviorTreeNodeStatus LastStatus = BehaviorTreeNodeStatus.Failure;
            public int LastVisitedTick;
            public object UserMemory;
        }
    }
}
