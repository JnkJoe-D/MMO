using System;

namespace SkillEditor
{
    [Serializable]
    public class AnimationTrack : TrackBase
    {
        public AnimationTrack()
        {
            trackName = "动画轨道";
            trackType = "AnimationTrack";
        }
        
        public override bool CanOverlap => true;

        public override TrackBase Clone()
        {
            AnimationTrack clone = new AnimationTrack();
            CloneBaseProperties(clone);
            return clone;
        }
    }
}
