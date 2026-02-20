using System.Collections;
using System.Collections.Generic;
using SkillEditor;
using UnityEngine;

public class CharSkillActor : ISkillActor
{
    private GameObject owner;
    public CharSkillActor(GameObject owner)
    {
        this.owner = owner;
    }

    public Transform GetBone(BindPoint point, string customName = "")
    {
        var animator = owner.GetComponent<Animator>();
        // 例如：
        switch (point)
        {
            case BindPoint.Root:
                return owner.transform;
            case BindPoint.Body:
                return animator != null ? animator.GetBoneTransform(HumanBodyBones.Spine) : owner.transform; // 优先获取 Spine 作为身体中心
            case BindPoint.Head:
                return animator != null ? animator.GetBoneTransform(HumanBodyBones.Head) : owner.transform; // 优先获取 Head
            case BindPoint.LeftHand:
                return animator != null ? animator.GetBoneTransform(HumanBodyBones.LeftHand) : owner.transform; // 优先获取 LeftHand
            case BindPoint.RightHand:
                return animator != null ? animator.GetBoneTransform(HumanBodyBones.RightHand) : owner.transform; // 优先获取 RightHand
            case BindPoint.WeaponLeft:
                return owner.transform.Find("WeaponLeftHolder"); 
            case BindPoint.WeaponRight:
                return owner.transform.Find("WeaponRightHolder");
            default:
                return owner.transform; // 默认返回根节点
        }
    }
}
