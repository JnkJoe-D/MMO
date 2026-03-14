using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.GraphTools;

namespace Game.AI
{
    [CreateAssetMenu(fileName = "BehaviorTreeGraph", menuName = "AI/Behavior Tree Graph")]
    /// <summary>
    /// 行为树图资产，保存作者态节点、边、黑板以及编译缓存。
    /// </summary>
    public sealed class BehaviorTreeGraphAsset : GraphAssetBase
    {
        [SerializeReference]
        public BehaviorTreeDefinition CompiledDefinition = new BehaviorTreeDefinition();

        public IEnumerable<BehaviorTreeNodeModelBase> BehaviorNodes => GetNodes<BehaviorTreeNodeModelBase>();
        public IEnumerable<BehaviorTreeChildEdgeModel> ChildEdges => GetEdges<BehaviorTreeChildEdgeModel>();
        public IEnumerable<BehaviorTreeBlackboardEntry> BlackboardEntries => Blackboard.OfType<BehaviorTreeBlackboardEntry>();
        public BehaviorTreeRootNodeModel RootNode => GetNodes<BehaviorTreeRootNodeModel>().FirstOrDefault();

        /// <summary>
        /// 确保图中至少存在一个根节点。
        /// </summary>
        /// <returns>现有或新建的根节点。</returns>
        public BehaviorTreeRootNodeModel EnsureRootNode()
        {
            if (RootNode != null)
            {
                return RootNode;
            }

            BehaviorTreeRootNodeModel rootNode = new BehaviorTreeRootNodeModel
            {
                Position = new Vector2(120f, 120f)
            };

            Nodes.Add(rootNode);
            return rootNode;
        }

        /// <summary>
        /// 编译当前图资产为运行时定义。
        /// </summary>
        /// <returns>编译结果报告。</returns>
        public GraphCompileReport CompileDefinition()
        {
            SynchronizeTypedValues();
            BehaviorTreeGraphCompiler compiler = new BehaviorTreeGraphCompiler();
            return compiler.Compile(this);
        }

        /// <summary>
        /// 根据当前图资产创建一个运行时行为树实例。
        /// </summary>
        /// <param name="owner">运行时 owner。</param>
        /// <param name="runtimeBindings">运行时绑定表。</param>
        /// <param name="blackboard">可选的外部黑板。</param>
        /// <param name="forceRecompile">是否强制重新编译。</param>
        /// <returns>新的行为树实例。</returns>
        public BehaviorTreeInstance CreateInstance(
            object owner = null,
            IBehaviorTreeRuntimeBindings runtimeBindings = null,
            BehaviorTreeBlackboard blackboard = null,
            bool forceRecompile = false)
        {
            if (forceRecompile || !HasCompiledDefinition())
            {
                CompileDefinition();
            }

            return new BehaviorTreeInstance(CompiledDefinition, owner, runtimeBindings, blackboard);
        }

        /// <summary>
        /// 同步黑板默认值和条件常量的强类型数据。
        /// </summary>
        public void SynchronizeTypedValues()
        {
            Dictionary<string, BehaviorTreeBlackboardEntry> blackboardByKey = BlackboardEntries
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Key))
                .ToDictionary(entry => entry.Key, entry => entry);

            foreach (BehaviorTreeBlackboardEntry entry in blackboardByKey.Values)
            {
                entry.DefaultValueData ??= BehaviorTreeValueData.CreateDefault(entry.ValueType);
                if (entry.DefaultValueData.ValueType != entry.ValueType)
                {
                    entry.DefaultValueData.SetValueType(entry.ValueType, true);
                }

                entry.SerializedTypeName = entry.ValueType.ToString();
            }

            foreach (BehaviorTreeConditionNodeModel node in BehaviorNodes.OfType<BehaviorTreeConditionNodeModel>())
            {
                BehaviorTreeBlackboardValueType valueType = BehaviorTreeBlackboardValueType.String;
                if (!string.IsNullOrWhiteSpace(node.BlackboardKey) &&
                    blackboardByKey.TryGetValue(node.BlackboardKey, out BehaviorTreeBlackboardEntry entry))
                {
                    valueType = entry.ValueType;
                }
                else if (node.ExpectedValueData != null)
                {
                    valueType = node.ExpectedValueData.ValueType;
                }

                node.ExpectedValueData ??= BehaviorTreeValueData.CreateDefault(valueType);
                if (node.ExpectedValueData.ValueType != valueType)
                {
                    node.ExpectedValueData.SetValueType(valueType, true);
                }
            }
        }

        /// <summary>
        /// 获取某个父节点下的有序子边集合。
        /// </summary>
        /// <param name="parentNodeId">父节点 ID。</param>
        /// <returns>按 ChildIndex 排序后的子边。</returns>
        public IEnumerable<BehaviorTreeChildEdgeModel> GetOrderedChildEdges(string parentNodeId)
        {
            if (string.IsNullOrWhiteSpace(parentNodeId))
            {
                return Enumerable.Empty<BehaviorTreeChildEdgeModel>();
            }

            return ChildEdges
                .Where(edge => edge != null && edge.OutputNodeId == parentNodeId)
                .OrderBy(edge => edge.ChildIndex)
                .ThenBy(edge => edge.SortOrder)
                .ThenBy(edge => edge.EdgeId);
        }

        /// <summary>
        /// 判断一条子边是否还能按方向移动。
        /// </summary>
        /// <param name="parentNodeId">父节点 ID。</param>
        /// <param name="edgeId">子边 ID。</param>
        /// <param name="direction">移动方向，-1 上移，1 下移。</param>
        /// <returns>是否可以移动。</returns>
        public bool CanMoveChildEdge(string parentNodeId, string edgeId, int direction)
        {
            List<BehaviorTreeChildEdgeModel> orderedEdges = GetOrderedChildEdges(parentNodeId).ToList();
            int edgeIndex = orderedEdges.FindIndex(edge => edge.EdgeId == edgeId);
            if (edgeIndex < 0)
            {
                return false;
            }

            int targetIndex = edgeIndex + direction;
            return targetIndex >= 0 && targetIndex < orderedEdges.Count;
        }

        /// <summary>
        /// 按相对方向移动一条子边。
        /// </summary>
        /// <param name="parentNodeId">父节点 ID。</param>
        /// <param name="edgeId">要移动的边 ID。</param>
        /// <param name="direction">移动方向，-1 上移，1 下移。</param>
        public void MoveChildEdge(string parentNodeId, string edgeId, int direction)
        {
            List<BehaviorTreeChildEdgeModel> orderedEdges = GetOrderedChildEdges(parentNodeId).ToList();
            int edgeIndex = orderedEdges.FindIndex(edge => edge.EdgeId == edgeId);
            if (edgeIndex < 0)
            {
                return;
            }

            int targetIndex = edgeIndex + direction;
            if (targetIndex < 0 || targetIndex >= orderedEdges.Count)
            {
                return;
            }

            BehaviorTreeChildEdgeModel edgeToMove = orderedEdges[edgeIndex];
            orderedEdges.RemoveAt(edgeIndex);
            orderedEdges.Insert(targetIndex, edgeToMove);

            for (int index = 0; index < orderedEdges.Count; index++)
            {
                orderedEdges[index].ChildIndex = index;
                orderedEdges[index].SortOrder = index;
            }
        }

        /// <summary>
        /// 把一条子边移动到指定索引。
        /// </summary>
        /// <param name="parentNodeId">父节点 ID。</param>
        /// <param name="edgeId">要移动的边 ID。</param>
        /// <param name="targetIndex">目标索引。</param>
        public void MoveChildEdgeToIndex(string parentNodeId, string edgeId, int targetIndex)
        {
            List<BehaviorTreeChildEdgeModel> orderedEdges = GetOrderedChildEdges(parentNodeId).ToList();
            int edgeIndex = orderedEdges.FindIndex(edge => edge.EdgeId == edgeId);
            if (edgeIndex < 0 || targetIndex < 0 || targetIndex >= orderedEdges.Count)
            {
                return;
            }

            BehaviorTreeChildEdgeModel edgeToMove = orderedEdges[edgeIndex];
            orderedEdges.RemoveAt(edgeIndex);
            orderedEdges.Insert(targetIndex, edgeToMove);

            for (int index = 0; index < orderedEdges.Count; index++)
            {
                orderedEdges[index].ChildIndex = index;
                orderedEdges[index].SortOrder = index;
            }
        }

        /// <summary>
        /// 删除父节点下的一条子分支。
        /// </summary>
        /// <param name="parentNodeId">父节点 ID。</param>
        /// <param name="edgeId">要删除的边 ID。</param>
        public void RemoveChildBranch(string parentNodeId, string edgeId)
        {
            BehaviorTreeChildEdgeModel edge = ChildEdges
                .FirstOrDefault(item => item != null && item.OutputNodeId == parentNodeId && item.EdgeId == edgeId);
            if (edge == null)
            {
                return;
            }

            Edges.Remove(edge);

            bool hasOtherParents = ChildEdges.Any(item => item != null && item.InputNodeId == edge.InputNodeId);
            if (!hasOtherParents)
            {
                RemoveNodeBranch(edge.InputNodeId);
            }

            NormalizeChildOrder(parentNodeId);
        }

        /// <summary>
        /// 递归移除一个节点及其无主子树。
        /// </summary>
        /// <param name="nodeId">要删除的节点 ID。</param>
        private void RemoveNodeBranch(string nodeId)
        {
            BehaviorTreeNodeModelBase node = FindNode<BehaviorTreeNodeModelBase>(nodeId);
            if (node == null)
            {
                return;
            }

            List<BehaviorTreeChildEdgeModel> outgoingEdges = GetOrderedChildEdges(nodeId).ToList();
            foreach (BehaviorTreeChildEdgeModel edge in outgoingEdges)
            {
                Edges.Remove(edge);

                bool hasOtherParents = ChildEdges.Any(item => item != null && item.InputNodeId == edge.InputNodeId);
                if (!hasOtherParents)
                {
                    RemoveNodeBranch(edge.InputNodeId);
                }
            }

            Nodes.Remove(node);
            Edges.RemoveAll(edge => edge != null && (edge.InputNodeId == nodeId || edge.OutputNodeId == nodeId));
        }

        /// <summary>
        /// 规范化父节点下所有子边的顺序号。
        /// </summary>
        /// <param name="parentNodeId">父节点 ID。</param>
        private void NormalizeChildOrder(string parentNodeId)
        {
            List<BehaviorTreeChildEdgeModel> orderedEdges = GetOrderedChildEdges(parentNodeId).ToList();
            for (int index = 0; index < orderedEdges.Count; index++)
            {
                orderedEdges[index].ChildIndex = index;
                orderedEdges[index].SortOrder = index;
            }
        }

        /// <summary>
        /// 判断当前是否已有可用的编译缓存。
        /// </summary>
        /// <returns>是否存在有效编译结果。</returns>
        private bool HasCompiledDefinition()
        {
            return CompiledDefinition != null &&
                   !string.IsNullOrWhiteSpace(CompiledDefinition.RootNodeId) &&
                   CompiledDefinition.Nodes != null &&
                   CompiledDefinition.Nodes.Count > 0;
        }
    }
}
