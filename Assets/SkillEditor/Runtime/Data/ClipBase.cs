using System;
using UnityEngine;

namespace SkillEditor
{
    /// <summary>
    /// 片段基类 (Non-Generic Wrapper for serialization)
    /// </summary>
    [Serializable]
    public abstract class ClipBase : ISkillClipData
    {
        [HideInInspector]
        public string clipId = Guid.NewGuid().ToString();
        [SkillProperty("片段名称")]
        public string clipName = "Clip";
        [SkillProperty("启用")]
        public bool isEnabled = true;

        [SkillProperty("开始时间")]
        public float startTime;
        
        [SkillProperty("持续时间")]
        public float duration = 1.0f;

        public float StartTime => startTime;
        public float Duration => duration;
        public float EndTime => startTime + duration;

        // Legacy / Blending support
        public virtual bool SupportsBlending => false;
        
        [SkillProperty("渐入时长")]
        public float blendInDuration;
        
        [SkillProperty("渐出时长")]
        public float blendOutDuration;

        public abstract ClipBase Clone();
    }
}
