using System;
using UnityEngine;
using XLua;

public class LuaInstance : MonoBehaviour
{
    public static LuaInstance instance;
    public static LuaEnv luaEnv = new LuaEnv(); //all lua behaviour shared one luaenv only!

    [Tooltip("the unit is second")]
    public float GCInterval = 1;//1 second 
    public string LuaPath = "Lua/txt/";

    private float mLastGCTime = 0;
    private LuaTable mLuaTable;
    private Action mLuaOnOpenAction;
    private Action mLuaOnCloseAction;

    void Awake()
    {
        instance = this;

        luaEnv.AddLoader(CustomLoader);
        luaEnv.AddBuildin("rapidjson", XLua.LuaDLL.Lua.LoadRapidJson);
        luaEnv.AddBuildin("pb", XLua.LuaDLL.Lua.LoadLuaProfobuf);

        mLuaTable = luaEnv.NewTable();

        // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
        LuaTable meta = luaEnv.NewTable();
        meta.Set("__index", luaEnv.Global);
        mLuaTable.SetMetaTable(meta);
        meta.Dispose();

        mLuaTable.Set("self", this);
    }

    void Start()
    {

    }

    void OnDestroy()
    {
        mLuaOnOpenAction = null;
        mLuaOnCloseAction = null;

        //luaEnv.Dispose();
    }

    public void Initialize()
    {
        byte[] scriptByteArray = LoadScript("LuaMain");
        luaEnv.DoString(scriptByteArray, "LuaMain", mLuaTable);

        mLuaTable.Get("OnOpen", out mLuaOnOpenAction);
        mLuaTable.Get("OnClose", out mLuaOnCloseAction);
    }

    public void OpenLua()
    {
        if (mLuaOnOpenAction != null)
        {
            mLuaOnOpenAction();
        }
    }

    public void CloseLua()
    {
        if (mLuaOnCloseAction != null)
        {
            mLuaOnCloseAction();
        }
    }

    void Update()
    {
        float time = Time.time;
        if (time - mLastGCTime > GCInterval)
        {
            luaEnv.Tick();
            mLastGCTime = time;
        }
    }

    public byte[] LoadScript(string pathFileName)
    {
        pathFileName = string.Concat(LuaPath, pathFileName);

        TextAsset textAsset = Resources.Load<TextAsset>(pathFileName);
        if (textAsset != null)
        {
            byte[] bytes = textAsset.bytes;

            return bytes;
        }

        return null;
    }

    private byte[] CustomLoader(ref string pathFileName)
    {
        return LoadScript(pathFileName);
    }
}