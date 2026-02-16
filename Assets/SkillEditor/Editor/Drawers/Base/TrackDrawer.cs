using UnityEditor;
using UnityEngine;

namespace SkillEditor.Editor
{
    public class TrackDrawer : SkillInspectorBase
    {
        // 保持 DrawInspector(TrackBase track) 签名，但内部调用 base
        public virtual void DrawInspector(TrackBase track)
        {
            base.DrawInspector(track);
        }
    }
    
    // 简单的注册中心 (这也是一个简化实现，可以用 Attribute + Reflection 自动发现)
    public static class DrawerFactory
    {
        public static TrackDrawer CreateDrawer(TrackBase track)
        {
            if (track is VFXTrack) return new VFXTrackDrawer();
            if (track is AnimationTrack) return new AnimationTrackDrawer();
            // ... 其他映射
            return new DefaultTrackDrawer();
        }
    }
    
    public class DefaultTrackDrawer : TrackDrawer
    {
        public override void DrawInspector(TrackBase track)
        {
            base.DrawInspector(track);
        }
    }
}
