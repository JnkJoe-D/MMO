using UnityEngine;
using XLua;

public class LuaCommon
{
    public static LuaTable GetComponentLuaTable(MonoBehaviour luaTableComponent)
    {
        LuaTable luaTable = null;
        if (luaTableComponent is LuaScript)
        {
            LuaScript luaScript = luaTableComponent as LuaScript;
            luaTable = luaScript.GetLuaTable();
        }
        else if (luaTableComponent is LuaView)
        {
            LuaView luaView = luaTableComponent as LuaView;
            luaTable = luaView.GetLuaTable();
        }

        return luaTable;
    }
}
