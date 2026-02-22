using System;
using UnityEngine;
using SkillEditor;
using Game.MAnimSystem;
using Game.Pool;

namespace Game.Adapters
{
    public class SkillServiceFactory : IServiceFactory
    {
        private GameObject _owner;

        public SkillServiceFactory(GameObject owner)
        {
            _owner = owner;
        }

        public object ProvideService(Type serviceType)
        {
            // 1. 动画服务
            if (serviceType == typeof(ISkillAnimationHandler))
            {
                var animComp = _owner.GetComponent<AnimComponent>();
                if (animComp == null) return null;
                return new AnimComponentAdapter(animComp);
            }

            // 2. 协程服务 (返回 MonoBehaviour)
            // 优先查找现有的 MonoBehaviour 组件作为 Runner
            if (serviceType == typeof(MonoBehaviour))
            {
                var mb = _owner.GetComponent<MonoBehaviour>(); // 获取任意一个
                return mb;
            }

            // 3. 技能角色服务
            if (serviceType == typeof(ISkillActor))
            {
                 return new CharSkillActor(_owner);
            }

            //4. 音频管理服务
            if(serviceType == typeof(ISkillAudioHandler))
            {
                return _owner.AddComponent<GameSkillAudioHandler>();
            }

            //5. 伤害处理服务
            if(serviceType == typeof(ISkillDamageHandler))
            {
                return new DamageHandler();
            }

            // 6. VFX 对象池服务
            if (serviceType == typeof(IVFXPoolService))
            {
                return new VFXPoolServiceAdapter();
            }

            // 7. Spawn 服务
            if (serviceType == typeof(ISkillSpawnHandler))
            {
                return new SkillSpawnHandler();
            }
            return null;
        }
    }

    /// <summary>
    /// IVFXPoolService 适配器，委托给 GlobalPoolManager
    /// </summary>
    internal class VFXPoolServiceAdapter : IVFXPoolService
    {
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return GlobalPoolManager.Spawn(prefab, position, rotation, parent);
        }

        public void Return(GameObject instance)
        {
            GlobalPoolManager.Return(instance);
        }
    }
}
