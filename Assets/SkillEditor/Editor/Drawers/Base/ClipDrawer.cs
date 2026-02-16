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
        public static ClipDrawer CreateDrawer(ClipBase clip)
        {
            if (clip is VFXClip) return new VFXClipDrawer();
            if (clip is SkillAnimationClip) return new AnimationClipDrawer();
            // ... 其他映射
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
