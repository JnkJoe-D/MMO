using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class TankInfo
{
    public string id = "";
    public int camp;
    public int hp;
    public float x;
    public float y;
    public float z;
    public float ex;
    public float ey;
    public float ez;
}
public class MsgEnterBattle : MsgBase
    {
        public MsgEnterBattle() { protoName = "MsgEnterBattle"; }
        //服务端回
        public TankInfo[] tanks;
        public int mapId = 1;
    }
    public class MsgBattleResult : MsgBase
    {
        public MsgBattleResult() { protoName = "MsgBattleResult"; }
        //服务端回
        public int winCamp = 0;
    }
public class MsgLeaveBattle : MsgBase
{
    public MsgLeaveBattle() { protoName = "MsgExitBattle"; }
    //服务端回
    public string id = "";
}
public class MsgSyncTank : MsgBase
{
    public MsgSyncTank() { protoName = "MsgSyncTank"; }
    public float x = 0f;
    public float y = 0f;
    public float z = 0f;
    public float ex = 0f;
    public float ey = 0f;
    public float ez = 0f;
    public float turretY = 0f; //炮塔旋转角度
                               //服务端补充
    public string id = ""; //哪个坦克
}
public class MsgFire : MsgBase
{
    public MsgFire() { protoName = "MsgFire"; }
    //炮弹初始位置、旋转
    public float x = 0f;
    public float y = 0f;
    public float z = 0f;
    public float ex = 0f;
    public float ey = 0f;
    public float ez = 0f;
    //服务端补充
    public string id = ""; //哪个坦克
}
public class MsgHit : MsgBase
{
    public MsgHit() { protoName = "MsgHit"; }
    //击中谁
    public string targetId = "";
    //击中点
    public float x = 0f;
    public float y = 0f;
    public float z = 0f;
    //服务端补充
    public string id = ""; //哪个坦克
    public int remainHp = 0;  //被命中的坦克的伤害
    public int damage = 0; //受到的伤害
}
