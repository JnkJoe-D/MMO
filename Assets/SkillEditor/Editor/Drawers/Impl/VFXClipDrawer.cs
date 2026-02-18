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
            EditorGUI.BeginChangeCheck();
            base.DrawInspector(clip);
            bool propertiesChanged = EditorGUI.EndChangeCheck();

            // 绘制美化后的匹配长度按钮
            if (vfxClip.effectPrefab != null)
            {
                GUILayout.Space(10);

                // 尝试获取当前编辑器预览中的实例
                Editor.EditorVFXProcess activeProcess = null;
                SkillEditorWindow window = null;

                // 防止 GetWindow 抢夺焦点导致 Inspector 无法输入
                if (EditorWindow.HasOpenInstances<SkillEditorWindow>())
                {
                    window = EditorWindow.GetWindow<SkillEditorWindow>(false, "技能编辑器", false);
                    if (window != null && window.PreviewRunner != null)
                    {
                        foreach (var p in window.PreviewRunner.ActiveProcesses)
                        {
                            if (p.clip == vfxClip && p.isActive && p.process is Editor.EditorVFXProcess vfxProcess)
                            {
                                activeProcess = vfxProcess;
                                break;
                            }
                        }
                    }
                }
                
                // 如果属性发生变更，且当前有活跃实例，强制刷新 Transform
                if (propertiesChanged && activeProcess != null)
                {
                    activeProcess.ForceUpdateTransform();
                }

                if (activeProcess != null && activeProcess.Instance != null)
                {
                    activeProcess.GetCurrentRelativeOffset(out Vector3 pOffset, out Vector3 rOffset, out Vector3 s);
                    
                    bool isChanged = Vector3.Distance(vfxClip.positionOffset, pOffset) > 0.001f ||
                                     Vector3.Distance(vfxClip.rotationOffset, rOffset) > 0.001f ||
                                     Vector3.Distance(vfxClip.scale, s) > 0.001f;

                    // 使用内置图标 d_Refresh
                    var content = EditorGUIUtility.IconContent("d_Refresh");
                    content.text = isChanged ? "同步变换 (有变更)" : "变换已同步";

                    // 颜色变化
                    var defaultColor = GUI.backgroundColor;
                    if (isChanged)
                    {
                        GUI.backgroundColor = Color.yellow; // 有变更时高亮
                    }

                    if (GUILayout.Button(content, GUILayout.Height(30)))
                    {
                        if (isChanged)
                        {
                            if (window != null)
                            {
                                var timeline = window.GetCurrentTimeline();
                                if (timeline != null)
                                {
                                    Undo.RecordObject(timeline, "Sync VFX Transform");
                                    vfxClip.positionOffset = pOffset;
                                    vfxClip.rotationOffset = rOffset;
                                    vfxClip.scale = s;
                                    EditorUtility.SetDirty(timeline);
                                }
                            }
                        }
                    }
                    
                    GUI.backgroundColor = defaultColor;
                }
                else
                {
                    // 未激活时显示禁用或提示
                     using (new EditorGUI.DisabledScope(true))
                     {
                         GUILayout.Button("请在预览模式下选中播放以同步", GUILayout.Height(30));
                     }
                }
            }
        }
        
        protected override bool ShouldShow(System.Reflection.FieldInfo field, object obj)
        {
            if (!base.ShouldShow(field, obj)) return false;
            
            // 简单的硬编码 ShowIf 逻辑
            if (field.Name == "blendInDuration" || field.Name == "blendOutDuration")
            {
                if (obj is ClipBase c && !c.SupportsBlending) return false;
            }

            // 自定义骨骼名仅在 bindPoint == CustomBone 时显示
            if (field.Name == "customBoneName")
            {
                if (obj is VFXClip vfx && vfx.bindPoint != VFXBindPoint.CustomBone)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}
