using UnityEngine;

namespace SkillEditor
{
    /// <summary>
    /// 技能生成处理器接口
    /// 战斗系统需实现此接口，用于处理投射物/召唤物的实例化
    /// </summary>
    public interface ISkillSpawnHandler
    {
        /// <summary>
        /// 技能请求生成物体
        /// </summary>
        /// <param name="prefab">待生成的预制体</param>
        /// <param name="position">计算后的初始世界坐标</param>
        /// <param name="rotation">计算后的初始世界旋转</param>
        /// <param name="eventTag">配置在 Timeline 上的技能标识（如传递给投射物的参数包ID）</param>
        /// <param name="detach">是否脱离父节点挂载到世界根节点</param>
        /// <param name="parent">可选的父节点。如果不脱离，则挂载于此</param>
        /// <returns>生成的投射物逻辑控制接口，可用于被强行打断时回收</returns>
        ISkillProjectile SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation, string eventTag, bool detach, Transform parent);
        
        /// <summary>
        /// 技能提早或意外中断时，请求销毁相关的生成物
        /// </summary>
        /// <param name="projectile">之前生成的实例接口</param>
        void DestroySpawnedObject(ISkillProjectile projectile);
    }
}
