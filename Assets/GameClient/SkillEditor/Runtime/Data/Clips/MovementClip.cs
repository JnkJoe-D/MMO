using System;
using UnityEngine;

namespace SkillEditor
{
    [Serializable]
    public class MovementClip : ClipBase
    {
        [Header("Movement Settings")]
        public Vector3 targetPosition;
        public float speed = 5f;

        public MovementClip()
        {
            clipName = "Movement Clip";
            duration = 1.0f;
        }

        public override ClipBase Clone()
        {
            return new MovementClip
            {
                clipId = Guid.NewGuid().ToString(),
                clipName = this.clipName,
                startTime = this.startTime,
                duration = this.duration,
                isEnabled = this.isEnabled,
                targetPosition = this.targetPosition,
                speed = this.speed
            };
        }
    }
}
