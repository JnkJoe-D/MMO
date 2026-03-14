using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Game.GraphTools
{
    public static class GraphConditionRegistry
    {
        public sealed class GraphConditionInfo
        {
            public Type ConditionType;
            public GraphConditionDefinitionAttribute Definition;
        }

        private static List<GraphConditionInfo> cachedConditions;

        public static IReadOnlyList<GraphConditionInfo> GetRegisteredConditions(Type contractType = null)
        {
            if (cachedConditions == null)
            {
                BuildCache();
            }

            if (contractType == null)
            {
                return cachedConditions;
            }

            return cachedConditions.Where(x => contractType.IsAssignableFrom(x.ConditionType)).ToList();
        }

        private static void BuildCache()
        {
            cachedConditions = new List<GraphConditionInfo>();

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
                    if (type == null || type.IsAbstract)
                    {
                        continue;
                    }

                    GraphConditionDefinitionAttribute attribute = type.GetCustomAttribute<GraphConditionDefinitionAttribute>();
                    if (attribute == null)
                    {
                        continue;
                    }

                    cachedConditions.Add(new GraphConditionInfo
                    {
                        ConditionType = type,
                        Definition = attribute
                    });
                }
            }

            cachedConditions.Sort((a, b) => a.Definition.Order.CompareTo(b.Definition.Order));
        }
    }
}
