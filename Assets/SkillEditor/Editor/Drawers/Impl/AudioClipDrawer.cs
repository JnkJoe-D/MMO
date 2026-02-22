using UnityEditor;
using SkillEditor;

namespace SkillEditor.Editor
{
    [CustomDrawer(typeof(SkillAudioClip))]
    public class AudioClipDrawer : ClipDrawer
    {
        public override void DrawInspector(ClipBase clip)
        {
            var audioClip = clip as SkillAudioClip;
            if (audioClip == null) return;

            EditorGUILayout.LabelField("音频片段设置", EditorStyles.boldLabel);

            // 使用基类的反射绘制通用属性
            base.DrawInspector(clip);

            // 可以添加额外的自定义绘制，例如简单的播放按钮来预览（虽然 Timeline 也可以预览）
            // 目前保持默认即可
        }
    }
}
