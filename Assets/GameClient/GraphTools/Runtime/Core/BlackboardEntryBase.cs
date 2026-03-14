using System;

namespace Game.GraphTools
{
    [Serializable]
    public abstract class BlackboardEntryBase
    {
        public string Key = string.Empty;
        public string DisplayName = string.Empty;
        public string SerializedTypeName = string.Empty;
    }
}
