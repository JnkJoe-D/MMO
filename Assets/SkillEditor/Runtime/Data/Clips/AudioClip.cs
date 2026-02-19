using System;
using UnityEngine;

namespace SkillEditor
{
    [Serializable]
    public class AudioClip : ClipBase
    {
        [Header("Audio Settings")]
        [SkillProperty("音频资源")]
        public UnityEngine.AudioClip audioClip;
        
        [SkillProperty("音量")]
        [Range(0f, 1f)]
        public float volume = 1.0f;

        [SkillProperty("音调")]
        [Range(0.1f, 3f)]
        public float pitch = 1.0f;

        [SkillProperty("循环播放")]
        public bool loop = false;

        [SkillProperty("空间混合 (0=2D, 1=3D)")]
        [Range(0f, 1f)]
        public float spatialBlend = 0.0f;

        [HideInInspector]
        public string clipGuid;
        
        public override bool SupportsBlending => true;

        public AudioClip()
        {
            clipName = "Audio Clip";
            duration = 1.0f;
            volume = 1.0f;
            pitch = 1.0f;
            spatialBlend = 0.0f;
        }

        public override ClipBase Clone()
        {
            return new AudioClip
            {
                clipId = Guid.NewGuid().ToString(),
                clipName = this.clipName,
                startTime = this.startTime,
                duration = this.duration,
                isEnabled = this.isEnabled,
                audioClip = this.audioClip,
                volume = this.volume,
                pitch = this.pitch,
                loop = this.loop,
                spatialBlend = this.spatialBlend,
                clipGuid = this.clipGuid,
                blendInDuration = this.blendInDuration,
                blendOutDuration = this.blendOutDuration
            };
        }
    }
}
