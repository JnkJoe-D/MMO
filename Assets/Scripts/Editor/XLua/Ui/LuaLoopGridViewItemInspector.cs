using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LuaLoopGridViewItem), true)]
public class LuaLoopGridViewItemInspector : Editor
{
    private LuaLoopGridViewItem mLuaLoopGridViewItem;

    void OnEnable()
    {
        mLuaLoopGridViewItem = target as LuaLoopGridViewItem;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("GetUnityObject"))
        {
            ObjectInjection[] objectInjectionArray = mLuaLoopGridViewItem.objectInjectionArray;
            if (objectInjectionArray != null)
            {
                for (int i = 0; i < objectInjectionArray.Length; i++)
                {
                    LuaScriptInspector.GetUnityObject(objectInjectionArray[i]);
                }
            }

            ArrayInjection[] arrayInjectionArray = mLuaLoopGridViewItem.arrayInjectionArray;
            if (arrayInjectionArray != null)
            {
                for (int i = 0; i < arrayInjectionArray.Length; i++)
                {
                    LuaScriptInspector.GetUnityObjectArray(arrayInjectionArray[i]);
                }
            }

            EditorUtility.SetDirty(mLuaLoopGridViewItem.gameObject);
        }
    }
}