using System;
using System.Collections.Generic;

namespace Game.GraphTools
{
    public enum GraphMessageSeverity
    {
        Info,
        Warning,
        Error
    }

    [Serializable]
    public sealed class GraphValidationMessage
    {
        public GraphMessageSeverity Severity;
        public string Code = string.Empty;
        public string Message = string.Empty;
        public string NodeId = string.Empty;
        public string EdgeId = string.Empty;
    }

    [Serializable]
    public sealed class GraphValidationResult
    {
        private readonly List<GraphValidationMessage> messages = new List<GraphValidationMessage>();

        public IReadOnlyList<GraphValidationMessage> Messages => messages;
        public bool HasErrors => messages.Exists(static x => x.Severity == GraphMessageSeverity.Error);
        public bool HasWarnings => messages.Exists(static x => x.Severity == GraphMessageSeverity.Warning);

        public void AddInfo(string code, string message, string nodeId = "", string edgeId = "")
        {
            AddMessage(GraphMessageSeverity.Info, code, message, nodeId, edgeId);
        }

        public void AddWarning(string code, string message, string nodeId = "", string edgeId = "")
        {
            AddMessage(GraphMessageSeverity.Warning, code, message, nodeId, edgeId);
        }

        public void AddError(string code, string message, string nodeId = "", string edgeId = "")
        {
            AddMessage(GraphMessageSeverity.Error, code, message, nodeId, edgeId);
        }

        public void Merge(GraphValidationResult other)
        {
            if (other == null) return;
            messages.AddRange(other.messages);
        }

        private void AddMessage(GraphMessageSeverity severity, string code, string message, string nodeId, string edgeId)
        {
            messages.Add(new GraphValidationMessage
            {
                Severity = severity,
                Code = code ?? string.Empty,
                Message = message ?? string.Empty,
                NodeId = nodeId ?? string.Empty,
                EdgeId = edgeId ?? string.Empty
            });
        }
    }
}
