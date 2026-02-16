using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XLua;
[LuaCallCSharp]
public class ResManager
{
    private static readonly Dictionary<string, PathEnvir> envirDict
    = new Dictionary<string, PathEnvir>();
    public static PathEnvir AddEnvir(string prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return null;
        if (envirDict.ContainsKey(prefix)) return envirDict[prefix];
        PathEnvir envir = new PathEnvir(prefix);
        envirDict.Add(prefix, envir);
        return envir;
    }
    public static Object LoadAsset(PathEnvir envir, string path, string suffix)
    {
        return Load(envir.GetFullPath(path, suffix));
    }
    public static GameObject LoadPrefabAtPath(string path)
    {
        return Load<GameObject>(path);
    }
    public static T Load<T>(string path) where T : Object
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<T>(path);
#endif
        return Resources.Load<T>(path.Replace("Assets/Resources/","").Replace(".prefab",""));
    }
    public static Object Load(string path)
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<Object>(path);
#endif
        return Resources.Load<GameObject>(path.Replace("Assets/Resources/","").Replace(".prefab",""));
    }
}
public class PathEnvir
{
    //根目录
    public string prefix;
    public PathEnvir(string pre)
    {
        prefix = pre;
    }
    public string GetFullPath(string path, string suffix)
    {
        return string.Concat(prefix, path, suffix);
    }
}
public class Suffix
{
    public const string prefab = ".prefab";
    public const string bytes = ".bytes";
    public const string txt = ".txt";
    public const string png = ".png";
    public const string jpg = ".jpg";
    public const string json = ".json";
    public const string xml = ".xml";
    public const string lua = ".lua";
    public const string assetbundle = ".assetbundle";
}