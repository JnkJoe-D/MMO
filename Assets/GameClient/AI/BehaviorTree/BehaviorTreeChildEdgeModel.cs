using System;
using Game.GraphTools;

namespace Game.AI
{
    [Serializable]
    /// <summary>
    /// 行为树父子关系边，额外记录子节点顺序。
    /// </summary>
    public sealed class BehaviorTreeChildEdgeModel : GraphEdgeModelBase
    {
        public int ChildIndex;
    }
}
