using System;
using UnityEngine;

namespace Game.GraphTools
{
    [Serializable]
    public abstract class GraphNodeModelBase
    {
        [SerializeField]
        private string nodeId = Guid.NewGuid().ToString("N");

        public string NodeId => nodeId;
        public string Title = "Node";
        public Vector2 Position;
        public bool IsEnabled = true;
        public bool IsCollapsed;
        public string Note = string.Empty;
    }
}
