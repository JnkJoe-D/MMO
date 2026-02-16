using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : BasePanel
{
    private Transform content;
    private GameObject playerItem;
    // 自动生成的UI组件字段
    private Button StartBtn;
    private Button ExitBtn;

    private void BindUIComponents()
    {
        // 自动绑定UI组件
        StartBtn = selfObject.transform.Find("View/Button/StartBtn").GetComponent<Button>();
        ExitBtn = selfObject.transform.Find("View/Button/ExitBtn").GetComponent<Button>();
    }
    //初始化
    public override void OnInit()
    {
        path = "RoomPanel";
        base.OnInit();
        layer = PanelLayer.Panel;
    }
    //显示
    public override void OnShow(params object[] args)
    {
        base.OnShow(args);
        playerItem = ResManager.LoadPrefabAtPath("Assets/Resources/UiPrefab/PlayerItem.prefab");
        content = selfObject.transform.Find("View/ScrollView/Viewport/Content");
        BindUIComponents();
        StartBtn.onClick.AddListener(OnStartBtnClick);
        ExitBtn.onClick.AddListener(OnExitBtnClick);

        //协议
        //监听
        NetManager.AddMsgEventListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
        NetManager.AddMsgEventListener("MsgLeaveRoom", OnMsgLeaveRoom);
        NetManager.AddMsgEventListener("MsgStartBattle", OnMsgStartBattle);
        //发送
        MsgGetRoomInfo msgGetRoomInfo = new MsgGetRoomInfo();
        NetManager.Send(msgGetRoomInfo);
    }

    private void OnMsgStartBattle(MsgBase msg)
    {
        MsgStartBattle msgStartBattle = (MsgStartBattle)msg;
        if(msgStartBattle.result==0)
        {
            Debug.Log("开始游戏");
            // MsgStartBattle msgsb = new MsgStartBattle();
            // NetManager.Send(msgsb);
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("开始游戏失败");
        }
    }

    private void GeneratePlayerItem(PlayerInfo playerInfo)
    {
        GameObject go = GameObject.Instantiate(playerItem, content);
        go.transform.SetParent(content);
        go.transform.localScale = Vector3.one;
        go.SetActive(true);

        Text IdText = go.transform.Find("Text/IdText").GetComponent<Text>();
        Text CampText = go.transform.Find("Text/CampText").GetComponent<Text>();
        Text ScoreText = go.transform.Find("Text/ScoreText").GetComponent<Text>();

        IdText.text = playerInfo.id;
        CampText.text = playerInfo.camp == 1 ? "红方" : "蓝方";
        ScoreText.text = playerInfo.win + "胜 " + playerInfo.lost + "负";
        if (playerInfo.isOwner == 1)
        {
            IdText.text += "(房主)";
        }
    }

    private void OnMsgLeaveRoom(MsgBase msg)
    {
        MsgLeaveRoom msgLeaveRoom = (MsgLeaveRoom)msg;
        if(msgLeaveRoom.result==0)
        {
            PanelManager.Open<TipPanel>("离开房间");
            PanelManager.Open<RoomListPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("离开房间失败");
        }
    }

    private void OnMsgGetRoomInfo(MsgBase msg)
    {
        MsgGetRoomInfo msgGetRoomInfo = (MsgGetRoomInfo)msg;
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        if (msgGetRoomInfo.playerList == null || msgGetRoomInfo.playerList.Length == 0)
        {
            return;
        }
        for (int i = 0; i < msgGetRoomInfo.playerList.Length; i++)
        {
            GeneratePlayerItem(msgGetRoomInfo.playerList[i]);
        }
    }

    void OnStartBtnClick()
    {
        MsgStartBattle msgStartBattle = new MsgStartBattle();
        NetManager.Send(msgStartBattle);
    }
    void OnExitBtnClick()
    {
        MsgLeaveRoom msgLeaveRoom = new MsgLeaveRoom();
        NetManager.Send(msgLeaveRoom);
    }
    //关闭
    public override void OnClose()
    {
        base.OnClose();
        //监听
        NetManager.RemoveMsgEventListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
        NetManager.RemoveMsgEventListener("MsgLeaveRoom", OnMsgLeaveRoom);
        NetManager.RemoveMsgEventListener("MsgStartBattle", OnMsgStartBattle);
    }
}
