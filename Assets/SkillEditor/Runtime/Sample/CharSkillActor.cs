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

    public Transform GetBone(VFXBindPoint point, string customName = "")
    {
        var animator = owner.GetComponent<Animator>();
        // 例如：
        switch (point)
        {
            case VFXBindPoint.Root:
                return owner.transform;
            case VFXBindPoint.Body:
                return animator != null ? animator.GetBoneTransform(HumanBodyBones.Spine) : owner.transform; // 优先获取 Spine 作为身体中心
            case VFXBindPoint.Head:
                return animator != null ? animator.GetBoneTransform(HumanBodyBones.Head) : owner.transform; // 优先获取 Head
            case VFXBindPoint.LeftHand:
                return animator != null ? animator.GetBoneTransform(HumanBodyBones.LeftHand) : owner.transform; // 优先获取 LeftHand
            case VFXBindPoint.RightHand:
                return animator != null ? animator.GetBoneTransform(HumanBodyBones.RightHand) : owner.transform; // 优先获取 RightHand
            case VFXBindPoint.WeaponLeft:
                return owner.transform.Find("WeaponLeftHolder"); 
            case VFXBindPoint.WeaponRight:
                return owner.transform.Find("WeaponRightHolder");
            default:
                return owner.transform; // 默认返回根节点
        }
    }
}
