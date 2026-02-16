using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TipPanel : BasePanel
{
    public Text text => selfObject.transform.Find("View/Main/TipText").GetComponent<Text>();
    public Button confirmBtn => selfObject.transform.Find("View/Main/ConfirmBtn").GetComponent<Button>();
    public Button closeBtn => selfObject.transform.Find("View/CloseBtn").GetComponent<Button>();

    public override void OnInit()
    {
        path = "TipPanel";
        base.OnInit();
        layer = PanelLayer.Tip;
    }
    public override void OnShow(params object[] args)
    {
        base.OnShow(args);
        confirmBtn.onClick.AddListener(OnConfirmBtnClick);
        closeBtn.onClick.AddListener(OnCloseBtnClick);
        if (args.Length == 1)
        {
            text.text = (string)args[0];
        }
    }

    private void OnConfirmBtnClick()
    {
        Close();
    }
    private void OnCloseBtnClick()
    {
        Close();
    }
    public override void OnClose()
    {
        base.OnClose();
    }
}
