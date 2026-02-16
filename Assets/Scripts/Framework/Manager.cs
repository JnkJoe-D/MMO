using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static GameObject root;
    public static Manager instance;
    public static Manager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Manager>();
                if (instance == null)
                {
                    root = new GameObject("GameManager");
                    instance = root.AddComponent<Manager>();
                }
            }
            return instance;
        }
    }
    public static readonly Dictionary<Type, object> managers = new Dictionary<Type, object>();

    public T GetManager<T>() where T : MonoSingleton<T>
    {
        Type type = typeof(T);
        if (managers.TryGetValue(type, out object m))
        {
            return m as T;
        }
        GameObject obj = new GameObject(typeof(T).Name);
        obj.transform.SetParent(root.transform);
        T manager = obj.AddComponent<T>();
        managers[type] = manager;
        return manager;
    }
    public void RemoveManager<T>() where T : class, new()
    {
        Type type = typeof(T);
        if (managers.ContainsKey(type))
        {
            Destroy(root.transform.Find(type.Name)?.gameObject);
            managers.Remove(type);
        }
    }
}
