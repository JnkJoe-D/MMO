using System;

namespace SkillEditor
{
    /// <summary>
    /// 用于定义在 SkillEditor Inspector 中显示的名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SkillPropertyAttribute : Attribute
    {
        public string Name { get; private set; }

        public SkillPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}
