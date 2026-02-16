using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LuaScript), true)]
public class LuaScriptInspector : Editor
{
    public static void GetUnityObject(ObjectInjection objectInjection)
    {
        if (objectInjection != null && objectInjection.subject != null)
        {
            string typeName = objectInjection.typeName;
            if (!string.IsNullOrEmpty(typeName))
            {
                typeName = typeName.Trim();
            }

            if (string.IsNullOrEmpty(typeName))
            {
                objectInjection.unityObject = objectInjection.subject;
            }
            else
            {
                objectInjection.unityObject = objectInjection.subject.GetComponent(typeName);
            }
        }
    }

    public static void GetUnityObjectArray(ArrayInjection arrayInjection)
    {
        if (arrayInjection != null && arrayInjection.subjectArray != null)
        {
            arrayInjection.unityObjectArray = new GameObject[arrayInjection.subjectArray.Length];

            string typeName = arrayInjection.typeName;
            if (!string.IsNullOrEmpty(typeName))
            {
                typeName = typeName.Trim();
            }

            int length = arrayInjection.subjectArray.Length;
            if (string.IsNullOrEmpty(typeName))
            {
                for (int i = 0; i < length; i++)
                {
                    if (arrayInjection.subjectArray[i] != null)
                    {
                        arrayInjection.unityObjectArray[i] = arrayInjection.subjectArray[i];
                    }
                }
            }
            else
            {
                List<Component> componentList = new List<Component>();
                for (int i = 0; i < length; i++)
                {
                    if (arrayInjection.subjectArray[i] != null)
                    {
                        Component component = arrayInjection.subjectArray[i].GetComponent(typeName);
                        componentList.Add(component);
                    }
                }

                arrayInjection.unityObjectArray = componentList.ToArray();
            }
        }
    }

    private LuaScript mLuaScript;

    void OnEnable()
    {
        mLuaScript = target as LuaScript;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("GetUnityObject"))
        {
            ObjectInjection[] objectInjectionArray = mLuaScript.objectInjectionArray;
            if (objectInjectionArray != null)
            {
                for (int i = 0; i < objectInjectionArray.Length; i++)
                {
                    GetUnityObject(objectInjectionArray[i]);
                }
            }

            ArrayInjection[] arrayInjectionArray = mLuaScript.arrayInjectionArray;
            if (arrayInjectionArray != null)
            {
                for (int i = 0; i < arrayInjectionArray.Length; i++)
                {
                    GetUnityObjectArray(arrayInjectionArray[i]);
                }
            }

            EditorUtility.SetDirty(mLuaScript.gameObject);
        }
    }
}