using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListPanel : BasePanel
{
    private GameObject roomItem;
    private Transform content;
    // 自动生成的UI组件字段
    private Text NameText;
    private Text ScoreText;
    private Button CreateBtn;
    private Button ReflashBtn;
    private const string roomItemPath = "Assets/Resources/UiPrefab/RoomItem.prefab";
    private void BindUIComponents()
    {
        roomItem = ResManager.LoadPrefabAtPath(roomItemPath);
        content = selfObject.transform.Find("View/Right/RoomList/ListPanel/ScrollView/Viewport/Content");
        // 自动绑定UI组件
        NameText = selfObject.transform.Find("View/Left/InfoPanel/Info/NameText").GetComponent<Text>();
        ScoreText = selfObject.transform.Find("View/Left/InfoPanel/Info/ScoreText").GetComponent<Text>();
        CreateBtn = selfObject.transform.Find("View/Left/CtrlPanel/Button/CreateBtn").GetComponent<Button>();
        ReflashBtn = selfObject.transform.Find("View/Left/CtrlPanel/Button/ReflashBtn").GetComponent<Button>();
    }



    //初始化
    public override void OnInit()
    {
        path = "RoomListPanel";
        base.OnInit();
        layer = PanelLayer.Panel;
    }
    //显示
    public override void OnShow(params object[] args)
    {
        BindUIComponents();
        CreateBtn.onClick.AddListener(OnCreateBtnClick);
        ReflashBtn.onClick.AddListener(OnReflashBtnClick);

        //协议
        //监听
        NetManager.AddMsgEventListener("MsgGetAchieve", OnMsgGetAchieve);
        NetManager.AddMsgEventListener("MsgGetRoomList", OnMsgGetRoomList);
        NetManager.AddMsgEventListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.AddMsgEventListener("MsgEnterRoom", OnMsgEnterRoom);
        //发送
        MsgGetAchieve msgGetAchieve = new MsgGetAchieve();
        NetManager.Send(msgGetAchieve);
        MsgGetRoomList msgGetRoomList = new MsgGetRoomList();
        NetManager.Send(msgGetRoomList);
    }
    private void OnMsgGetAchieve(MsgBase msg)
    {
        MsgGetAchieve msgGetAchieve = (MsgGetAchieve)msg;
        NameText.text = msgGetAchieve.name;
        ScoreText.text = msgGetAchieve.win + "胜 " + msgGetAchieve.lose + "负";
    }
    private void OnMsgGetRoomList(MsgBase msg)
    {
        MsgGetRoomList msgGetRoomList = (MsgGetRoomList)msg;
        for(int i = 0;i<content.childCount;i++)
        {
            if(content.GetChild(i).gameObject!=null) Destroy(content.GetChild(i).gameObject);
        }
        if(msgGetRoomList.roomList==null || msgGetRoomList.roomList.Length==0)
        {
            return;
        }
        foreach (RoomInfo roomInfo in msgGetRoomList.roomList)
        {
            GenerateRoomItem(roomInfo);
        }
    }

    private void GenerateRoomItem(RoomInfo roomInfo)
    {
        GameObject obj = GameObject.Instantiate(roomItem, content);
        obj.transform.SetParent(content);
        obj.SetActive(true);
        obj.transform.localScale = Vector3.one;

        Text IdText = obj.transform.Find("Text/IdText").GetComponent<Text>();
        Text CountText = obj.transform.Find("Text/CountText").GetComponent<Text>();
        Text StatusText = obj.transform.Find("Text/StatusText").GetComponent<Text>();
        Button JoinBtn = obj.transform.Find("JoinBtn").GetComponent<Button>();

        IdText.text = roomInfo.roomId.ToString();
        CountText.text = roomInfo.playerCount + "/" + RoomInfo.maxPlayerCount;
        StatusText.text = roomInfo.roomState == 0 ? "等待中" : "游戏中";
        JoinBtn.onClick.AddListener(()=>OnJoinBtnClick(IdText.text));
    }

    private void OnJoinBtnClick(string roomId)
    {
        MsgEnterRoom msgEnterRoom = new MsgEnterRoom();
        msgEnterRoom.roomId = int.Parse(roomId);
        NetManager.Send(msgEnterRoom);
    }

    private void OnMsgCreateRoom(MsgBase msg)
    {
        MsgCreateRoom msgCreateRoom = (MsgCreateRoom)msg;
        if (msgCreateRoom.result == 0)
        {
            PanelManager.Open<TipPanel>("创建房间成功");
            PanelManager.Open<RoomPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("创建房间失败");
        }
    }

    private void OnMsgEnterRoom(MsgBase msg)
    {
        MsgEnterRoom msgEnterRoom = (MsgEnterRoom)msg;
        if(msgEnterRoom.result==0)
        {
            PanelManager.Open<RoomPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("加入房间失败");
        }
    }

    void OnCreateBtnClick()
    {
        MsgCreateRoom msgCreateRoom = new MsgCreateRoom();
        NetManager.Send(msgCreateRoom);
    }
    void OnReflashBtnClick()
    {
        MsgGetRoomList msgGetRoomList = new MsgGetRoomList();
        NetManager.Send(msgGetRoomList);
    }
    //关闭
    public override void OnClose()
    {
        //移除监听
        NetManager.RemoveMsgEventListener("MsgGetAchieve", OnMsgGetAchieve);
        NetManager.RemoveMsgEventListener("MsgGetRoomList", OnMsgGetRoomList);
        NetManager.RemoveMsgEventListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.RemoveMsgEventListener("MsgEnterRoom", OnMsgEnterRoom);
    }
}
