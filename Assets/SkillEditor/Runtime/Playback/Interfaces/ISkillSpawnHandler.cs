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
        /// <param name="eventTag">配置在 Timeline 上的技能标识（如传递给投射物的伤害ID）</param>
        /// <param name="initialVelocity">配置的初速度</param>
        /// <param name="detach">是否脱离父节点</param>
        /// <param name="parent">可选的父节点。如果不脱离，则挂载于此</param>
        /// <returns>生成的 GameObject 实例，可用于控制生命周期</returns>
        GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation, string eventTag, Vector3 initialVelocity, bool detach, Transform parent);
        
        /// <summary>
        /// 技能提早或意外中断时，请求销毁相关的生成物
        /// </summary>
        /// <param name="spawnedObject">之前生成的实例</param>
        void DestroySpawnedObject(GameObject spawnedObject);
    }
}
