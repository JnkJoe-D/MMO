using System;
using UnityEngine;

namespace Game.GraphTools
{
    [Serializable]
    public sealed class GraphGroupModel
    {
        [SerializeField]
        private string groupId = Guid.NewGuid().ToString("N");

        public string GroupId => groupId;
        public string Title = "Group";
        public Rect Position = new Rect(0f, 0f, 400f, 250f);
        public bool IsCollapsed;
    }
}
