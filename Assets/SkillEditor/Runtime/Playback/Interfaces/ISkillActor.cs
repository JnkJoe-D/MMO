using UnityEngine;

namespace SkillEditor
{
    /// <summary>
    /// 特效挂载点枚举
    /// </summary>
    public enum VFXBindPoint
    {
        Root,           // 根节点
        Body,           // 身体中心 (胸口/骨盆)
        Head,           // 头部
        LeftHand,       // 左手
        RightHand,      // 右手
        WeaponLeft,     // 左手武器
        WeaponRight,    // 右手武器
        CustomBone      // 自定义骨骼 (需配合 string boneName)
    }

    /// <summary>
    /// 技能角色接口
    /// 角色脚本需实现此接口，以便 SkillEditor 获取挂点
    /// </summary>
    public interface ISkillActor
    {
        /// <summary>
        /// 获取特效挂载点 Transform
        /// </summary>
        /// <param name="point">挂点类型</param>
        /// <param name="customName">自定义骨骼名（仅当 point 为 CustomBone 时使用）</param>
        /// <returns>挂点 Transform</returns>
        Transform GetBone(VFXBindPoint point, string customName = "");
    }
}
