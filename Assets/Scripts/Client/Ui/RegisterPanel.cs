using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : BasePanel
{
    InputField idInput;
    InputField pwInput;
    InputField repInput;
    Button regBtn;
    Button closeBtn;
    public override void OnInit()
    {
        path = "RegisterPanel";
        base.OnInit();
        layer = PanelLayer.Panel;
    }
    public override void OnShow(params object[] args)
    {
        base.OnShow(args);
        //组件
        idInput = selfObject.transform.Find("View/Main/Id/IdInput").GetComponent<InputField>();
        pwInput = selfObject.transform.Find("View/Main/Pw/PwInput").GetComponent<InputField>();
        repInput = selfObject.transform.Find("View/Main/Rep/RepInput").GetComponent<InputField>();
        closeBtn = selfObject.transform.Find("View/CloseBtn").GetComponent<Button>();
        regBtn = selfObject.transform.Find("View/Main/Button/RegBtn").GetComponent<Button>();
        closeBtn.onClick.AddListener(OnCloseClick);
        regBtn.onClick.AddListener(OnRegClick);
        //网络
        //协议
        NetManager.AddMsgEventListener("MsgRegister", OnMsgRegister);
    }



    public override void OnClose()
    {
        base.OnClose();
        //移除
        //协议
        NetManager.RemoveMsgEventListener("MsgLogin", OnMsgRegister);
    }
    void OnCloseClick()
    {
        Close();
    }
    void OnRegClick()
    {
        //用户名密码为空
        if (string.IsNullOrEmpty(idInput.text) || string.IsNullOrEmpty(pwInput.text))
        {
            Debug.Log("用户名密码不能为空");
            PanelManager.Open<TipPanel>("用户名密码不能为空");
            return;
        }
        if (pwInput.text != repInput.text)
        {
            Debug.Log("密码不一致，请重试");
            PanelManager.Open<TipPanel>("密码不一致，请重试");
            return;
        }
        MsgRegister msg = new MsgRegister();
        msg.id = idInput.text;
        msg.pw = pwInput.text;
        NetManager.Send(msg);
    }
    void OnMsgRegister(MsgBase msg)
    {
        MsgRegister ret = (MsgRegister)msg;
        if (ret.result == 0)
        {
            Debug.Log("注册成功");
            PanelManager.Open<TipPanel>("注册成功");
            Close();
        }
        else if (ret.result == 1)
        {
            Debug.Log("注册失败");
            PanelManager.Open<TipPanel>("注册失败");
        }
    }
}
