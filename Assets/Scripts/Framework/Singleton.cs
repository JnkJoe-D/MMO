
public class Singleton<T> where T : new()
{
    protected Singleton()
    {
    }

    private static T instance = new T();
    public static T Instance 
    {
        get { return instance; } 
    }
}
