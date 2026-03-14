using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditorInternal;
using System.Collections;
using Game.AI;

namespace Game.GraphTools.Editor
{
    public class GraphBlackboardPanel : VisualElement
    {
        private readonly Label headerLabel;
        private readonly VisualElement bodyContainer;
        private GraphAssetBase currentGraph;
        private Action<string, Action> applyChange;
        private BlackboardEntryBase selectedEntry;
        private ReorderableList blackboardList;

        public event Action<BlackboardEntryBase> BlackboardEntrySelected;

        public GraphBlackboardPanel()
        {
            style.marginBottom = 8f;
            style.paddingLeft = 6f;
            style.paddingRight = 6f;

            headerLabel = new Label("Blackboard");
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(headerLabel);

            bodyContainer = new VisualElement();
            bodyContainer.style.marginTop = 4f;
            Add(bodyContainer);
        }

        public void BindGraph(
            GraphAssetBase graphAsset,
            BlackboardEntryBase selectedEntry = null,
            Action<string, Action> onApplyChange = null)
        {
            currentGraph = graphAsset;
            this.selectedEntry = selectedEntry;
            applyChange = onApplyChange;
            bodyContainer.Clear();

            if (graphAsset == null)
            {
                AddMessage("No graph blackboard data.");
                return;
            }

            if (graphAsset is not BehaviorTreeGraphAsset behaviorTreeGraph)
            {
                AddMessage($"Entries: {graphAsset.Blackboard.Count}");
                return;
            }

            List<BehaviorTreeBlackboardEntry> entries = behaviorTreeGraph.BlackboardEntries.ToList();
            headerLabel.text = $"Blackboard ({entries.Count})";

            VisualElement listContainer = new VisualElement();
            listContainer.style.marginTop = 4f;
            bodyContainer.Add(listContainer);

            IMGUIContainer imguiContainer = new IMGUIContainer(() =>
            {
                if (currentGraph == null) return;
                
                // Ensure list is initialized or updated if count changed
                if (blackboardList == null || blackboardList.list.Count != entries.Count)
                {
                    blackboardList = new ReorderableList((IList)entries, typeof(BehaviorTreeBlackboardEntry), true, true, false, false);
                    blackboardList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Entries");
                    blackboardList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        var entry = entries[index];
                        string label = $"{entry.Key} [{entry.ValueType}] = {entry.DefaultValueData?.ToDisplayString() ?? string.Empty}";
                        EditorGUI.LabelField(rect, label);
                    };
                    blackboardList.onReorderCallbackWithDetails = (list, oldIdx, newIdx) =>
                    {
                        BehaviorTreeBlackboardEntry movedEntry = entries[newIdx];
                        ApplyChange("Reorder Blackboard", () => MoveBlackboardEntryToIndex(movedEntry, newIdx));
                    };
                    blackboardList.onSelectCallback = (list) =>
                    {
                        if (list.index >= 0 && list.index < entries.Count)
                        {
                            BlackboardEntrySelected?.Invoke(entries[list.index]);
                        }
                    };
                }

                if (selectedEntry is BehaviorTreeBlackboardEntry currentEntry)
                {
                    int selectedIndex = entries.IndexOf(currentEntry);
                    if (selectedIndex >= 0) blackboardList.index = selectedIndex;
                }

                blackboardList.DoLayoutList();
            });
            listContainer.Add(imguiContainer);

            if (entries.Count == 0)
            {
                Label emptyLabel = new Label("No blackboard entries.");
                emptyLabel.style.marginTop = 4f;
                emptyLabel.style.whiteSpace = WhiteSpace.Normal;
                bodyContainer.Add(emptyLabel);
            }

            VisualElement footer = new VisualElement();
            footer.style.flexDirection = FlexDirection.Row;
            footer.style.justifyContent = Justify.FlexEnd;
            footer.style.marginTop = 4f;

            Button addButton = new Button(ShowAddBlackboardMenu)
            {
                text = "+"
            };
            addButton.style.width = 28f;
            addButton.style.marginRight = 4f;
            footer.Add(addButton);

            Button removeButton = new Button(() =>
            {
                if (blackboardList == null || blackboardList.index < 0 || blackboardList.index >= entries.Count)
                {
                    return;
                }

                var entry = entries[blackboardList.index];
                ApplyChange("Delete Blackboard Entry", () => currentGraph.Blackboard.Remove(entry));
            })
            {
                text = "-"
            };
            removeButton.style.width = 28f;
            removeButton.SetEnabled(entries.Count > 0);
            footer.Add(removeButton);

            bodyContainer.Add(footer);
        }

        private void ShowAddBlackboardMenu()
        {
            if (currentGraph is not BehaviorTreeGraphAsset behaviorTreeGraph)
            {
                return;
            }

            GenericMenu menu = new GenericMenu();
            foreach (BehaviorTreeBlackboardValueType valueType in Enum.GetValues(typeof(BehaviorTreeBlackboardValueType)))
            {
                BehaviorTreeBlackboardValueType capturedType = valueType;
                menu.AddItem(new GUIContent(capturedType.ToString()), false, () =>
                    ApplyChange($"Add Blackboard {capturedType}", () =>
                    {
                        string key = GenerateBlackboardKey(behaviorTreeGraph, capturedType);
                        behaviorTreeGraph.Blackboard.Add(new BehaviorTreeBlackboardEntry
                        {
                            Key = key,
                            DisplayName = key,
                            SerializedTypeName = capturedType.ToString(),
                            ValueType = capturedType,
                            DefaultValueData = BehaviorTreeValueData.CreateDefault(capturedType)
                        });
                    }));
            }

            menu.ShowAsContext();
        }

        private void MoveBlackboardEntryToIndex(BehaviorTreeBlackboardEntry entry, int targetIndex)
        {
            if (currentGraph == null || entry == null)
            {
                return;
            }

            int currentIndex = currentGraph.Blackboard.IndexOf(entry);
            if (currentIndex < 0 || targetIndex < 0 || targetIndex >= currentGraph.Blackboard.Count)
            {
                return;
            }

            currentGraph.Blackboard.RemoveAt(currentIndex);
            currentGraph.Blackboard.Insert(targetIndex, entry);
        }

        private void ApplyChange(string actionName, Action change)
        {
            applyChange?.Invoke(actionName, change);
        }

        private static string GenerateBlackboardKey(BehaviorTreeGraphAsset graphAsset, BehaviorTreeBlackboardValueType valueType)
        {
            int index = graphAsset.BlackboardEntries.Count() + 1;
            return $"{valueType}Key{index}";
        }

        private void AddMessage(string message)
        {
            headerLabel.text = "Blackboard";
            Label label = new Label(message);
            label.style.whiteSpace = WhiteSpace.Normal;
            bodyContainer.Add(label);
        }
    }
}
