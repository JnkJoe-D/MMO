using System;
using System.Collections.Generic;

namespace Game.GraphTools
{
    [Serializable]
    public sealed class GraphCompileReport
    {
        private readonly List<GraphValidationMessage> messages = new List<GraphValidationMessage>();

        public IReadOnlyList<GraphValidationMessage> Messages => messages;
        public bool HasErrors => messages.Exists(static x => x.Severity == GraphMessageSeverity.Error);

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

        public void Merge(GraphValidationResult result)
        {
            if (result == null) return;
            messages.AddRange(result.Messages);
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
