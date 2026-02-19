using UnityEngine;
using System.Collections;

namespace SkillEditor
{
    [ProcessBinding(typeof(VFXClip), PlayMode.Runtime)]
    public class RuntimeVFXProcess : ProcessBase<VFXClip>
    {
        private struct ParticleSpeedInfo
        {
            public ParticleSystem ps;
            public float initialSpeed;
        }
        private ParticleSpeedInfo[] particleInfos;
        private GameObject vfxInstance;
        public override void OnEnter()
        {
            Debug.Log($"[RuntimeVFXProcess] OnEnter at time: {UnityEngine.Time.time}");
            if (clip.effectPrefab == null) return;

            // 1. 获取挂点
            Transform targetTransform = null;
            var actor = context.GetService<ISkillActor>(context.Owner.name);
            if (actor != null)
            {
                targetTransform = actor.GetBone(clip.bindPoint, clip.customBoneName);
            }
            
            // 降级处理
            if (targetTransform == null)
            {
                if (context.OwnerTransform != null) targetTransform = context.OwnerTransform;
            }

            Vector3 spawnPos = Vector3.zero;
            Quaternion spawnRot = Quaternion.identity;

            if (targetTransform != null)
            {
                spawnPos = targetTransform.position;
                spawnRot = targetTransform.rotation;
            }

            // 2. 实例化
            Transform parent = clip.followTarget ? targetTransform : null;
            vfxInstance = VFXPoolManager.Spawn(clip.effectPrefab, spawnPos, spawnRot, parent);

            if (vfxInstance != null)
            {
                // 3. 应用变换
                vfxInstance.transform.localScale = clip.scale;

                if (clip.followTarget)
                {
                    vfxInstance.transform.localPosition += clip.positionOffset;
                    vfxInstance.transform.localRotation *= Quaternion.Euler(clip.rotationOffset);
                }
                else
                {
                    if (targetTransform != null)
                    {
                        Vector3 finalPos = targetTransform.position + targetTransform.rotation * clip.positionOffset;
                        Quaternion finalRot = targetTransform.rotation * Quaternion.Euler(clip.rotationOffset);
                        vfxInstance.transform.SetPositionAndRotation(finalPos, finalRot);
                    }
                    else
                    {
                        vfxInstance.transform.position += clip.positionOffset;
                        vfxInstance.transform.rotation *= Quaternion.Euler(clip.rotationOffset);
                    }
                }

                // 4. 缓存粒子信息用于速度同步
                var systems = vfxInstance.GetComponentsInChildren<ParticleSystem>();
                particleInfos = new ParticleSpeedInfo[systems.Length];
                for (int i = 0; i < systems.Length; i++)
                {
                    particleInfos[i] = new ParticleSpeedInfo
                    {
                        ps = systems[i],
                        initialSpeed = systems[i].main.simulationSpeed
                    };
                }
                
                // 立即同步一次速度
                SyncSpeed();
            }
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
            try
            {
                SyncSpeed();
            }
            catch(System.Exception ex)
            {
                Debug.LogError($"VFX OnUpdate: {ex.Message}");
            }
        }

        private void SyncSpeed()
        {
            if (particleInfos == null) return;
            float globalSpeed = context.GlobalPlaySpeed;
            for (int i = 0; i < particleInfos.Length; i++)
            {
                var info = particleInfos[i];
                if (info.ps != null)
                {
                    var main = info.ps.main;
                    main.simulationSpeed = info.initialSpeed * globalSpeed;
                }
            }
        }

        public override void OnExit()
        {
            Debug.Log($"[RuntimeVFXProcess] OnExit at time: {UnityEngine.Time.time}");
            if (vfxInstance == null) return;

            if (clip.destroyOnEnd)
            {
                if (clip.stopEmissionOnEnd)
                {
                    // 软结束
                    var particles = vfxInstance.GetComponentsInChildren<ParticleSystem>();
                    float maxLifetime = 0f;
                    foreach (var ps in particles)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        if (ps.main.startLifetime.constantMax > maxLifetime)
                            maxLifetime = ps.main.startLifetime.constantMax;
                    }

                    // 尝试获取 CoroutineRunner
                    var runner = context.GetService<MonoBehaviour>("CoroutineRunner");
                    if (runner == null && Application.isPlaying)
                    {
                        // Fallback to SkillLifecycleManager if available
                        runner = SkillLifecycleManager.Instance;
                    }

                    if (runner != null && runner.isActiveAndEnabled)
                    {
                        runner.StartCoroutine(DelayReturn(vfxInstance, maxLifetime));
                    }
                    else
                    {
                        // 无 Runner，强行回收
                        VFXPoolManager.Return(vfxInstance);
                    }
                }
                else
                {
                    // 硬结束
                    VFXPoolManager.Return(vfxInstance);
                }
            }
            else
            {
                // 不销毁 (交由外部或自行销毁)，断开引用
                vfxInstance = null; 
            }
        }
        public override void OnDisable() 
        {
            
        }
        public override void Reset()
         {
            base.Reset();
            particleInfos = null;
            vfxInstance = null;
        }
            
        private IEnumerator DelayReturn(GameObject inst, float delay)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            VFXPoolManager.Return(inst);
        }
    }
}
