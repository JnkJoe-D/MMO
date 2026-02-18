using UnityEngine;
using System.Collections;

namespace SkillEditor
{
    [ProcessBinding(typeof(VFXClip), PlayMode.Runtime)]
    public class RuntimeVFXProcess : ProcessBase<VFXClip>
    {
        private GameObject vfxInstance;
        private Coroutine returnCoroutine;

        public override void OnEnable()
        {
            base.OnEnable();

        }
        public override void OnEnter()
        {
            if (clip.effectPrefab == null) return;

            // 1. 获取挂点
            Transform targetTransform = null;
            var actor = context.SkillActor;
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
            }
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
        }

        public override void OnExit()
        {
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
                    var runner = context.CoroutineRunner;
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

        }
        private IEnumerator DelayReturn(GameObject inst, float delay)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            VFXPoolManager.Return(inst);
        }
    }
}
