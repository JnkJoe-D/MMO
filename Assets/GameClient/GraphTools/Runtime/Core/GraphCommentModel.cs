using System;
using UnityEngine;

namespace Game.GraphTools
{
    [Serializable]
    public sealed class GraphCommentModel
    {
        [SerializeField]
        private string commentId = Guid.NewGuid().ToString("N");

        public string CommentId => commentId;
        public string Title = "Comment";
        [TextArea(3, 8)]
        public string Content = string.Empty;
        public Rect Position = new Rect(0f, 0f, 260f, 120f);
    }
}
