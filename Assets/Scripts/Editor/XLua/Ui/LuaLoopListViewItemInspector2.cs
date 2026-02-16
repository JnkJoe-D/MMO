using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LuaLoopListViewItem2), true)]
public class LuaLoopListViewItemInspector2 : Editor
{
    private LuaLoopListViewItem2 mLuaLoopListViewItem2;

    void OnEnable()
    {
        mLuaLoopListViewItem2 = target as LuaLoopListViewItem2;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("GetUnityObject"))
        {
            ObjectInjection[] objectInjectionArray = mLuaLoopListViewItem2.objectInjectionArray;
            if (objectInjectionArray != null)
            {
                for (int i = 0; i < objectInjectionArray.Length; i++)
                {
                    LuaScriptInspector.GetUnityObject(objectInjectionArray[i]);
                }
            }

            ArrayInjection[] arrayInjectionArray = mLuaLoopListViewItem2.arrayInjectionArray;
            if (arrayInjectionArray != null)
            {
                for (int i = 0; i < arrayInjectionArray.Length; i++)
                {
                    LuaScriptInspector.GetUnityObjectArray(arrayInjectionArray[i]);
                }
            }

            EditorUtility.SetDirty(mLuaLoopListViewItem2.gameObject);
        }
    }
}