using UnityEditor;
using UnityEngine;
using SkillEditor;

namespace SkillEditor.Editor
{
    public class ClipDrawer : SkillInspectorBase
    {
        public virtual void DrawInspector(ClipBase clip)
        {
            base.DrawInspector(clip);
        }
    }
    
    public static class ClipDrawerFactory
    {
        private static System.Collections.Generic.Dictionary<System.Type, System.Type> _drawerMap;

        private static void Initialize()
        {
            _drawerMap = new System.Collections.Generic.Dictionary<System.Type, System.Type>();
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies)
            {
                // Simple filter to speed up
                var asmName = asm.GetName().Name;
                if (asmName.StartsWith("System") || asmName.StartsWith("mscorlib")) continue;

                System.Type[] types;
                try { types = asm.GetTypes(); } catch { continue; }

                foreach (var type in types)
                {
                    if (typeof(ClipDrawer).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        var attr = (CustomDrawerAttribute)System.Attribute.GetCustomAttribute(type, typeof(CustomDrawerAttribute));
                        if (attr != null && attr.TargetType != null)
                        {
                            _drawerMap[attr.TargetType] = type;
                        }
                    }
                }
            }
        }

        public static ClipDrawer CreateDrawer(ClipBase clip)
        {
            if (_drawerMap == null) Initialize();

            if (clip != null && _drawerMap.TryGetValue(clip.GetType(), out var drawerType))
            {
                return (ClipDrawer)System.Activator.CreateInstance(drawerType);
            }
            return new DefaultClipDrawer();
        }
    }
    
    public class DefaultClipDrawer : ClipDrawer
    {
        public override void DrawInspector(ClipBase clip)
        {
            base.DrawInspector(clip);
        }
    }
}
