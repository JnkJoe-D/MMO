using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestNotepad : MonoBehaviour
{
    public InputField idInput;
    public InputField pwInput;
    public InputField noteInput;
    public Button conectBtn;
    public Button disconectBtn;
    public Button loginBtn;
    public Button regBtn;
    public Button saveBtn;
    void Start()
    {
        NetManager.AddNetEventListener(NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddNetEventListener(NetEvent.ConnectFail, OnConnectFail);
        NetManager.AddNetEventListener(NetEvent.Close, OnClose);
        NetManager.AddMsgEventListener("MsgLogin", OnMsgLogin);
        NetManager.AddMsgEventListener("MsgRegister", OnMsgRegister);
        NetManager.AddMsgEventListener("MsgKick", OnMsgKick);
        NetManager.AddMsgEventListener("MsgGetText", OnMsgGetText);
        NetManager.AddMsgEventListener("MsgSaveText", OnMsgSaveText);
        conectBtn.onClick.AddListener(OnConnectClick);
        disconectBtn.onClick.AddListener(OnDisConnectClick);
        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegClick);
        saveBtn.onClick.AddListener(OnSaveClick);
    }
    void Update()
    {
        NetManager.MsgUpdate();
    }
    //系统事件
    void OnConnectSucc(string str)
    {
        Debug.Log("连接服务器成功回调");
    }
    void OnConnectFail(string str)
    {
        Debug.Log("连接服务器失败回调" + str);
    }
    void OnClose(string str)
    {
        Debug.Log("断开服务器连接回调" + str);
    }
    //按钮事件
    private void OnConnectClick()
    {
        NetManager.Connect("127.0.0.1", 33333);
    }
    private void OnDisConnectClick()
    {
        NetManager.Close();
    }
    private void OnLoginClick()
    {
        MsgLogin msg = new MsgLogin();
        msg.id = idInput.text;
        msg.pw = pwInput.text;
        NetManager.Send(msg);
    }
    private void OnRegClick()
    {
        MsgRegister msg = new MsgRegister();
        msg.id = idInput.text;
        msg.pw = pwInput.text;
        NetManager.Send(msg);
    }
    private void OnSaveClick()
    {
        MsgSaveText msg = new MsgSaveText();
        msg.text = noteInput.text;
        NetManager.Send(msg);
    }
    //消息事件
    public void OnMsgLogin(MsgBase msg)
    {
        MsgLogin m = (MsgLogin)msg;
        if (m.result == 0)
        {
            Debug.Log("登录成功");
            MsgGetText msg2 = new MsgGetText();
            NetManager.Send(msg2);
        }
        else
        {
            Debug.Log("登录失败");
        }
    }
    public void OnMsgRegister(MsgBase msg)
    {
        MsgRegister m = (MsgRegister)msg;
        if (m.result == 0)
        {
            Debug.Log("注册成功");
        }
        else
        {
            Debug.Log("注册失败");
        }
    }
    public void OnMsgKick(MsgBase msg)
    {
        MsgKick m = (MsgKick)msg;
        if (m.reason == 0)
        {
            Debug.Log("被踢出，原因：别人重复登录");
        }
    }
    public void OnMsgGetText(MsgBase msg)
    {
        MsgGetText m = (MsgGetText)msg;
        noteInput.text = m.text;
    }
    public void OnMsgSaveText(MsgBase msg)
    {
        MsgSaveText m = (MsgSaveText)msg;
        if (m.result == 0)
        {
            Debug.Log("保存成功");
        }
        else
        {
            Debug.Log("保存失败");
        }
    }
}
