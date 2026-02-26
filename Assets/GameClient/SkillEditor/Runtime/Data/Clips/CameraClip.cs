using System;
using UnityEngine;

namespace SkillEditor
{
    [Serializable]
    public class CameraClip : ClipBase
    {
        [Header("Camera Settings")]
        public Vector3 cameraOffset;
        public float fieldOfView = 60f;

        public CameraClip()
        {
            clipName = "Camera Clip";
            duration = 1.0f;
        }

        public override ClipBase Clone()
        {
            return new CameraClip
            {
                clipId = Guid.NewGuid().ToString(),
                clipName = this.clipName,
                startTime = this.startTime,
                duration = this.duration,
                isEnabled = this.isEnabled,
                cameraOffset = this.cameraOffset,
                fieldOfView = this.fieldOfView
            };
        }
    }
}
