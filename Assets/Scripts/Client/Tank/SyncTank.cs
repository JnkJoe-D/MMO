using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SyncTank : BaseTank
{
    //预测信息，哪个时间到达哪个位置
    private Vector3 lastPos;
    private Vector3 lastRot;
    private Vector3 forecastPos; //预测位置
    private Vector3 forecastRot; //预测旋转
    private float forecastTime; //最近一次收到同步协议的时间

    public override void Init(string skinPath)
    {
        base.Init(skinPath);

        //不受物理影响
        rigid.constraints = RigidbodyConstraints.FreezeAll;
        rigid.useGravity = false;
        //初始化预测信息
        lastPos = transform.position;
        lastRot = transform.eulerAngles;
        forecastPos = transform.position;
        forecastRot = transform.eulerAngles;
        forecastTime = Time.time;
    }
    new void Update()
    {
        base.Update();
        //更新位置
        ForecastUpdate();
    }
    public void ForecastUpdate()
    {
        //时间
        float t = (Time.time - forecastTime) / CtrlTank.syncInterval;
        t = Mathf.Clamp01(t);
        //位置
        Vector3 pos = transform.position;
        pos = Vector3.Lerp(lastPos, forecastPos, t);
        transform.position = pos;
        //旋转
        Quaternion quat = transform.rotation;
        Quaternion forecastQuat = Quaternion.Euler(forecastRot);
        quat = Quaternion.Lerp(quat, forecastQuat, t);
        transform.rotation = quat;
    }
    public void SyncPos(MsgSyncTank msg)
    {
        //预测位置
        Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
        Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);
        forecastPos = pos + 2 * (pos - lastPos); //简单预测
        forecastRot = rot + 2 * (rot - lastRot);
        //更新
        lastPos = pos;
        lastRot = rot;
        forecastTime = Time.time;
        //炮塔
        Vector3 le = turret.localEulerAngles;
        le.y = msg.turretY;
        turret.localEulerAngles = le;
    }
    public void SyncFire(MsgFire msg)
    {
        Bullet bullet = Fire();
        //更新坐标
        Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
        Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);
        bullet.transform.position = pos;
        bullet.transform.eulerAngles = rot;
    }
}
