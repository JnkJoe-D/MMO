using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : BasePanel
{
    // 自动生成的UI组件字段
    private Image WinImage;
    private Image LostImage;
    private Button OkBtn;

    private void BindUIComponents()
    {
        // 自动绑定UI组件
        WinImage = selfObject.transform.Find("View/Main/WinImage").GetComponent<Image>();
        LostImage = selfObject.transform.Find("View/Main/LostImage").GetComponent<Image>();
        OkBtn = selfObject.transform.Find("View/Main/OkBtn").GetComponent<Button>();
    }


    public override void OnInit()
    {
        path = "ResultPanel";
        base.OnInit();
        layer = PanelLayer.Tip;
    }
    public override void OnShow(params object[] args)
    {
        base.OnShow(args);
        BindUIComponents();

        OkBtn.onClick.AddListener(OnOkBtnClick);
        if (args.Length == 1)
        {
            bool isWin = (bool)args[0];
            if (isWin)
            {
                WinImage.gameObject.SetActive(true);
                LostImage.gameObject.SetActive(false);
            }
            else
            {
                WinImage.gameObject.SetActive(false);
                LostImage.gameObject.SetActive(true);
            }
        }
    }

    private void OnOkBtnClick()
    {
        PanelManager.Open<RoomPanel>();
        Close();
    }

    public override void OnClose()
    {
        base.OnClose();
    }
}
