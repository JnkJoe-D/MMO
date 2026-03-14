using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Game.GraphTools
{
    public static class GraphNodeRegistry
    {
        public sealed class GraphNodeInfo
        {
            public Type NodeType;
            public GraphNodeDefinitionAttribute Definition;
        }

        private static List<GraphNodeInfo> cachedNodes;

        public static IReadOnlyList<GraphNodeInfo> GetRegisteredNodes(Type graphAssetType = null)
        {
            if (cachedNodes == null)
            {
                BuildCache();
            }

            if (graphAssetType == null)
            {
                return cachedNodes;
            }

            return cachedNodes.Where(x => x.Definition.GraphAssetType == null || x.Definition.GraphAssetType == graphAssetType).ToList();
        }

        private static void BuildCache()
        {
            cachedNodes = new List<GraphNodeInfo>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyName = assembly.GetName().Name;
                if (assemblyName.StartsWith("System") || assemblyName.StartsWith("Unity") || assemblyName.StartsWith("mscorlib") || assemblyName.StartsWith("Mono"))
                {
                    continue;
                }

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }

                foreach (Type type in types)
                {
                    if (type == null || type.IsAbstract || !typeof(GraphNodeModelBase).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    GraphNodeDefinitionAttribute attribute = type.GetCustomAttribute<GraphNodeDefinitionAttribute>();
                    if (attribute == null)
                    {
                        continue;
                    }

                    cachedNodes.Add(new GraphNodeInfo
                    {
                        NodeType = type,
                        Definition = attribute
                    });
                }
            }

            cachedNodes.Sort((a, b) => a.Definition.Order.CompareTo(b.Definition.Order));
        }
    }
}
