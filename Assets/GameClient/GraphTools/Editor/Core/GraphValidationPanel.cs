using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.GraphTools.Editor
{
    public class GraphValidationPanel : VisualElement
    {
        private readonly VisualElement container;

        public GraphValidationPanel()
        {
            style.flexGrow = 1f;
            style.paddingLeft = 6f;
            style.paddingRight = 6f;

            Label headerLabel = new Label("Validation");
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(headerLabel);

            container = new VisualElement();
            container.style.flexGrow = 1f;
            Add(container);
        }

        public void ClearMessages()
        {
            container.Clear();
            container.Add(new Label("当前没有校验结果。"));
        }

        public void BindMessages(IEnumerable<GraphValidationMessage> messages)
        {
            container.Clear();
            bool hasAny = false;
            if (messages != null)
            {
                foreach (GraphValidationMessage message in messages)
                {
                    hasAny = true;
                    Label label = new Label($"[{message.Severity}] {message.Message}");
                    label.style.whiteSpace = WhiteSpace.Normal;
                    container.Add(label);
                }
            }

            if (!hasAny)
            {
                container.Add(new Label("当前没有校验结果。"));
            }
        }
    }
}
