using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTank : MonoBehaviour
{
    GameObject skin;
    protected Rigidbody rigid;
    //转向速度
    public float rotateSpeed = 20;
    //移动速度
    public float moveSpeed = 3;
    //炮塔旋转速度
    public float turretSpeed = 30;
    //炮塔
    public Transform turret;
    //炮管
    public Transform gun;
    //炮口位置
    public Transform firePoint;
    //炮弹cd时间
    public float fireCD = 0.5f;
    //上次开火时间
    public float lastFireTime = 0;

    //血量
    public float hp = 100;
    public string id = "";
    public int camp = 0; //阵营 0红 1蓝
    protected void Update()
    {

    }
    public virtual void Init(string skinPath)
    {
        if (skin != null)
        {
            Destroy(skin);
        }
        skin = Instantiate(ResManager.LoadPrefabAtPath(skinPath));
        skin.transform.SetParent(this.transform);
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localRotation = Quaternion.identity;

        rigid = gameObject.AddComponent<Rigidbody>();
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(7, 5, 12);
        boxCollider.center = new Vector3(0, 3.5f, 1.47f);

        turret = skin.transform.Find("Turret");
        gun = turret.Find("Gun");
        firePoint = gun.Find("FirePoint");
    }

    public Bullet Fire()
    {
        if (IsDie())
        {
            return null;
        }
        GameObject bulletObj = new GameObject("Bullet");
        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.Init(this);
        bulletObj.transform.position = firePoint.position;
        bulletObj.transform.rotation = firePoint.rotation;
        lastFireTime = Time.time;
        return bullet;
    }
    public void FireCheck()
    {
        if (firePoint == null)
        {
            return;
        }
        if (Time.time - lastFireTime < fireCD)
        {
            return;
        }
        //fire
        Bullet bullet = Fire();
        MsgFire msg = new MsgFire();
        msg.id = id;
        msg.x = bullet.transform.position.x;
        msg.y = bullet.transform.position.y;
        msg.z = bullet.transform.position.z;
        msg.ex = bullet.transform.eulerAngles.x;
        msg.ey = bullet.transform.eulerAngles.y;
        msg.ez = bullet.transform.eulerAngles.z;
        NetManager.Send(msg);
    }
    public bool IsDie()
    {
        return hp <= 0;
    }
    public void Attacked(MsgHit msg)
    {
        if (IsDie())
        {
            return;
        }
        if (msg.id != id)
        {
            return;
        }
        hp = msg.remainHp;
        if (IsDie())
        {
            Debug.Log("Tank Die");
        }
    }
}
