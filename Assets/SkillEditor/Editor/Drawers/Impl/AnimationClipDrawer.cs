using UnityEditor;
using UnityEngine;
using SkillEditor;

namespace SkillEditor.Editor
{
    public class AnimationClipDrawer : ClipDrawer
    {
        public override void DrawInspector(ClipBase clip)
        {
            var animClip = clip as SkillAnimationClip;
            if (animClip == null) return;

            EditorGUILayout.LabelField("动画片段", EditorStyles.boldLabel);
            base.DrawInspector(clip);
        }
    }
}
