using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatttleManager
{
    public static readonly Dictionary<string, BaseTank> tanks = new Dictionary<string, BaseTank>();
    static GameObject terrain;
    public static void Init()
    {
        NetManager.AddMsgEventListener("MsgEnterBattle", OnMsgEnterBattle);
        NetManager.AddMsgEventListener("MsgBattleResult", OnMsgBattleResult);
        NetManager.AddMsgEventListener("MsgLeaveBattle", OnMsgLeaveBattle);

        NetManager.AddMsgEventListener("MsgSyncTank", OnMsgSyncTank);
        NetManager.AddMsgEventListener("MsgFire", OnMsgFire);
        NetManager.AddMsgEventListener("MsgHit", OnMsgHit);
    }
    private static void OnMsgEnterBattle(MsgBase msgBase)
    {
        MsgEnterBattle msg = (MsgEnterBattle)msgBase;
        EnterBattle(msg);
    }

    private static void OnMsgBattleResult(MsgBase msgBase)
    {
        MsgBattleResult msg = (MsgBattleResult)msgBase;
        //判断显示胜利还是失败
        bool isWin = false;
        BaseTank tank = GetCtrlTank();
        if (tank != null & tank.camp == msg.winCamp)
        {
            isWin = true;
        }
        PanelManager.Open<ResultPanel>(isWin);
    }

    private static void OnMsgLeaveBattle(MsgBase msgBase)
    {
        MsgLeaveBattle msg = (MsgLeaveBattle)msgBase;
        //销毁地形
        if (terrain != null) GameObject.Destroy(terrain);
        terrain = null;
        //查找坦克
        BaseTank tank = GetTank(msg.id);
        if (tank != null)
        {
            //销毁坦克
            GameObject.Destroy(tank.gameObject);
            //移除字典
            RemoveTank(msg.id);
        }
    }
    private static void OnMsgSyncTank(MsgBase msgBase)
    {
        MsgSyncTank msg = (MsgSyncTank)msgBase;
        //不同步自己
        if(msg.id == UiTest1.id)
        {
            return;
        }
        SyncTank tank = (SyncTank)GetTank(msg.id);
        if (tank == null)
        {
            return;
        }
        //移动同步
        tank.SyncPos(msg);
    }

    private static void OnMsgFire(MsgBase msgBase)
    {
        MsgFire msg = (MsgFire)msgBase;
        //不同步自己
        if (msg.id == UiTest1.id)
        {
            return;
        }
        SyncTank tank = (SyncTank)GetTank(msg.id);
        if (tank == null)
        {
            return;
        }
        //开火同步
        tank.SyncFire(msg);
    }

    private static void OnMsgHit(MsgBase msgBase)
    {
        MsgHit msg = (MsgHit)msgBase;
        //不同步自己
        if (msg.id == UiTest1.id)
        {
            return;
        }
        SyncTank tank = (SyncTank)GetTank(msg.id);
        if (tank == null)
        {
            return;
        }
        //受击同步
        tank.Attacked(msg);
    }
    public static void EnterBattle(MsgEnterBattle msg)
    {
        Reset();
        PanelManager.Close("RoomPanel");
        PanelManager.Close("ResultPanel");
        LoadEnvironment();
        for (int i = 0; i < msg.tanks.Length; i++)
        {
            GenerateTank(msg.tanks[i]);
        }
    }
    public static void LoadEnvironment()
    {
        if(terrain!=null)
        {
            return;
        }
        GameObject envPrefab = (GameObject)ResManager.LoadPrefabAtPath("Assets/Resources/Prefab/Terrain/Terrain.prefab");
        terrain = GameObject.Instantiate(envPrefab);
        terrain.transform.position = Vector3.zero;
    }
    public static void GenerateTank(TankInfo info)
    {
        string objName = "Tanl_" + info.id;
        GameObject tankObj = new GameObject(objName);
        //AddComponent
        BaseTank tank = null;
        if (info.id == UiTest1.id)
        {
            tank = tankObj.AddComponent<CtrlTank>();
        }
        else
        {
            tank = tankObj.AddComponent<SyncTank>();
        }
        if (info.id == UiTest1.id)
        {
            tankObj.AddComponent<CameraFollow>();
        }
        //属性
        tank.id = info.id;
        tank.camp = info.camp;
        tank.hp = info.hp;
        tank.transform.position = new Vector3(info.x, info.y, info.z);
        tank.transform.eulerAngles = new Vector3(info.ex, info.ey, info.ez);
        //init
        if (info.camp == 1)
        {
            tank.Init("Assets/Resources/Prefab/Tank/tankPrefab_Red.prefab");
        }
        else
        {
            tank.Init("Assets/Resources/Prefab/Tank/tankPrefab_Blue.prefab");
        }
        AddTank(info.id, tank);
    }
    public static void AddTank(string id, BaseTank tank)
    {
        tanks[id] = tank;
    }
    public static void RemoveTank(string id)
    {
        tanks.Remove(id);
    }
    public static BaseTank GetTank(string id)
    {
        if (tanks.ContainsKey(id))
        {
            return tanks[id];
        }
        return null;
    }
    public static BaseTank GetCtrlTank()
    {
        return GetTank(UiTest1.id);
    }
    public static void Reset()
    {
        foreach (var tank in tanks.Values)
        {
            GameObject.Destroy(tank.gameObject);
        }
        tanks.Clear();
    }
}
