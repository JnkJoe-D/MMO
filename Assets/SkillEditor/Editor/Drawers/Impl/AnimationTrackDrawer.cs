using UnityEditor;
using UnityEngine;

namespace SkillEditor.Editor
{
    public class AnimationTrackDrawer : TrackDrawer
    {
        public override void DrawInspector(TrackBase track)
        {
            var animTrack = track as AnimationTrack;
            if (animTrack == null) return;

            EditorGUILayout.LabelField("动画轨道", EditorStyles.boldLabel);
            base.DrawInspector(track);
        }
    }
}
