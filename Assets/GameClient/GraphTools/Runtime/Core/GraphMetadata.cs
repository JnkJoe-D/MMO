using System;
using UnityEngine;

namespace Game.GraphTools
{
    [Serializable]
    public sealed class GraphMetadata
    {
        public Vector3 ViewPosition = Vector3.zero;
        public Vector3 ViewScale = Vector3.one;
        public string LastOpenedAtUtc = string.Empty;
        public string LastCompiledAtUtc = string.Empty;
    }
}
