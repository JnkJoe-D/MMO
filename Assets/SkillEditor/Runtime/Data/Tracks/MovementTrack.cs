using System;

namespace SkillEditor
{
    [Serializable]
    public class MovementTrack : TrackBase
    {
        public MovementTrack()
        {
            trackName = "移动轨道";
            trackType = "MovementTrack";
        }

        public override TrackBase Clone()
        {
            MovementTrack clone = new MovementTrack();
            CloneBaseProperties(clone);
            return clone;
        }
    }
}
