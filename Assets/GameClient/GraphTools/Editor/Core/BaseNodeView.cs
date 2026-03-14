using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Game.GraphTools.Editor
{
    public class BaseNodeView : Node
    {
        public const string DefaultInputPortId = "input";
        public const string DefaultOutputPortId = "output";

        public GraphNodeModelBase Model { get; private set; }
        public Port InputPort { get; protected set; }
        public Port OutputPort { get; protected set; }

        public virtual void Bind(GraphNodeModelBase model)
        {
            Model = model;
            title = model != null ? model.Title : "Node";
            if (model != null)
            {
                SetPosition(new Rect(model.Position, new Vector2(240f, 120f)));
            }

            capabilities &= ~Capabilities.Deletable;
        }

        protected Port CreateFlowInput(
            string portName = "",
            Port.Capacity capacity = Port.Capacity.Multi,
            Orientation orientation = Orientation.Horizontal)
        {
            InputPort = CreatePort(Direction.Input, portName, DefaultInputPortId, capacity, orientation);
            inputContainer.Add(InputPort);
            return InputPort;
        }

        protected Port CreateFlowOutput(
            string portName = "",
            Port.Capacity capacity = Port.Capacity.Multi,
            Orientation orientation = Orientation.Horizontal)
        {
            OutputPort = CreatePort(Direction.Output, portName, DefaultOutputPortId, capacity, orientation);
            outputContainer.Add(OutputPort);
            return OutputPort;
        }

        private Port CreatePort(
            Direction direction,
            string portName,
            string portId,
            Port.Capacity capacity,
            Orientation orientation)
        {
            Port port = InstantiatePort(orientation, direction, capacity, typeof(bool));
            port.portName = portName;
            port.userData = portId;
            return port;
        }
    }
}
