using System;

namespace Game.GraphTools
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class GraphConditionDefinitionAttribute : Attribute
    {
        public GraphConditionDefinitionAttribute(string displayName, int order = 0)
        {
            DisplayName = displayName;
            Order = order;
        }

        public string DisplayName { get; }
        public int Order { get; }
    }
}
