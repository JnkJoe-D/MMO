using System;

namespace SkillEditor.Editor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CustomDrawerAttribute : Attribute
    {
        public Type TargetType { get; private set; }

        public CustomDrawerAttribute(Type targetType)
        {
            TargetType = targetType;
        }
    }
}
