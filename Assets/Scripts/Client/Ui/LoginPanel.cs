using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : BasePanel
{
    InputField idInput;
    InputField pwInput;
    Button loginBtn;
    Button regBtn;
    public override void OnInit()
    {
        path = "LoginPanel";
        base.OnInit();
        layer = PanelLayer.Panel;
    }
    public override void OnShow(params object[] args)
    {
        base.OnShow(args);
        //组件
        idInput = selfObject.transform.Find("View/Main/Id/IdInput").GetComponent<InputField>();
        pwInput = selfObject.transform.Find("View/Main/Pw/PwInput").GetComponent<InputField>();
        loginBtn = selfObject.transform.Find("View/Main/Button/LoginBtn").GetComponent<Button>();
        regBtn = selfObject.transform.Find("View/Main/Button/RegBtn").GetComponent<Button>();
        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegClick);
        //网络
        //协议
        NetManager.AddMsgEventListener("MsgLogin", OnMsgLogin);
        //事件
        NetManager.AddNetEventListener(NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddNetEventListener(NetEvent.ConnectFail, OnConnectFail);
        //开始连接
        NetManager.Connect("127.0.0.1", 33333);
    }



    public override void OnClose()
    {
        base.OnClose();
        //移除
        //协议
        NetManager.RemoveMsgEventListener("MsgLogin", OnMsgLogin);
        //事件
        NetManager.RemoveNetEventListener(NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.RemoveNetEventListener(NetEvent.ConnectFail, OnConnectFail);
    }
    void OnLoginClick()
    {
        //用户名密码为空
        if (string.IsNullOrEmpty(idInput.text) || string.IsNullOrEmpty(pwInput.text))
        {
            Debug.Log("用户名密码不能为空");
            PanelManager.Open<TipPanel>("用户名密码不能为空");
            return;
        }
        //发送登录协议
        MsgLogin msg = new MsgLogin();
        msg.id = idInput.text;
        msg.pw = pwInput.text;
        NetManager.Send(msg);
    }
    void OnRegClick()
    {
        PanelManager.Open<RegisterPanel>();
    }
    void OnMsgLogin(MsgBase msg)
    {
        MsgLogin ret = (MsgLogin)msg;
        if (ret.result == 0)
        {
            Debug.Log("登录成功");
            // PanelManager.Open<TipPanel>("登录成功");
            PanelManager.Open<RoomListPanel>();
            Close();
            UiTest1.id = ret.id;
        }
        else if (ret.result == 1)
        {
            Debug.Log("登录失败");
            PanelManager.Open<TipPanel>("登录失败");
        }
    }
    void OnConnectSucc(string str)
    {
        Debug.Log("连接成功");
    }
    void OnConnectFail(string str)
    {
        Debug.Log("连接失败");
        // PanelManager.Open<TipPanel>("连接服务器失败");
    }
}
