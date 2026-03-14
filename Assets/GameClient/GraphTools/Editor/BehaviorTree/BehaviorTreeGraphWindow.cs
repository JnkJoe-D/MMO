using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using Game.AI;

namespace Game.GraphTools.Editor
{
    public sealed class BehaviorTreeGraphWindow : BaseGraphWindow
    {
        protected override string WindowTitle => "Behavior Tree Graph";

        [MenuItem("GraphTools/AI/Behavior Tree")]
        public static void OpenWindow()
        {
            BehaviorTreeGraphWindow window = GetWindow<BehaviorTreeGraphWindow>();
            window.minSize = new Vector2(1000f, 600f);
            window.Show();
        }

        [OnOpenAsset]
        public static bool OpenAsset(int instanceId, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceId) is not BehaviorTreeGraphAsset behaviorTreeGraph)
            {
                return false;
            }

            Selection.activeObject = behaviorTreeGraph;
            BehaviorTreeGraphWindow window = GetWindow<BehaviorTreeGraphWindow>();
            window.minSize = new Vector2(1000f, 600f);
            window.Show();
            window.Focus();
            window.TryBindFromSelection();
            return true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            TryBindFromSelection();
        }

        protected override BaseGraphView CreateGraphView()
        {
            BehaviorTreeGraphView view = new BehaviorTreeGraphView
            {
                BlackboardEntryCreateRequested = AddBlackboardEntry
            };
            view.Initialize(this);
            return view;
        }

        protected override void BuildCustomToolbar(Toolbar toolbar)
        {
            ToolbarButton useSelectionButton = new ToolbarButton(TryBindFromSelection) { text = "Use Selection" };
            toolbar.Add(useSelectionButton);

            ToolbarButton compileButton = new ToolbarButton(CompileCurrentGraph) { text = "Compile" };
            toolbar.Add(compileButton);
        }

        protected override IEnumerable<GraphValidationMessage> GetValidationMessages(GraphAssetBase graphAsset)
        {
            return graphAsset is BehaviorTreeGraphAsset behaviorTreeGraph
                ? BehaviorTreeGraphValidator.Validate(behaviorTreeGraph).Messages
                : null;
        }

        private void OnSelectionChange()
        {
            TryBindFromSelection();
        }

        private void TryBindFromSelection()
        {
            if (Selection.activeObject is not BehaviorTreeGraphAsset behaviorTreeGraph)
            {
                return;
            }

            behaviorTreeGraph.SynchronizeTypedValues();
            BindGraph(behaviorTreeGraph);
            ShowMessages(BehaviorTreeGraphValidator.Validate(behaviorTreeGraph).Messages);
        }

        private void CompileCurrentGraph()
        {
            if (currentGraph is not BehaviorTreeGraphAsset behaviorTreeGraph)
            {
                return;
            }

            behaviorTreeGraph.SynchronizeTypedValues();
            BehaviorTreeGraphCompiler compiler = new BehaviorTreeGraphCompiler();
            GraphCompileReport report = compiler.Compile(behaviorTreeGraph);
            EditorUtility.SetDirty(behaviorTreeGraph);
            ShowMessages(report.Messages);
        }

        private void AddBlackboardEntry(BehaviorTreeBlackboardValueType valueType)
        {
            if (currentGraph is not BehaviorTreeGraphAsset behaviorTreeGraph)
            {
                return;
            }

            Undo.RecordObject(behaviorTreeGraph, $"Add Blackboard {valueType}");

            string key = GenerateBlackboardKey(behaviorTreeGraph, valueType);
            BehaviorTreeBlackboardEntry entry = new BehaviorTreeBlackboardEntry
            {
                Key = key,
                DisplayName = key,
                SerializedTypeName = valueType.ToString(),
                ValueType = valueType,
                DefaultValueData = BehaviorTreeValueData.CreateDefault(valueType)
            };
            behaviorTreeGraph.Blackboard.Add(entry);
            behaviorTreeGraph.SynchronizeTypedValues();

            EditorUtility.SetDirty(behaviorTreeGraph);
            BindGraph(behaviorTreeGraph);
            currentSelectionModel = entry;
            RefreshPanels();
        }

        private static string GenerateBlackboardKey(BehaviorTreeGraphAsset graphAsset, BehaviorTreeBlackboardValueType valueType)
        {
            int index = graphAsset.BlackboardEntries.Count() + 1;
            return $"{valueType}Key{index}";
        }
    }
}
