using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //子弹速度
    public float speed = 100f;
    //发射者
    public BaseTank tank;
    //炮弹模型
    public GameObject skin;
    //刚体
    Rigidbody rigid;
    public void Init(BaseTank tank)
    {
        this.tank = tank;
        if (skin != null)
        {
            Destroy(skin);
        }
        skin = Instantiate((GameObject)ResManager.LoadPrefabAtPath("Assets/Resources/TankPrefab/bulletPrefab.prefab"));
        skin.transform.SetParent(this.transform);
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localRotation = Quaternion.identity;

        rigid = gameObject.AddComponent<Rigidbody>();
        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = 0.3f;
        sphereCollider.isTrigger = true;
        rigid.useGravity = true;
        Destroy(this.gameObject, 5f);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.IsChildOf(tank.transform))
        {
            return;
        }
        BaseTank hitTank = other.GetComponentInParent<BaseTank>();
        if (hitTank != null)
        {
            Debug.Log("Hit Tank");
            MsgHit msg = new MsgHit();
            msg.targetId = hitTank.id;
            msg.id = tank.id;
            msg.damage = 35;
            NetManager.Send(msg);
        }
        else
        {
            Debug.Log("Hit Wall");
        }
        GameObject explode = ResManager.LoadPrefabAtPath("Assets/Resources/Particles/fire.prefab");
        Instantiate(explode, this.transform.position, transform.rotation);
        Destroy(this.gameObject);
    }
    void Update()
    {
        Vector3 deltaZ = this.transform.forward * speed * Time.deltaTime;
        this.transform.position += deltaZ;
    }
}
