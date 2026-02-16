using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlTank : BaseTank
{
    //上一次发送同步信息的时间
    private float lastSendSyncTime = 0;
    //同步频率
    public static float syncInterval = 0.1f;
    new void Update()
    {
        base.Update();
        MoveUpdate();
        TurretUpdate();
        FireUpdate();
        SyncUpdate();
    }

    public void MoveUpdate()
    {
        if(IsDie())
        {
            return;
        }
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        this.transform.Rotate(Vector3.up * h * rotateSpeed * Time.deltaTime);
        Vector3 deltaZ = this.transform.forward * v * moveSpeed * Time.deltaTime;
        this.transform.position += deltaZ;
    }
    public void TurretUpdate()
    {
        if (IsDie())
        {
            return;
        }
        if (turret == null)
        {
            return;
        }
        float axis = 0;
        if (Input.GetKey(KeyCode.Q))
        {
            axis = -1;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            axis = 1;
        }
        if (axis == 0)
        {
            return;
        }
        Vector3 le = turret.localEulerAngles;
        le.y += axis * turretSpeed * Time.deltaTime;
        turret.localEulerAngles = le;
    }
    public void FireUpdate()
    {
        if (IsDie())
        {
            return;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            FireCheck();
        }
    }
    private void SyncUpdate()
    {
        if (Time.time - lastSendSyncTime < syncInterval)
        {
            return;
        }
        lastSendSyncTime = Time.time;
        //发送同步消息
        MsgSyncTank msg = new MsgSyncTank();
        msg.id = id;
        msg.x = this.transform.position.x;
        msg.y = this.transform.position.y;
        msg.z = this.transform.position.z;
        msg.ex = this.transform.eulerAngles.x;
        msg.ey = this.transform.eulerAngles.y;
        msg.ez = this.transform.eulerAngles.z;
        msg.turretY = turret.localEulerAngles.y;
        NetManager.Send(msg);
    }
    void OllisionEnter(Collision collision)
    {
        //打到的坦克
        GameObject coolObj = collision.gameObject;
        BaseTank hitTank = coolObj.GetComponent<BaseTank>();
        //不能打自己
        if (hitTank == this)
        {
            return;
        }
        //打到其他坦克
        if (hitTank != null)
        {
            SendHitMsg(this, hitTank);
        }
    }
    void SendHitMsg(BaseTank tank, BaseTank hitTank)
    {
        if (tank == null || hitTank == null)
        {
            return;
        }
        //不是自己发出的炮弹
        if (tank.id != UiTest1.id)
        {
            return;
        }
        MsgHit msg = new MsgHit();
        msg.targetId = hitTank.id;
        msg.id = tank.id;
        Vector3 hitPoint = hitTank.transform.position; //简单处理，打到坦克中心
        msg.x = hitPoint.x;
        msg.y = hitPoint.y;
        msg.z = hitPoint.z;
        NetManager.Send(msg);
    }
}
