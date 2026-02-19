using System;
using UnityEngine;
using SkillEditor;
using Game.MAnimSystem;

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
                // 注意：这里返回任何一个可靠的 MonoBehaviour 即可
                // 如果 GameClient 有专门的 SkillRunnerComponent 最好，否则复用 AnimComponent 也行
                // 为了通用性，我们尝试获取一个名为 "SkillRunner" 的组件，或者 fallback 到 AnimComponent
                
                var mb = _owner.GetComponent<MonoBehaviour>(); // 获取任意一个
                return mb;
            }

            // 3. 技能角色服务
            if (serviceType == typeof(ISkillActor))
            {
                 return new CharSkillActor(_owner);
            }

            return null;
        }
    }
}
