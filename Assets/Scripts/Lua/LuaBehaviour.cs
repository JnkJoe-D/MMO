using System;
using XLua;

[LuaCallCSharp]
public class LuaBehaviour : LuaScript
{
    private Action mLuaAwakeAction;
    private Action mLuaStartAction;
    private Action mLuaUpdateAction;
    private Action mLuaOnDestroyAction;

    public override void Awake()
    {
        base.Awake();

        if (mLuaTable != null)
        {
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

    public override void Start()
    {
        base.Start();

        if (mLuaStartAction != null)
        {
            mLuaStartAction();
        }
    }

    public override void Update()
    {
        base.Update();

        if (mLuaUpdateAction != null)
        {
            mLuaUpdateAction();
        }
    }

    public override void OnDestroy()
    {
        if (mLuaOnDestroyAction != null)
        {
            mLuaOnDestroyAction();
        }

        mLuaAwakeAction = null;
        mLuaStartAction = null;
        mLuaUpdateAction = null;
        mLuaOnDestroyAction = null;

        base.OnDestroy();
    }
}