using UnityEngine;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 编辑器预览：VFX 片段 Process
    /// 通过 EditorVFXManager 管理实例的生命周期和采样
    /// </summary>
    [ProcessBinding(typeof(VFXClip), PlayMode.EditorPreview)]
    public class EditorVFXProcess : ProcessBase<VFXClip>
    {
        private GameObject instance;
        private EditorVFXManager vfxMgr;

        public override void OnEnable()
        {
            vfxMgr = EditorVFXManager.Instance;
        }

        public override void OnEnter()
        {
            if (clip.effectPrefab == null) return;

            Vector3 pos = context.OwnerTransform != null
                ? context.OwnerTransform.position + clip.offset
                : clip.offset;

            instance = vfxMgr.Spawn(clip.effectPrefab, pos, Quaternion.identity);
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
            // 正常播放时 ParticleSystem 自动更新
            // 如需位置跟随角色，可在这里更新 instance.transform.position
            if (instance != null && context.OwnerTransform != null)
            {
                instance.transform.position = context.OwnerTransform.position + clip.offset;
            }
        }

        public override void OnExit()
        {
            // 级别 1：回收 VFX 实例到池
            if (instance != null)
            {
                vfxMgr.Return(instance);
                instance = null;
            }
        }

        public override void Reset()
        {
            base.Reset();
            instance = null;
            vfxMgr = null;
        }
    }
}
