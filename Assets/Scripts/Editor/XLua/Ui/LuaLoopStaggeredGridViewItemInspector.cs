using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LuaLoopStaggeredGridViewItem), true)]
public class LuaLoopStaggeredGridViewItemInspector : Editor
{
    private LuaLoopStaggeredGridViewItem mLuaLoopStaggeredGridViewItem;

    void OnEnable()
    {
        mLuaLoopStaggeredGridViewItem = target as LuaLoopStaggeredGridViewItem;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("GetUnityObject"))
        {
            ObjectInjection[] objectInjectionArray = mLuaLoopStaggeredGridViewItem.objectInjectionArray;
            if (objectInjectionArray != null)
            {
                for (int i = 0; i < objectInjectionArray.Length; i++)
                {
                    LuaScriptInspector.GetUnityObject(objectInjectionArray[i]);
                }
            }

            ArrayInjection[] arrayInjectionArray = mLuaLoopStaggeredGridViewItem.arrayInjectionArray;
            if (arrayInjectionArray != null)
            {
                for (int i = 0; i < arrayInjectionArray.Length; i++)
                {
                    LuaScriptInspector.GetUnityObjectArray(arrayInjectionArray[i]);
                }
            }

            EditorUtility.SetDirty(mLuaLoopStaggeredGridViewItem.gameObject);
        }
    }
}