using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Game.GraphTools.Editor
{
    public class BaseGraphView : GraphView
    {
        private bool suppressGraphChanges;

        protected GraphAssetBase graphAsset;

        public GraphAssetBase GraphAsset => graphAsset;
        public event Action<object> SelectionModelChanged;
        public event Action GraphModelChanged;

        public BaseGraphView()
        {
            style.flexGrow = 1f;

            GridBackground background = new GridBackground();
            Insert(0, background);
            background.style.position = Position.Absolute;
            background.style.left = 0f;
            background.style.top = 0f;
            background.style.right = 0f;
            background.style.bottom = 0f;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            graphViewChanged = HandleGraphViewChanged;

            RegisterCallback<MouseUpEvent>(_ => NotifySelectionChanged());
            RegisterCallback<KeyUpEvent>(_ => NotifySelectionChanged());
        }

        public virtual void BindGraph(GraphAssetBase asset)
        {
            suppressGraphChanges = true;
            try
            {
                DeleteElements(graphElements.ToList());
                ClearSelection();
                graphAsset = asset;
            }
            finally
            {
                suppressGraphChanges = false;
            }

            NotifySelectionChanged();
        }

        public virtual void RefreshPresentation()
        {
        }

        public override System.Collections.Generic.List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList()
                .Where(port => port != startPort && port.node != startPort.node && port.direction != startPort.direction)
                .ToList();
        }

        protected virtual bool TryCreateEdgeModel(Edge edgeView, out GraphEdgeModelBase edgeModel)
        {
            edgeModel = null;
            return false;
        }

        protected void MarkAssetDirty(string actionName)
        {
            if (graphAsset == null)
            {
                return;
            }

            Undo.RecordObject(graphAsset, actionName);
            EditorUtility.SetDirty(graphAsset);
        }

        protected void ApplyGraphChange(string actionName, Action applyAction)
        {
            if (graphAsset == null || applyAction == null)
            {
                return;
            }

            MarkAssetDirty(actionName);
            applyAction();
            NotifyGraphChanged();
            NotifySelectionChanged();
        }

        protected void NotifyGraphChanged()
        {
            GraphModelChanged?.Invoke();
        }

        protected void NotifySelectionChanged()
        {
            object selectionModel = graphAsset;
            if (selection != null)
            {
                ISelectable selectedItem = selection.FirstOrDefault();
                if (selectedItem is BaseNodeView nodeView)
                {
                    selectionModel = nodeView.Model != null ? (object)nodeView.Model : graphAsset;
                }
                else if (selectedItem is Edge edgeView)
                {
                    selectionModel = edgeView.userData ?? graphAsset;
                }
            }

            SelectionModelChanged?.Invoke(selectionModel);
        }

        private GraphViewChange HandleGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (suppressGraphChanges || graphAsset == null)
            {
                return graphViewChange;
            }

            bool changed = false;

            if (graphViewChange.movedElements != null)
            {
                bool recorded = false;
                foreach (GraphElement element in graphViewChange.movedElements)
                {
                    if (element is not BaseNodeView nodeView || nodeView.Model == null)
                    {
                        continue;
                    }

                    if (!recorded)
                    {
                        MarkAssetDirty("Move Graph Node");
                        recorded = true;
                    }

                    nodeView.Model.Position = nodeView.GetPosition().position;
                    changed = true;
                }
            }

            if (graphViewChange.elementsToRemove != null)
            {
                // Pre-process: If a node is being removed, also remove its connected edges visually
                List<GraphElement> extraRemovals = new List<GraphElement>();
                foreach (GraphElement element in graphViewChange.elementsToRemove)
                {
                    if (element is Node node)
                    {
                        foreach (Edge edge in edges.ToList())
                        {
                            if (edge.input?.node == node || edge.output?.node == node)
                            {
                                if (!graphViewChange.elementsToRemove.Contains(edge) && !extraRemovals.Contains(edge))
                                {
                                    extraRemovals.Add(edge);
                                }
                            }
                        }
                    }
                }
                
                if (extraRemovals.Count > 0)
                {
                    graphViewChange.elementsToRemove.AddRange(extraRemovals);
                }

                bool recorded = false;
                foreach (GraphElement element in graphViewChange.elementsToRemove)
                {
                    if (element is BaseNodeView nodeView && nodeView.Model != null)
                    {
                        if (!recorded)
                        {
                            MarkAssetDirty("Remove Graph Element");
                            recorded = true;
                        }

                        graphAsset.Nodes.Remove(nodeView.Model);
                        graphAsset.Edges.RemoveAll(edge =>
                            edge != null &&
                            (edge.InputNodeId == nodeView.Model.NodeId || edge.OutputNodeId == nodeView.Model.NodeId));
                        changed = true;
                        continue;
                    }

                    if (element is Edge edgeView)
                    {
                        // Visually disconnect ports to ensure UI refresh
                        edgeView.input?.Disconnect(edgeView);
                        edgeView.output?.Disconnect(edgeView);

                        if (edgeView.userData is GraphEdgeModelBase edgeModel)
                        {
                            if (!recorded)
                            {
                                MarkAssetDirty("Remove Graph Element");
                                recorded = true;
                            }

                            graphAsset.Edges.Remove(edgeModel);
                            changed = true;
                        }
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                bool recorded = false;
                // Use a removal list to safely modify the collection while iterating
                List<Edge> edgesToRemoveFromList = new List<Edge>();

                foreach (Edge edgeView in graphViewChange.edgesToCreate)
                {
                    if (edgeView == null || edgeView.userData is GraphEdgeModelBase)
                    {
                        continue;
                    }

                    if (!TryCreateEdgeModel(edgeView, out GraphEdgeModelBase edgeModel) || edgeModel == null)
                    {
                        // Filter out edges that failed model creation (e.g., duplicates or invalid)
                        // This prevents redundant visual edges from being added to the GraphView.
                        edgesToRemoveFromList.Add(edgeView);
                        continue;
                    }

                    if (!recorded)
                    {
                        MarkAssetDirty("Create Graph Edge");
                        recorded = true;
                    }

                    graphAsset.Edges.Add(edgeModel);
                    edgeView.userData = edgeModel;
                    changed = true;
                }

                if (edgesToRemoveFromList.Count > 0)
                {
                    foreach (Edge edge in edgesToRemoveFromList)
                    {
                        graphViewChange.edgesToCreate.Remove(edge);
                    }
                }
            }

            if (changed)
            {
                NotifyGraphChanged();
                NotifySelectionChanged();
            }

            return graphViewChange;
        }
    }
}
