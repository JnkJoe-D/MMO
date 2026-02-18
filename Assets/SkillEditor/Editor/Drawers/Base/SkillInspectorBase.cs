using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 基于反射的 Inspector 基类
    /// </summary>
    public class SkillInspectorBase
    {
        public Object[] UndoContext { get; set; }

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
            bool changed = false;

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
            // 简单的 List 支持
            else if (typeof(IList).IsAssignableFrom(fieldType))
            {
                // 暂不实现复杂 ListView，Clip 数据通常不包含复杂 List
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
            }
        }
    }
}
