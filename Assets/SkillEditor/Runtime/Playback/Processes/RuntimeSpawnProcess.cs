using UnityEngine;

namespace SkillEditor
{
    [ProcessBinding(typeof(SpawnClip), PlayMode.Runtime)]
    public class RuntimeSpawnProcess : ProcessBase<SpawnClip>
    {
        private ISkillSpawnHandler spawnHandler;
        private GameObject spawnedInstance;

        public override void OnEnable()
        {
            spawnHandler = context.GetService<ISkillSpawnHandler>();
        }

        public override void OnEnter()
        {
            if (spawnHandler == null || clip.prefab == null) return;

            GetMatrix(out Vector3 pos, out Quaternion rot, out Transform parent);

            spawnedInstance = spawnHandler.SpawnObject(
                clip.prefab, 
                pos, 
                rot, 
                clip.eventTag, 
                clip.initialVelocity, 
                clip.detach,
                clip.detach ? null : parent
            );
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
            // Do nothing on update for spawn
        }

        public override void OnExit()
        {
            // 如果技能被打断，且要求打断时销毁
            // 简单判定打断的方法：判断父物体是否已失活或主动抛出打断事件（根据业务补充）
            if (clip.destroyOnInterrupt && spawnedInstance != null)
            {
                // 判断是否是正常结束
                // Todo: Check if naturally finished. This needs proper context.IsInterrupted info.
                // 暂时注释，待外部注入断言。
                // spawnHandler?.DestroySpawnedObject(spawnedInstance);
            }
            spawnedInstance = null;
        }

        private void GetMatrix(out Vector3 pos, out Quaternion rot, out Transform parent)
        {
            parent = null;
            if (context != null)
            {
                var actor = context.GetService<ISkillActor>();
                parent = actor.GetBone(clip.bindPoint);
            }

            if (parent != null)
            {
                pos = parent.position + parent.rotation * clip.positionOffset;
                rot = parent.rotation * Quaternion.Euler(clip.rotationOffset);
            }
            else
            {
                pos = clip.positionOffset;
                rot = Quaternion.Euler(clip.rotationOffset);
            }
        }

        public override void Reset()
        {
            base.Reset();
            spawnHandler = null;
            spawnedInstance = null;
        }
    }
}
