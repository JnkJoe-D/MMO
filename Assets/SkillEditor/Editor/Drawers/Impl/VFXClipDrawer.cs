using UnityEditor;
using UnityEngine;
using SkillEditor;

namespace SkillEditor.Editor
{
    public class VFXClipDrawer : ClipDrawer
    {
        public override void DrawInspector(ClipBase clip)
        {
            var vfxClip = clip as VFXClip;
            if (vfxClip == null) return;
            
            EditorGUILayout.LabelField("特效片段设置", EditorStyles.boldLabel);
            
            // 使用基类的反射绘制
            // 注意：基类会绘制所有 public 字段。
            // VFXClip 有 startTime, duration, clipName, isEnabled (from ClipBase)
            // 和 effectPrefab, offset (from VFXClip)
            
            // 为了更好的体验，我们可能希望手动控制某些字段，或者只依靠反射
            // 这里我们尝试完全依靠反射，看看由于 Field 顺序是否符合预期
            base.DrawInspector(clip);
            
            // 如果需要额外的逻辑（比如 blend 时间的条件显示），可以在 Base 中实现 ShowIf
            // 或者在这里补充绘制
            // 目前 Base 还没有实现 attribute based ShowIf, 所以我们暂时手动处理 blending 逻辑?
            // 但 VFXClipDrawer 之前是手动写的。
            // 如果完全依赖 Base，那么 SupportsBlending 逻辑需要在 Base 中通用化，或者 VFXClip 需要 Attributes.
            
            // 当前方案：混用。但为了响应"ActionEditor那样方便"，我们应该尽量用 Base。
            // 让我们在 SkillInspectorBase 中加一个小 hack：如果字段是 blendXXX 且 SupportsBlending 为 false，则不画。
        }
        
        protected override bool ShouldShow(System.Reflection.FieldInfo field, object obj)
        {
            if (!base.ShouldShow(field, obj)) return false;
            
            // 简单的硬编码 ShowIf 逻辑，模拟 Attribute
            if (field.Name == "blendInDuration" || field.Name == "blendOutDuration")
            {
                if (obj is ClipBase c && !c.SupportsBlending) return false;
            }
            
            return true;
        }
    }
}
