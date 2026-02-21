using UnityEngine;

namespace SkillEditor
{
    /// <summary>
    /// 技能投射物接口
    /// 挂载在投射物/召唤物 Prefab 上的核心组件，接管生成后的生命周期、轨道飞行、碰撞检测。
    /// 可以接受来自外部服务（如 ProjectileManager）的Tick驱动，也可以自己在内部 Update。
    /// </summary>
    public interface ISkillProjectile
    {
        /// <summary>
        /// 投射物被生成时的初始化数据灌入
        /// </summary>
        /// <param name="eventTag">技能传递过来的参数（例如伤害ID的标识）</param>
        /// <param name="spawnPosition">初始世界坐标</param>
        /// <param name="spawnRotation">初始世界旋转</param>
        /// <param name="context">技能执行上下文（如果需要反算施法者等信息）</param>
        void Initialize(string eventTag, Vector3 spawnPosition, Quaternion spawnRotation, ProcessContext context);

        /// <summary>
        /// 销毁逻辑（如果是被中断等外部强制回收）
        /// </summary>
        void Terminate();
    }
}
