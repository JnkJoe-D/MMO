using System;

namespace SkillEditor
{
    [Serializable]
    [TrackDefinition("伤害判定轨道",  typeof(DamageClip), "#E57F33", "Animation.EventMarker", 3)]
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
