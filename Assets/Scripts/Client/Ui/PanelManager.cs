using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager
{
    //层级列表
    public static readonly Dictionary<PanelLayer, Transform> layerList = new Dictionary<PanelLayer, Transform>();
    //面板列表
    public static readonly Dictionary<string, BasePanel> panelList = new Dictionary<string, BasePanel>();
    public static Transform root;
    public static Transform canvas;
    public static PathEnvir pathEnvir { get; private set; }
    public static void Init()
    {
        //获取UI资源加载目录
        pathEnvir = ResManager.AddEnvir("Assets/Resources/UiPrefab/");
        
        root = GameObject.Find("Root").transform;
        canvas = root?.Find("Canvas");
        Transform panel = canvas?.Find("Panel");
        Transform tip = canvas?.Find("Tip");
        layerList.Add(PanelLayer.Panel, panel);
        layerList.Add(PanelLayer.Tip, tip);
    }
    public static void Open<T>(params object[] parm) where T : BasePanel
    {
        //已经打开
        string name = typeof(T).ToString();
        if (panelList.ContainsKey(name))
        {
            return;
        }
        //组件
        BasePanel panel = root.gameObject.AddComponent<T>();
        panel.Init();
        //父容器
        Transform layer = layerList[panel.layer];
        panel.selfObject.transform.SetParent(layer,false);
        //列表
        panelList.Add(name, panel);
        //显示
        panel.OnShow(parm);
    }
    public static void Close(string panelName)
    {
        //没有打开
        if (!panelList.ContainsKey(panelName))
        {
            return;
        }
        BasePanel panel = panelList[panelName];
        panel.OnClose();
        panelList.Remove(panelName);
        //销毁面板资源
        GameObject.DestroyImmediate(panel.selfObject);
        //销毁面板组件
        GameObject.DestroyImmediate(panel);
    }
}
public enum PanelLayer
{
    Panel,
    Tip,
}