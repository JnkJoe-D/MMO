using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 查询战绩
/// </summary>
public class MsgGetAchieve : MsgBase
{
    public MsgGetAchieve()
    {
        protoName = "MsgGetAchieve";
    }
    public string name = "";
    public int win = 0;
    public int lose = 0;
}
/// <summary>
/// 房间信息
/// </summary>
[Serializable]
public class RoomInfo
{
    public int roomId;
    public string roomName;
    public int playerCount;
    public const int maxPlayerCount = 6;
    public int roomState; //0等待中 1游戏中
}
/// <summary>
/// 查询房间列表
/// </summary>
public class MsgGetRoomList : MsgBase
{
    public MsgGetRoomList()
    {
        protoName = "MsgGetRoomList";
    }
    public RoomInfo[] roomList;
}
/// <summary>
/// 创建房间
/// </summary>
public class MsgCreateRoom : MsgBase
{
    public MsgCreateRoom()
    {
        protoName = "MsgCreateRoom";
    }
    //服务端回
    public int result; //0成功 1失败
}
public class MsgEnterRoom : MsgBase
{
    public MsgEnterRoom()
    {
        protoName = "MsgEnterRoom";
    }
    //客户端发
    public int roomId;
    //服务端回
    public int result; //0成功 1失败
}
/// <summary>
/// 房间内玩家信息
/// </summary>
[Serializable]
public class PlayerInfo
{
    public string id;
    public int camp;
    public int win;
    public int lost;
    public int isOwner;
}
/// <summary>
/// 查询房间内信息
/// </summary>
public class MsgGetRoomInfo : MsgBase
{
    public MsgGetRoomInfo()
    {
        protoName = "MsgGetRoomInfo";
    }
    public PlayerInfo[] playerList;
}
public class MsgLeaveRoom : MsgBase
{
    public MsgLeaveRoom()
    {
        protoName = "MsgLeaveRoom";
    }
    //服务端回
    public int result; //0成功 1失败
}
public class MsgStartBattle : MsgBase
{
    public MsgStartBattle()
    {
        protoName = "MsgStartBattle";
    }
    //服务端回
    public int result; //0成功 1失败
}
