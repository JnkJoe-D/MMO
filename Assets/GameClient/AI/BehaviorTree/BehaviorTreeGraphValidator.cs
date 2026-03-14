using System.Collections.Generic;
using System.Linq;
using Game.GraphTools;

namespace Game.AI
{
    /// <summary>
    /// 行为树图校验器，负责在编译前检查结构是否合法。
    /// </summary>
    public static class BehaviorTreeGraphValidator
    {
        /// <summary>
        /// 校验一张行为树图资产。
        /// </summary>
        /// <param name="graphAsset">要校验的行为树图。</param>
        /// <returns>校验结果。</returns>
        public static GraphValidationResult Validate(BehaviorTreeGraphAsset graphAsset)
        {
            GraphValidationResult result = new GraphValidationResult();
            if (graphAsset == null)
            {
                result.AddError("bt.graph.null", "BehaviorTree graph asset is null.");
                return result;
            }

            List<BehaviorTreeRootNodeModel> roots = graphAsset.GetNodes<BehaviorTreeRootNodeModel>()
                .Where(node => node != null)
                .ToList();

            if (roots.Count == 0)
            {
                result.AddError("bt.root.missing", "BehaviorTree graph is missing a root node.");
            }
            else if (roots.Count > 1)
            {
                result.AddError("bt.root.duplicate", "BehaviorTree graph has more than one root node.");
            }

            Dictionary<string, BehaviorTreeNodeModelBase> nodeMap = graphAsset.BehaviorNodes
                .Where(node => node != null && !string.IsNullOrEmpty(node.NodeId))
                .GroupBy(node => node.NodeId)
                .ToDictionary(group => group.Key, group => group.First());

            Dictionary<string, int> parentCountByNodeId = new Dictionary<string, int>();
            Dictionary<string, List<BehaviorTreeChildEdgeModel>> edgesByParent = new Dictionary<string, List<BehaviorTreeChildEdgeModel>>();

            foreach (BehaviorTreeChildEdgeModel edge in graphAsset.ChildEdges.Where(edge => edge != null))
            {
                if (string.IsNullOrEmpty(edge.OutputNodeId) || string.IsNullOrEmpty(edge.InputNodeId))
                {
                    result.AddError("bt.edge.endpoint.empty", "BehaviorTree edge has an empty endpoint.", edgeId: edge.EdgeId);
                    continue;
                }

                if (!nodeMap.ContainsKey(edge.OutputNodeId))
                {
                    result.AddError("bt.edge.parent.missing", "BehaviorTree edge parent node is missing.", edgeId: edge.EdgeId);
                    continue;
                }

                if (!nodeMap.ContainsKey(edge.InputNodeId))
                {
                    result.AddError("bt.edge.child.missing", "BehaviorTree edge child node is missing.", edgeId: edge.EdgeId);
                    continue;
                }

                if (!edgesByParent.TryGetValue(edge.OutputNodeId, out List<BehaviorTreeChildEdgeModel> childEdges))
                {
                    childEdges = new List<BehaviorTreeChildEdgeModel>();
                    edgesByParent.Add(edge.OutputNodeId, childEdges);
                }

                childEdges.Add(edge);

                if (!parentCountByNodeId.TryAdd(edge.InputNodeId, 1))
                {
                    parentCountByNodeId[edge.InputNodeId]++;
                }
            }

            HashSet<string> blackboardKeys = new HashSet<string>(graphAsset.BlackboardEntries.Select(entry => entry.Key));

            foreach (BehaviorTreeNodeModelBase node in graphAsset.BehaviorNodes.Where(node => node != null))
            {
                edgesByParent.TryGetValue(node.NodeId, out List<BehaviorTreeChildEdgeModel> children);
                int childCount = children?.Count ?? 0;

                if (node is not BehaviorTreeRootNodeModel && parentCountByNodeId.TryGetValue(node.NodeId, out int parentCount) && parentCount > 1)
                {
                    result.AddError("bt.node.parent.multiple", "BehaviorTree node has multiple parents.", node.NodeId);
                }

                switch (node)
                {
                    case BehaviorTreeRootNodeModel when childCount != 1:
                        result.AddError("bt.root.child.count", "Root node must have exactly one child.", node.NodeId);
                        break;
                    case BehaviorTreeCompositeNodeModel when childCount == 0:
                        result.AddWarning("bt.composite.child.empty", "Composite node has no children.", node.NodeId);
                        break;
                    case BehaviorTreeConditionNodeModel conditionNode:
                        if (childCount != 1)
                        {
                            result.AddError("bt.condition.child.count", "Condition node must have exactly one child.", node.NodeId);
                        }

                        if (string.IsNullOrWhiteSpace(conditionNode.BlackboardKey))
                        {
                            result.AddError("bt.condition.key.empty", "Condition node requires a blackboard key.", node.NodeId);
                        }
                        else if (!blackboardKeys.Contains(conditionNode.BlackboardKey))
                        {
                            result.AddWarning("bt.condition.key.missing", "Condition node references a missing blackboard key.", node.NodeId);
                        }

                        if (conditionNode.ExpectedValueSource == BehaviorTreeConditionValueSource.BlackboardKey)
                        {
                            if (string.IsNullOrWhiteSpace(conditionNode.ExpectedBlackboardKey))
                            {
                                result.AddWarning("bt.condition.expected.key.empty", "Condition node compares against an empty expected blackboard key.", node.NodeId);
                            }
                            else if (!blackboardKeys.Contains(conditionNode.ExpectedBlackboardKey))
                            {
                                result.AddWarning("bt.condition.expected.key.missing", "Condition node references a missing expected blackboard key.", node.NodeId);
                            }
                        }
                        break;
                    case BehaviorTreeServiceNodeModel serviceNode:
                        if (childCount != 1)
                        {
                            result.AddError("bt.service.child.count", "Service node must have exactly one child.", node.NodeId);
                        }

                        if (string.IsNullOrWhiteSpace(serviceNode.ServiceKey))
                        {
                            result.AddWarning("bt.service.key.empty", "Service node does not define a service key.", node.NodeId);
                        }

                        if (serviceNode.IntervalSeconds <= 0f)
                        {
                            result.AddError("bt.service.interval.invalid", "Service node interval must be greater than zero.", node.NodeId);
                        }
                        break;
                    case BehaviorTreeActionNodeModel actionNode:
                        if (childCount != 0)
                        {
                            result.AddError("bt.action.child.invalid", "Action node cannot have child nodes.", node.NodeId);
                        }

                        if (string.IsNullOrWhiteSpace(actionNode.TaskKey))
                        {
                            result.AddWarning("bt.action.task.empty", "Action node does not define a task key.", node.NodeId);
                        }
                        break;
                }
            }

            return result;
        }
    }
}
