using System;
using Game.MAnimSystem;
using UnityEngine;

namespace SkillEditor
{
    // 注意：Unity 也有 UnityEngine.AnimationClip，这里需要避免命名冲突
    // 但根据原文件，类名确实是 AnimationClip。建议重命名为 SkillAnimationClip 以避免混淆。
    // 不过为了保持兼容性，先保留原名，但在使用时需全名引用 UnityEngine.AnimationClip
    
    [Serializable]
    public class SkillAnimationClip : ClipBase
    {
        [SkillProperty("动画资源")]
        public AnimationClip animationClip;
        
        [SkillProperty("播放速度")]
        public float playbackSpeed = 1.0f;
        [SkillProperty("目标动画层")]
        public EAnimLayer layer = EAnimLayer.Locomotion;
        [SkillProperty("目标动画遮罩")]
        public AvatarMask avatarMask;
        [HideInInspector]
        public string clipGuid;
        [HideInInspector]
        public string maskGuid;
        public override bool SupportsBlending => true;

        public SkillAnimationClip()
        {
            clipName = "动画片段";
            duration = 1.0f;
        }

        public override ClipBase Clone()
        {
            return new SkillAnimationClip
            {
                clipId = Guid.NewGuid().ToString(),
                clipName = this.clipName,
                startTime = this.startTime,
                duration = this.duration,
                isEnabled = this.isEnabled,
                animationClip = this.animationClip,
                playbackSpeed = this.playbackSpeed,
                clipGuid = this.clipGuid,
                maskGuid = this.maskGuid,
                layer = this.layer,
                blendInDuration = this.blendInDuration,
                blendOutDuration = this.blendOutDuration
            };
        }
    }
}
