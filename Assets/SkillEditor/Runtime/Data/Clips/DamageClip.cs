using System;
using UnityEngine;

namespace SkillEditor
{
    [Serializable]
    public class DamageClip : ClipBase
    {
        [Header("Damage Settings")]
        public float damage = 100f;
        public float radius = 2f;

        public DamageClip()
        {
            clipName = "Damage Clip";
            duration = 0.5f;
        }

        public override ClipBase Clone()
        {
            return new DamageClip
            {
                clipId = Guid.NewGuid().ToString(),
                clipName = this.clipName,
                startTime = this.startTime,
                duration = this.duration,
                isEnabled = this.isEnabled,
                damage = this.damage,
                radius = this.radius
            };
        }
    }
}
