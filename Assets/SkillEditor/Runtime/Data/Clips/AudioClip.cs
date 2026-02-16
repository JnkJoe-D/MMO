using System;
using UnityEngine;

namespace SkillEditor
{
    [Serializable]
    public class AudioClip : ClipBase
    {
        [Header("Audio Settings")]
        public UnityEngine.AudioClip audioClip;
        public float volume = 1.0f;
        public string clipGuid;
        
        public override bool SupportsBlending => true;

        public AudioClip()
        {
            clipName = "Audio Clip";
            duration = 1.0f;
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
                clipGuid = this.clipGuid
            };
        }
    }
}
