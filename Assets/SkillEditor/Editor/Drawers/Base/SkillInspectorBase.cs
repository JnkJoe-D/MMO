using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using SkillEditor;
using Object = UnityEngine.Object;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 基于反射的 Inspector 基类
    /// </summary>
    public class SkillInspectorBase
    {
        public Object[] UndoContext { get; set; }

        /// <summary>
        /// 值发生改变时触发
        /// </summary>
        public event System.Action OnInspectorChanged;

        public virtual void DrawInspector(object target)
        {
            if (target == null) return;
            DrawDefaultInspector(target);
        }

        protected void DrawDefaultInspector(object obj)
        {
            var targetType = obj.GetType();
            
            // 获取继承链 (Base -> Derived)
            var typeHierarchy = new Stack<Type>();
            var current = targetType;
            while (current != null && current != typeof(object))
            {
                typeHierarchy.Push(current);
                current = current.BaseType;
            }

            // 按顺序绘制每一层的字段
            foreach (var type in typeHierarchy)
            {
                FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                foreach (var field in fieldInfos)
                {
                    if (ShouldShow(field, obj))
                    {
                        DrawField(field, obj);
                    }
                }
            }
        }

        protected virtual bool ShouldShow(FieldInfo field, object obj)
        {
            if (field.IsDefined(typeof(HideInInspector), true)) return false;
            
            // 硬编码的 Blending 逻辑 
            if (field.Name == "blendInDuration" || field.Name == "blendOutDuration")
            {
                if (obj is ClipBase c && !c.SupportsBlending) return false;
            }

            return true;
        }

        protected virtual void DrawField(FieldInfo field, object obj)
        {
            var value = field.GetValue(obj);
            var fieldType = field.FieldType;
            var attribute = field.GetCustomAttribute<SkillPropertyAttribute>();
            var name = attribute != null ? attribute.Name : ObjectNames.NicifyVariableName(field.Name);

            object newValue = value;

            EditorGUI.BeginChangeCheck();

            if (fieldType == typeof(int))
            {
                newValue = EditorGUILayout.IntField(name, (int)value);
            }
            else if (fieldType == typeof(float))
            {
                // 特殊处理：如果是 startTime/duration 等需要限制非负
                // 也可以引入 [Min] 属性
                
                if (field.Name == "blendInDuration" || field.Name == "blendOutDuration")
                {
                    // 动态获取 duration 作为最大值
                    float maxDuration = 10f;
                    if (obj is ClipBase c) maxDuration = c.Duration / 2f;
                    
                    newValue = EditorGUILayout.Slider(name, (float)value, 0f, maxDuration);
                }
                else
                {
                    float floatVal = EditorGUILayout.FloatField(name, (float)value);
                    if (field.Name == "startTime" || field.Name == "duration")
                    {
                        floatVal = Mathf.Max(0f, floatVal);
                    }
                    newValue = floatVal;
                }
            }
            else if (fieldType == typeof(bool))
            {
                newValue = EditorGUILayout.Toggle(name, (bool)value);
            }
            else if (fieldType == typeof(string))
            {
                newValue = EditorGUILayout.TextField(name, (string)value);
            }
            else if (fieldType == typeof(Vector2))
            {
                newValue = EditorGUILayout.Vector2Field(name, (Vector2)value);
            }
            else if (fieldType == typeof(Vector3))
            {
                newValue = EditorGUILayout.Vector3Field(name, (Vector3)value);
            }
            else if (fieldType == typeof(Color))
            {
                newValue = EditorGUILayout.ColorField(name, (Color)value);
            }
            else if (fieldType == typeof(AnimationCurve))
            {
                newValue = EditorGUILayout.CurveField(name, (AnimationCurve)value ?? new AnimationCurve());
            }
            else if (typeof(Object).IsAssignableFrom(fieldType))
            {
                newValue = EditorGUILayout.ObjectField(name, (Object)value, fieldType, false);
            }
            else if (fieldType.IsEnum)
            {
                newValue = EditorGUILayout.EnumPopup(name, (Enum)value);
            }
            else if (fieldType == typeof(LayerMask))
            {
                // unity's built in LayerMask extension for EditorGUILayout
                LayerMask tempMask = (LayerMask)value;
                int maskField = EditorGUILayout.MaskField(name, InternalEditorUtility.LayerMaskToConcatenatedLayersMask(tempMask), InternalEditorUtility.layers);
                newValue = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(maskField);
            }
            else if (value is HitBoxShape shape)
            {
                EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                shape.shapeType = (HitBoxType)EditorGUILayout.EnumPopup("形状类型", shape.shapeType);
                if (shape.shapeType == HitBoxType.Box)
                {
                    shape.size = EditorGUILayout.Vector3Field("尺寸 (Box)", shape.size);
                }
                if (shape.shapeType == HitBoxType.Sphere || shape.shapeType == HitBoxType.Capsule || shape.shapeType == HitBoxType.Sector || shape.shapeType == HitBoxType.Ring)
                {
                    shape.radius = EditorGUILayout.FloatField("半径", shape.radius);
                }
                if (shape.shapeType == HitBoxType.Capsule || shape.shapeType == HitBoxType.Ring || shape.shapeType == HitBoxType.Sector)
                {
                    shape.height = EditorGUILayout.FloatField("高度", shape.height);
                }
                if (shape.shapeType == HitBoxType.Sector)
                {
                    shape.angle = EditorGUILayout.Slider("角度", shape.angle, 0f, 360f);
                }
                if (shape.shapeType == HitBoxType.Ring)
                {
                    shape.innerRadius = EditorGUILayout.FloatField("内半径", shape.innerRadius);
                }
                EditorGUI.indentLevel--;
                newValue = shape;
            }
            else if (value is List<SkillEventParam> paramList)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    paramList.Add(new SkillEventParam());
                    GUI.FocusControl(null);
                }
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < paramList.Count; i++)
                {
                    var p = paramList[i];
                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.BeginHorizontal();
                    p.key = EditorGUILayout.TextField("参数名", p.key);
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        paramList.RemoveAt(i);
                        GUI.FocusControl(null);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                    p.stringValue = EditorGUILayout.TextField("字符串", p.stringValue);
                    p.floatValue = EditorGUILayout.FloatField("浮点数", p.floatValue);
                    p.intValue = EditorGUILayout.IntField("整数", p.intValue);
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
                newValue = paramList;
            }
            else if (fieldType == typeof(string[]))
            {
                var stringArray = (string[])value;
                if (stringArray == null) stringArray = new string[0];

                // 尝试搜寻本地配置
                string[] availableTagsArray = null;
                var guids = AssetDatabase.FindAssets("t:SkillTagConfig");
                if (guids.Length > 0)
                {
                    var config = AssetDatabase.LoadAssetAtPath<global::SkillEditor.SkillTagConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    if (config != null && config.availableTags != null)
                    {
                        availableTagsArray = config.availableTags.ToArray();
                    }
                }

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
                
                // 没找到配置库警告
                if (availableTagsArray == null || availableTagsArray.Length == 0)
                {
                    EditorGUILayout.HelpBox("未找到 SkillTagConfig 资产！", MessageType.Warning);
                }

                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    Array.Resize(ref stringArray, stringArray.Length + 1);
                    stringArray[stringArray.Length - 1] = (availableTagsArray != null && availableTagsArray.Length > 0) ? availableTagsArray[0] : "";
                    GUI.FocusControl(null);
                }
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < stringArray.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    string currentVal = stringArray[i];

                    if (availableTagsArray != null && availableTagsArray.Length > 0)
                    {
                        int currentIndex = Array.IndexOf(availableTagsArray, currentVal);

                        if (currentIndex == -1) // 词库丢失或非法值
                        {
                            var oldColor = GUI.color;
                            GUI.color = Color.red;
                            stringArray[i] = EditorGUILayout.TextField($"[已丢失] {currentVal}");
                            GUI.color = oldColor;
                        }
                        else
                        {
                            int newIndex = EditorGUILayout.Popup(currentIndex, availableTagsArray);
                            stringArray[i] = availableTagsArray[newIndex];
                        }
                    }
                    else
                    {
                        // 兜底方案：如果没有预设库，退化为文本框
                        stringArray[i] = EditorGUILayout.TextField(stringArray[i]);
                    }

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        var list = new List<string>(stringArray);
                        list.RemoveAt(i);
                        stringArray = list.ToArray();
                        GUI.FocusControl(null);
                        EditorGUILayout.EndHorizontal();
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                newValue = stringArray;
            }
            else if (typeof(IList).IsAssignableFrom(fieldType))
            {
                EditorGUILayout.LabelField(name, "List (Not Implemented in Base)");
            }
            else
            {
                EditorGUILayout.LabelField(name, $"Unsupported Type: {fieldType.Name}");
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (UndoContext != null && UndoContext.Length > 0)
                {
                    Undo.RecordObjects(UndoContext, "Inspector Change: " + name);
                }
                field.SetValue(obj, newValue);
                OnInspectorChanged?.Invoke();
            }
        }
    }
}
