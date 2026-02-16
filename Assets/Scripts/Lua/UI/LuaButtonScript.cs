using XLua;

[System.Serializable]
public class LuaButtonEventItem
{
    public string eventName = "OnClick";
    public System.Action<string> action;
    public string parameter;
}

[LuaCallCSharp]
public class LuaButtonScript : LuaBehaviour
{
    public LuaButtonEventItem[] eventItemArray;

    public override void Awake()
    {
        base.Awake();

        if (mLuaTable != null && eventItemArray != null)
        {
            for (int i = 0; i < eventItemArray.Length; i++)
            {
                if (eventItemArray[i] != null)
                {
                    mLuaTable.Get(eventItemArray[i].eventName, out eventItemArray[i].action);
                }
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        for (int i = 0; i < eventItemArray.Length; i++)
        {
            if (eventItemArray[i] != null)
            {
                eventItemArray[i].action = null;
            }
        }
    }

    public void OnClick()
    {
        if (eventItemArray != null)
        {
            for (int i = 0; i < eventItemArray.Length; i++)
            {
                if (eventItemArray[i] != null && eventItemArray[i].action != null)
                {
                    eventItemArray[i].action(eventItemArray[i].parameter);
                }
            }
        }
    }
}