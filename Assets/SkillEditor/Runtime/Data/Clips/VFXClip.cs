using System;
using UnityEngine;

namespace SkillEditor
{
    [Serializable]
    public class VFXClip : ClipBase
    {
        [Header("VFX Settings")]
        [SkillProperty("特效预制体")]
        public GameObject effectPrefab;
        
        [SkillProperty("偏移坐标")]
        public Vector3 offset;
        
        [HideInInspector]
        public string clipGuid;

        public VFXClip()
        {
            clipName = "VFX Clip";
            duration = 1.0f;
        }

        public override ClipBase Clone()
        {
            return new VFXClip
            {
                clipId = Guid.NewGuid().ToString(),
                clipName = this.clipName,
                startTime = this.startTime,
                duration = this.duration,
                isEnabled = this.isEnabled,
                effectPrefab = this.effectPrefab,
                offset = this.offset,
                clipGuid = this.clipGuid
            };
        }
    }
}
