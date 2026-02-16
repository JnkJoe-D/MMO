using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
public class AssetLoader
{
    private static AssetLoader _Instance = null;
    public static AssetLoader Instance
    {
        get
        {
            if (_Instance == null) _Instance = new AssetLoader();
            return _Instance;
        }
    }
    string MainABName
    {
        get
        {
#if UNITY_IOS
        return "IOS"
#elif UNITY_ANDROID
        return "Android"
#else
            return "PC";
#endif
        }
    }
    public AssetBundle LoadABSync(string abName)
    {
        return AssetBundleLoader.Instance.LoadSync(abName);
    }
    public void LoadABAsync(string abName, AssetBundleLoader.AssetBundleLoadCallBack callback)
    {
        AssetBundleLoader.Instance.LoadAsync(abName, callback);
    }
    public Object LoadResSync(AssetBundle ab, string assetName)
    {
        if (!ab) return null;
        var asset = ab.LoadAsset(assetName);
        return asset;
    }
    public Object LoadResSync(AssetBundle ab, string assetName, Type type)
    {
        if (!ab) return null;
        var asset = ab.LoadAsset(assetName, type);
        return asset;
    }

    public T LoadResSync<T>(AssetBundle ab,  string assetName) where T : Object
    {
        if (!ab) return null;
        var asset = ab.LoadAsset<T>(assetName);
        return asset;
    }
    //[资源]异步加载
    public void LoadResAsync(AssetBundle ab, string assetName, Action<Object> onLoadDone, MonoBehaviour mono)
    {
        if (!ab) return;
        mono.StartCoroutine(DoLoadResAsync(ab, assetName, onLoadDone));
    }
    System.Collections.IEnumerator DoLoadResAsync(AssetBundle ab, string assetName, Action<Object> onLoadDone)
    {
        AssetBundleRequest asr = ab.LoadAssetAsync(assetName);
        yield return asr;
        onLoadDone(asr.asset);
    }
    public void LoadResAsync<T>(AssetBundle ab, string assetName, Action<T> onLoadDone,MonoBehaviour mono) where T : Object
    {
        if (!ab) return;
        mono.StartCoroutine(DoLoadResAsync<T>(ab, assetName, onLoadDone));
    }
    System.Collections.IEnumerator DoLoadResAsync<T>(AssetBundle ab, string assetName, Action<T> onLoadDone) where T : Object
        {
        AssetBundleRequest asr = ab.LoadAssetAsync<T>(assetName);
        yield return asr;
        onLoadDone(asr.asset as T);
    }
    public void LoadResAsync(AssetBundle ab, string assetName, Type type,Action<Object> onLoadDone, MonoBehaviour mono)
    {
        if (!ab) return;
        mono.StartCoroutine(DoLoadResAsync(ab, assetName, type,onLoadDone));
    }
    System.Collections.IEnumerator DoLoadResAsync(AssetBundle ab, string assetName, Type type, Action<Object> onLoadDone)
    {
        AssetBundleRequest asr = ab.LoadAssetAsync(assetName,type);
        yield return asr;
        onLoadDone(asr.asset);
    }
}
