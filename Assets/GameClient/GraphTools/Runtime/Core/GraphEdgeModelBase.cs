using System;
using UnityEngine;

namespace Game.GraphTools
{
    [Serializable]
    public abstract class GraphEdgeModelBase
    {
        [SerializeField]
        private string edgeId = Guid.NewGuid().ToString("N");

        public string EdgeId => edgeId;
        public string OutputNodeId = string.Empty;
        public string OutputPortId = string.Empty;
        public string InputNodeId = string.Empty;
        public string InputPortId = string.Empty;
        public int SortOrder;
        public bool IsEnabled = true;
    }
}
