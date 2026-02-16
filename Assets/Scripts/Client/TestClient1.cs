using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClient1 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CtrlTank tank = this.gameObject.AddComponent<CtrlTank>();
        tank.Init("TankPrefab/tankPrefab");
        this.gameObject.AddComponent<CameraFollow>();

        GameObject hitTankObj = new GameObject("HitTank");
        BaseTank hitTank = hitTankObj.AddComponent<BaseTank>();
        hitTank.Init("TankPrefab/tankPrefab");
        hitTankObj.transform.position = new Vector3(0, 10, 30);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
