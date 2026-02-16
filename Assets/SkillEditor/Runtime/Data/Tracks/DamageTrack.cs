using System;

namespace SkillEditor
{
    [Serializable]
    public class DamageTrack : TrackBase
    {
        public DamageTrack()
        {
            trackName = "伤害判定轨道";
            trackType = "DamageTrack";
        }

        public override TrackBase Clone()
        {
            DamageTrack clone = new DamageTrack();
            CloneBaseProperties(clone);
            return clone;
        }
    }
}
