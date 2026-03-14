using System;

namespace Game.GraphTools
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class GraphNodeDefinitionAttribute : Attribute
    {
        public GraphNodeDefinitionAttribute(string menuPath, Type graphAssetType, int order = 0)
        {
            MenuPath = menuPath;
            GraphAssetType = graphAssetType;
            Order = order;
        }

        public string MenuPath { get; }
        public Type GraphAssetType { get; }
        public int Order { get; }
    }
}
