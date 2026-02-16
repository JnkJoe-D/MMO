using System;

namespace SkillEditor
{
    [Serializable]
    public class VFXTrack : TrackBase
    {
        public VFXTrack()
        {
            trackName = "特效轨道";
            trackType = "VFXTrack";
        }

        public override TrackBase Clone()
        {
            VFXTrack clone = new VFXTrack();
            CloneBaseProperties(clone);
            return clone;
        }
    }
}
