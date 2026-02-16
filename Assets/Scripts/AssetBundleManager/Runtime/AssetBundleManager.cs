using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AssetBundleManager : MonoBehaviour
{
    public string[] assetNames;
    void Start()
    {
        AssetBundleLoader.Instance.LoadMainfest();
    }

    void Update()
    {
        AssetBundleLoader.Instance.Update();
    }
    // [Button("AB同步加载测试")]
    public void TestLoadABSync(string abName, string assetName)
    {
        var ab = AssetLoader.Instance.LoadABSync(abName);
        GameObject go = AssetLoader.Instance.LoadResSync<GameObject>(ab, assetName);
        if (!go) return;
        Instantiate(go);
        Debug.Log("succeed");
    }
    // [Button("AB异步加载测试")]
    public void TestLoadABAsync(string abName, string assetName)
    {
        AssetLoader.Instance.LoadABAsync(abName,(ab)=>{
            if(!ab)return;
            GameObject go = AssetLoader.Instance.LoadResSync<GameObject>(ab, assetName);
            if (!go) return;
            Instantiate(go);
            Debug.Log("succeed");
        });
    }
    // [Button("Asset异步加载测试")]
    public void TestLoadAssetAsync(string abName)
    {
        var ab = AssetLoader.Instance.LoadABSync(abName);
        if(!ab)return;
        for(int i=0;i<assetNames.Length;i++)
        {
            AssetLoader.Instance.LoadResAsync<GameObject>(ab, assetNames[i], (go) => 
            {
                if (!go) return;
                var instantiatedGo = Instantiate(go);
                instantiatedGo.transform.position += new Vector3(i, 0, 0);
                Debug.Log("succeed");
            }, this);
        }
        
    }
    // [Button("清理所有AB")]
    public void UnLoadAll()
    {
        AssetBundleLoader.Instance.ClearAll();
    }
}

