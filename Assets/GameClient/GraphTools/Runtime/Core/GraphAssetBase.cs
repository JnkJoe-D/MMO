using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.GraphTools
{
    public abstract class GraphAssetBase : ScriptableObject
    {
        public string GraphId = System.Guid.NewGuid().ToString("N");
        public int Version = 1;
        public string DisplayName = "New Graph";
        public GraphMetadata Metadata = new GraphMetadata();

        [SerializeReference]
        public List<GraphNodeModelBase> Nodes = new List<GraphNodeModelBase>();

        [SerializeReference]
        public List<GraphEdgeModelBase> Edges = new List<GraphEdgeModelBase>();

        [SerializeReference]
        public List<BlackboardEntryBase> Blackboard = new List<BlackboardEntryBase>();

        public List<GraphGroupModel> Groups = new List<GraphGroupModel>();
        public List<GraphCommentModel> Comments = new List<GraphCommentModel>();

        public IEnumerable<T> GetNodes<T>() where T : GraphNodeModelBase
        {
            return Nodes.OfType<T>();
        }

        public IEnumerable<T> GetEdges<T>() where T : GraphEdgeModelBase
        {
            return Edges.OfType<T>();
        }

        public GraphNodeModelBase FindNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return null;
            return Nodes.FirstOrDefault(x => x != null && x.NodeId == nodeId);
        }

        public T FindNode<T>(string nodeId) where T : GraphNodeModelBase
        {
            return FindNode(nodeId) as T;
        }

        public void EnsureMetadata()
        {
            if (Metadata == null)
            {
                Metadata = new GraphMetadata();
            }
        }
    }
}
