using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LuaView))]
public class LuaViewInspector : Editor
{
    private LuaView mLuaView;

    void OnEnable()
    {
        mLuaView = target as LuaView;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("GetUnityObject"))
        {
            ObjectInjection[] objectInjectionArray = mLuaView.objectInjectionArray;
            if (objectInjectionArray != null)
            {
                for (int i = 0; i < objectInjectionArray.Length; i++)
                {
                    LuaScriptInspector.GetUnityObject(objectInjectionArray[i]);
                }
            }

            ArrayInjection[] arrayInjectionArray = mLuaView.arrayInjectionArray;
            if (arrayInjectionArray != null)
            {
                for (int i = 0; i < arrayInjectionArray.Length; i++)
                {
                    LuaScriptInspector.GetUnityObjectArray(arrayInjectionArray[i]);
                }
            }

            EditorUtility.SetDirty(mLuaView.gameObject);
        }
    }
}