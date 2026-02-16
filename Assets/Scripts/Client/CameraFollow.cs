using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //距离矢量
    public Vector3 distance = new Vector3(0, 8, -18);
    //相机
    new public Camera camera;
    //偏移值
    public Vector3 offset = new Vector3(0, 5f, 0);
    //相机移动速度
    public float moveSpeed = 3f;
    void Start()
    {
        if (camera == null)
        {
            camera = Camera.main;
        }
        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        Vector3 initPos = pos - forward * 30 + Vector3.up * 10;
        camera.transform.position = initPos;
    }
    void LateUpdate()
    {
        Vector3 targetPos = this.transform.position + distance.z * transform.forward; ;
        targetPos.y += distance.y;
        Vector3 cameraPos = camera.transform.position;
        cameraPos = Vector3.MoveTowards(cameraPos, targetPos, moveSpeed * Time.deltaTime);
        camera.transform.position = cameraPos;
        camera.transform.LookAt(this.transform.position + offset);
    }
}
