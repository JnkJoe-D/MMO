using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel : MonoBehaviour
{
    //相对路径
    protected string path;
    // //目录
    // public const string skinPathPrefix = "Assets/Resources/UiPrefab/";
    // //后缀
    // public const string skinSuffix = ".prefab";
    //皮肤
    public GameObject selfObject;
    //层级
    public PanelLayer layer = PanelLayer.Panel;
    public void Init()
    {
        OnInit();
    }
    public void Close()
    {
        string panelName = this.GetType().Name;
        PanelManager.Close(panelName);
    }
    public virtual void OnInit()
    {
        GameObject skinPrefab = (GameObject)ResManager.LoadAsset(PanelManager.pathEnvir,path,Suffix.prefab);
        selfObject = (GameObject)Instantiate(skinPrefab);
        RectTransform rect = selfObject.transform as RectTransform;
        rect.sizeDelta = Vector2.zero;
        Close();
    }
    public virtual void OnShow(params object[] args)
    {
        selfObject?.SetActive(true);
    }
    public virtual void OnClose()
    { 
        selfObject?.SetActive(false);
    }
}
