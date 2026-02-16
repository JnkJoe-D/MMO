using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHuman : MonoBehaviour
{
    //是否在移动
    protected bool isMoving = false;
    //移动目标点
    protected Vector3 targetPos;
    //移动速度
    protected float moveSpeed = 1.2f;
    //动画组件
    protected Animator animator;
    //描述
    protected string desc = "";

    //移动到某处
    public void MoveTo(Vector3 pos)
    {
        targetPos = pos;
        isMoving = true;
        animator.SetBool("isMoving", true);
    }

    //移动Update
    public void MoveUpdate()
    {
        if (!isMoving) return;

        Vector3 pos = transform.position;
        transform.LookAt(targetPos);
        transform.position = Vector3.MoveTowards(pos, targetPos, moveSpeed * Time.deltaTime);
        Debug.Log(desc + " MoveUpdate:" + transform.position+" to " + targetPos);
        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
        }
    }

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    }
    protected virtual void Update()
    {
        MoveUpdate();
    }
}
