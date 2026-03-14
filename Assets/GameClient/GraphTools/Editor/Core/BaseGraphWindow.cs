using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.GraphTools.Editor
{
    public abstract class BaseGraphWindow : EditorWindow
    {
        protected GraphAssetBase currentGraph;
        protected BaseGraphView graphView;
        protected GraphInspectorPanel inspectorPanel;
        protected GraphBlackboardPanel blackboardPanel;
        protected GraphValidationPanel validationPanel;
        protected object currentSelectionModel;
        private string trackedGraphState = string.Empty;
        private double nextGraphStateCheckAt;

        protected abstract string WindowTitle { get; }
        protected abstract BaseGraphView CreateGraphView();

        protected virtual void BuildCustomToolbar(Toolbar toolbar)
        {
        }

        protected virtual IEnumerable<GraphValidationMessage> GetValidationMessages(GraphAssetBase graphAsset)
        {
            return null;
        }

        protected virtual string BuildGraphStateToken(GraphAssetBase graphAsset)
        {
            return graphAsset == null ? string.Empty : EditorJsonUtility.ToJson(graphAsset);
        }

        protected virtual void BeforeGraphRefresh(GraphAssetBase graphAsset)
        {
        }

        protected virtual void OnGraphBound(GraphAssetBase graphAsset)
        {
        }

        protected virtual void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
            BuildLayout();
        }

        protected virtual void OnDisable()
        {
            GraphViewStateSerializer.Capture(currentGraph, graphView);
            if (graphView != null)
            {
                graphView.SelectionModelChanged -= HandleSelectionModelChanged;
                graphView.GraphModelChanged -= HandleGraphModelChanged;
            }

            if (blackboardPanel != null)
            {
                blackboardPanel.BlackboardEntrySelected -= HandleBlackboardEntrySelected;
            }
        }

        protected virtual void Update()
        {
            if (currentGraph == null || graphView == null)
            {
                return;
            }

            if (EditorApplication.timeSinceStartup < nextGraphStateCheckAt)
            {
                return;
            }

            nextGraphStateCheckAt = EditorApplication.timeSinceStartup + 0.25d;

            string currentState = BuildGraphStateToken(currentGraph);
            if (currentState == trackedGraphState)
            {
                return;
            }

            BeforeGraphRefresh(currentGraph);
            GraphViewStateSerializer.Capture(currentGraph, graphView);
            currentSelectionModel = currentGraph;
            graphView.BindGraph(currentGraph);
            GraphViewStateSerializer.Apply(currentGraph, graphView);
            RefreshPanels();
            OnGraphBound(currentGraph);
            TrackGraphState();
            Repaint();
        }

        protected void BindGraph(GraphAssetBase graphAsset)
        {
            GraphViewStateSerializer.Capture(currentGraph, graphView);
            currentGraph = graphAsset;
            currentSelectionModel = graphAsset;
            graphView.BindGraph(graphAsset);
            GraphViewStateSerializer.Apply(graphAsset, graphView);
            RefreshPanels();
            OnGraphBound(graphAsset);
            TrackGraphState();
        }

        protected void ShowMessages(IEnumerable<GraphValidationMessage> messages)
        {
            validationPanel.BindMessages(messages);
        }

        protected virtual void RefreshPanels()
        {
            inspectorPanel.BindSelection(currentGraph, currentSelectionModel, ApplyInspectorChange, HandleInspectorSelectionRequested);
            blackboardPanel.BindGraph(currentGraph, currentSelectionModel as BlackboardEntryBase, ApplyInspectorChange);

            IEnumerable<GraphValidationMessage> messages = GetValidationMessages(currentGraph);
            if (messages == null)
            {
                validationPanel.ClearMessages();
            }
            else
            {
                validationPanel.BindMessages(messages);
            }
        }

        private void BuildLayout()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            Toolbar toolbar = new Toolbar();

            ToolbarButton saveButton = new ToolbarButton(SaveGraph) { text = "Save" };
            toolbar.Add(saveButton);

            ToolbarButton frameButton = new ToolbarButton(() => graphView?.FrameAll()) { text = "Frame" };
            toolbar.Add(frameButton);

            BuildCustomToolbar(toolbar);
            rootVisualElement.Add(toolbar);

            VisualElement content = new VisualElement();
            content.style.flexGrow = 1f;
            content.style.flexDirection = FlexDirection.Row;
            rootVisualElement.Add(content);

            graphView = CreateGraphView();
            graphView.SelectionModelChanged += HandleSelectionModelChanged;
            graphView.GraphModelChanged += HandleGraphModelChanged;
            content.Add(graphView);

            ScrollView sidePanel = new ScrollView();
            sidePanel.style.width = 300f;
            sidePanel.style.flexShrink = 0f;
            sidePanel.style.borderLeftWidth = 1f;
            sidePanel.style.borderLeftColor = new Color(0.18f, 0.18f, 0.18f);
            content.Add(sidePanel);

            inspectorPanel = new GraphInspectorPanel();
            blackboardPanel = new GraphBlackboardPanel();
            validationPanel = new GraphValidationPanel();
            blackboardPanel.BlackboardEntrySelected += HandleBlackboardEntrySelected;

            sidePanel.Add(inspectorPanel);
            sidePanel.Add(blackboardPanel);
            sidePanel.Add(validationPanel);

            RefreshPanels();
        }

        private void SaveGraph()
        {
            if (currentGraph == null)
            {
                return;
            }

            GraphViewStateSerializer.Capture(currentGraph, graphView);
            EditorUtility.SetDirty(currentGraph);
            AssetDatabase.SaveAssets();
            TrackGraphState();
        }

        private void HandleSelectionModelChanged(object selectionModel)
        {
            currentSelectionModel = selectionModel ?? currentGraph;
            inspectorPanel.BindSelection(currentGraph, currentSelectionModel, ApplyInspectorChange, HandleInspectorSelectionRequested);
            blackboardPanel.BindGraph(currentGraph, currentSelectionModel as BlackboardEntryBase, ApplyInspectorChange);
            TrackGraphState();
        }

        private void HandleGraphModelChanged()
        {
            if (currentGraph == null)
            {
                return;
            }

            EditorUtility.SetDirty(currentGraph);
            graphView.RefreshPresentation();
            RefreshPanels();
            TrackGraphState();
        }

        private void HandleBlackboardEntrySelected(BlackboardEntryBase entry)
        {
            currentSelectionModel = entry != null ? (object)entry : currentGraph;
            inspectorPanel.BindSelection(currentGraph, currentSelectionModel, ApplyInspectorChange, HandleInspectorSelectionRequested);
            blackboardPanel.BindGraph(currentGraph, entry, ApplyInspectorChange);
            TrackGraphState();
        }

        private void HandleInspectorSelectionRequested(object selectionModel)
        {
            currentSelectionModel = selectionModel ?? currentGraph;
            inspectorPanel.BindSelection(currentGraph, currentSelectionModel, ApplyInspectorChange, HandleInspectorSelectionRequested);
            blackboardPanel.BindGraph(currentGraph, currentSelectionModel as BlackboardEntryBase, ApplyInspectorChange);
            TrackGraphState();
        }

        private void ApplyInspectorChange(string actionName, Action applyAction)
        {
            if (currentGraph == null || applyAction == null)
            {
                return;
            }

            Undo.RecordObject(currentGraph, actionName);
            applyAction();
            EditorUtility.SetDirty(currentGraph);
            SanitizeSelectionAfterInspectorChange();
            graphView.RefreshPresentation();
            RefreshPanels();
            TrackGraphState();
        }

        private void SanitizeSelectionAfterInspectorChange()
        {
            if (currentSelectionModel is GraphNodeModelBase nodeModel && !currentGraph.Nodes.Contains(nodeModel))
            {
                currentSelectionModel = currentGraph;
                return;
            }

            if (currentSelectionModel is GraphEdgeModelBase edgeModel && !currentGraph.Edges.Contains(edgeModel))
            {
                currentSelectionModel = currentGraph;
                return;
            }

            if (currentSelectionModel is BlackboardEntryBase blackboardEntry && !currentGraph.Blackboard.Contains(blackboardEntry))
            {
                currentSelectionModel = currentGraph;
            }
        }

        private void TrackGraphState()
        {
            trackedGraphState = BuildGraphStateToken(currentGraph);
            nextGraphStateCheckAt = EditorApplication.timeSinceStartup + 0.25d;
        }
    }
}
