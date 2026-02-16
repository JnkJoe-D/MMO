using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiTest1 : MonoBehaviour
{
    public static string id = "";
    void Start()
    {
        Application.runInBackground = true;
        PanelManager.Init();
        BatttleManager.Init();
        PanelManager.Open<LoginPanel>();
    }
    void Update()
    {
        NetManager.MsgUpdate();
    }
}
