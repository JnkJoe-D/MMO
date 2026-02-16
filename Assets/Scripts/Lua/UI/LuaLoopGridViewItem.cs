using SuperScrollView;
using System;
using System.IO;
using XLua;

[LuaCallCSharp]
public class LuaLoopGridViewItem : LoopGridViewItem
{
    public string luaScriptPathFileName;
    public StringInjection[] stringInjectionArray;
    public ObjectInjection[] objectInjectionArray;
    public ArrayInjection[] arrayInjectionArray;

    protected LuaTable mLuaTable;
    private Action mLuaAwakeAction;
    private Action mLuaStartAction;
    private Action mLuaUpdateAction;
    private Action mLuaOnDestroyAction;

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

            mLuaTable.Get("Awake", out mLuaAwakeAction);
            mLuaTable.Get("Start", out mLuaStartAction);
            mLuaTable.Get("Update", out mLuaUpdateAction);
            mLuaTable.Get("OnDestroy", out mLuaOnDestroyAction);

            if (mLuaAwakeAction != null)
            {
                mLuaAwakeAction();
            }
        }
    }

    public virtual void Start()
    {
        if (mLuaStartAction != null)
        {
            mLuaStartAction();
        }
    }

    public virtual void Update()
    {
        if (mLuaUpdateAction != null)
        {
            mLuaUpdateAction();
        }
    }

    public virtual void OnDestroy()
    {
        if (mLuaOnDestroyAction != null)
        {
            mLuaOnDestroyAction();
        }

        mLuaAwakeAction = null;
        mLuaStartAction = null;
        mLuaUpdateAction = null;
        mLuaOnDestroyAction = null;

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