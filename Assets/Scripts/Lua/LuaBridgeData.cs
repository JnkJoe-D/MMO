using XLua;

public interface BTypeItemData
{
    public int type
    {
        get;
        set;
    }

    public int id
    {
        get;
        set;
    }

    public long count
    {
        get;
        set;
    }
}

public interface BItemData
{
    public int type
    {
        get;
        set;
    }

    public int id
    {
        get;
        set;
    }

    public long count
    {
        get;
        set;
    }

    public long uniqueId
    {
        get;
        set;
    }

    public int level
    {
        get;
        set;
    }

    public int lockState
    {
        get;
        set;
    }

    public int exp
    {
        get;
        set;
    }
}