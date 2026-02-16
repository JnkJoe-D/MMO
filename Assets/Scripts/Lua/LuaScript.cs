using System;
using System.IO;
using UnityEngine;
using XLua;

[Serializable]
public class StringInjection
{
    public string name;
    public string parameter;
}

[Serializable]
public class ObjectInjection
{
    public string name;
    public GameObject subject;
    public string typeName;
    public UnityEngine.Object unityObject;
}

[Serializable]
public class ArrayInjection
{
    public string name;
    public GameObject[] subjectArray;
    public string typeName;
    public UnityEngine.Object[] unityObjectArray;
}

[LuaCallCSharp]
public class LuaScript : MonoBehaviour
{
    public string luaScriptPathFileName;

    public StringInjection[] stringInjectionArray;
    public ObjectInjection[] objectInjectionArray;
    public ArrayInjection[] arrayInjectionArray;

    protected LuaTable mLuaTable;

    public virtual void Awake()
    {
        if (!string.IsNullOrEmpty(luaScriptPathFileName))
        {
            mLuaTable = LuaInstance.luaEnv.NewTable();

            // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
            LuaTable meta = LuaInstance.luaEnv.NewTable();
            meta.Set("__index", LuaInstance.luaEnv.Global);
            mLuaTable.SetMetaTable(meta);
            meta.Dispose();

            mLuaTable.Set("self", this);

            if (stringInjectionArray != null)
            {
                for (int i = 0; i < stringInjectionArray.Length; i++)
                {
                    if (stringInjectionArray[i] != null)
                    {
                        mLuaTable.Set(stringInjectionArray[i].name, stringInjectionArray[i].parameter);
                    }
                }
            }

            if (objectInjectionArray != null)
            {
                for (int i = 0; i < objectInjectionArray.Length; i++)
                {
                    if (objectInjectionArray[i] != null)
                    {
                        mLuaTable.Set(objectInjectionArray[i].name, objectInjectionArray[i].unityObject);
                    }
                }
            }

            if (arrayInjectionArray != null)
            {
                for (int i = 0; i < arrayInjectionArray.Length; i++)
                {
                    if (arrayInjectionArray[i] != null)
                    {
                        mLuaTable.Set(arrayInjectionArray[i].name, arrayInjectionArray[i].unityObjectArray);
                    }
                }
            }

            byte[] byteArray = LuaInstance.instance.LoadScript(luaScriptPathFileName);
            string fileName = Path.GetFileNameWithoutExtension(luaScriptPathFileName);
            LuaInstance.luaEnv.DoString(byteArray, fileName, mLuaTable);
        }
    }

    public virtual void Start()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void OnDestroy()
    {
        stringInjectionArray = null;
        objectInjectionArray = null;
        arrayInjectionArray = null;

        if (mLuaTable != null)
        {
            mLuaTable.Dispose();
        }
    }

    public LuaTable GetLuaTable()
    {
        return mLuaTable;
    }
}