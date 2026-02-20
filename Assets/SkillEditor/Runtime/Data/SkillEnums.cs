using System;

namespace SkillEditor
{
    public enum HitBoxType 
    { 
        Sphere, 
        Box, 
        Capsule, 
        Sector, 
        Ring 
    }

    public enum TargetType 
    { 
        Enemy, 
        Ally, 
        All 
    }

    public enum HitFrequency 
    { 
        Once, 
        Always, 
        Interval 
    }

    public enum TargetSortMode 
    { 
        None, 
        Closest, 
        Random 
    }
}
