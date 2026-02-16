using XLua;

[System.Serializable]
public class LuaComponentObject
{
    public LuaScript luaScript;
    public LuaView luaView;
    public LuaLoopListViewItem2 luaLoopListViewItem2;
    public LuaLoopGridViewItem luaLoopGridViewItem;
    public LuaLoopStaggeredGridViewItem luaLoopStaggeredGridViewItem;

    public LuaTable GetLuaTable()
    {
        if (luaView != null)
        {
            return luaView.GetLuaTable();
        }

        if (luaScript != null)
        {
            return luaScript.GetLuaTable();
        }

        if (luaLoopListViewItem2 != null)
        {
            return luaLoopListViewItem2.GetLuaTable();
        }

        if (luaLoopGridViewItem != null)
        {
            return luaLoopGridViewItem.GetLuaTable();
        }

        if (luaLoopStaggeredGridViewItem != null)
        {
            return luaLoopStaggeredGridViewItem.GetLuaTable();
        }

        return null;
    }
}