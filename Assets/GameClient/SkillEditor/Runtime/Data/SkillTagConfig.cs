using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor
{
    [CreateAssetMenu(fileName = "SkillTagConfig", menuName = "SkillEditor/TagConfig", order = 200)]
    public class SkillTagConfig : ScriptableObject
    {
        [Tooltip("配置技能系统中所有可用的标签")]
        public List<string> availableTags = new List<string>()
        {
            "Enemy",
            "Ally",
            "Self",
            "Friendly",
            "NPC"
        };
    }
}
