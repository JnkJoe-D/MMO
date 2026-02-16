using System;

namespace SkillEditor
{
    [Serializable]
    public class CameraTrack : TrackBase
    {
        public CameraTrack()
        {
            trackName = "摄像机轨道";
            trackType = "CameraTrack";
        }

        public override TrackBase Clone()
        {
            CameraTrack clone = new CameraTrack();
            CloneBaseProperties(clone);
            return clone;
        }
    }
}
