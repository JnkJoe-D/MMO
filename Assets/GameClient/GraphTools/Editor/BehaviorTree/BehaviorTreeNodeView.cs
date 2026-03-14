using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Game.AI;

namespace Game.GraphTools.Editor
{
    public sealed class BehaviorTreeNodeView : BaseNodeView
    {
        private readonly Action<string, Action> applyChange;
        private readonly BehaviorTreeGraphAsset graphAsset;
        private readonly IEdgeConnectorListener connectorListener;
        private readonly Label nodeTypeLabel = new Label();
        private readonly TextField nodeNameField = new TextField();
        private readonly VisualElement parameterContainer = new VisualElement();
        private readonly VisualElement expectedValueContainer = new VisualElement();
        private EnumField compositeModeField;
        private TextField descriptionField;
        private PopupField<string> blackboardKeyField;
        private EnumField comparisonField;
        private Toggle expectedBoolField;
        private IntegerField expectedIntField;
        private FloatField expectedFloatField;
        private TextField expectedTextField;
        private BehaviorTreeBlackboardValueType currentExpectedValueType = BehaviorTreeBlackboardValueType.String;
        private EnumField abortModeField;
        private TextField serviceKeyField;
        private FloatField intervalField;
        private TextField taskKeyField;

        public BehaviorTreeNodeView(
            BehaviorTreeGraphAsset ownerGraph,
            BehaviorTreeNodeModelBase model,
            Action<string, Action> onApplyChange,
            IEdgeConnectorListener listener)
        {
            graphAsset = ownerGraph;
            applyChange = onApplyChange;
            connectorListener = listener;
            Bind(model);
            ConfigureLayout();
            ConfigurePorts(model);
            BuildParameterFields(model);
            RefreshFromModel();
            RefreshExpandedState();
            RefreshPorts();
        }

        public void RefreshFromModel()
        {
            if (Model is not BehaviorTreeNodeModelBase model)
            {
                return;
            }

            title = model.Title;
            nodeTypeLabel.text = model.NodeKind.ToString().ToUpperInvariant();

            UpdateFieldIfNoFocus(nodeNameField, model.Title ?? string.Empty);
            UpdateFieldIfNoFocus(descriptionField, model.Description ?? string.Empty);

            if (compositeModeField != null && model is BehaviorTreeCompositeNodeModel compositeNode)
            {
                compositeModeField.SetValueWithoutNotify(compositeNode.CompositeMode);
            }

            if (blackboardKeyField != null && model is BehaviorTreeConditionNodeModel conditionNode)
            {
                RefreshBlackboardChoices(conditionNode.BlackboardKey);
            }

            if (comparisonField != null && model is BehaviorTreeConditionNodeModel comparisonNode)
            {
                comparisonField.SetValueWithoutNotify(comparisonNode.Comparison);
            }

            if (model is BehaviorTreeConditionNodeModel expectedValueNode)
            {
                RefreshExpectedValueField(expectedValueNode);
            }

            if (abortModeField != null && model is BehaviorTreeConditionNodeModel abortNode)
            {
                abortModeField.SetValueWithoutNotify(abortNode.AbortMode);
            }

            if (serviceKeyField != null && model is BehaviorTreeServiceNodeModel serviceNode)
            {
                UpdateFieldIfNoFocus(serviceKeyField, serviceNode.ServiceKey ?? string.Empty);
            }

            if (intervalField != null && model is BehaviorTreeServiceNodeModel intervalNode)
            {
                var focused = intervalField.panel?.focusController?.focusedElement as VisualElement;
                if (focused == null || (focused != intervalField && !intervalField.Contains(focused)))
                {
                    intervalField.SetValueWithoutNotify(intervalNode.IntervalSeconds);
                }
            }

            if (taskKeyField != null && model is BehaviorTreeActionNodeModel actionNode)
            {
                UpdateFieldIfNoFocus(taskKeyField, actionNode.TaskKey ?? string.Empty);
            }

            ApplyNodeColor(model);
        }

        private void UpdateFieldIfNoFocus<T>(BaseField<T> field, T newValue)
        {
            if (field == null) return;
            var focused = field.panel?.focusController?.focusedElement as VisualElement;
            if (focused == null || (focused != field && !field.Contains(focused)))
            {
                field.SetValueWithoutNotify(newValue);
            }
        }

        private readonly VisualElement bodyContent = new VisualElement();

        private void ConfigureLayout()
        {
            style.minWidth = 240f;
            style.maxWidth = 280f;
            capabilities |= Capabilities.Deletable;

            // Hide default elements we don't need or want to reposition
            titleButtonContainer.style.display = DisplayStyle.None;
            topContainer.style.flexDirection = FlexDirection.Column;
            topContainer.style.alignItems = Align.Stretch;

            // Clear and restructure topContainer
            inputContainer.RemoveFromHierarchy();
            titleContainer.RemoveFromHierarchy();
            outputContainer.RemoveFromHierarchy();

            // 1. Input Port at the top
            topContainer.Add(inputContainer);
            inputContainer.style.flexDirection = FlexDirection.Row;
            inputContainer.style.justifyContent = Justify.Center;
            inputContainer.style.minHeight = 20f;
            inputContainer.style.paddingTop = 6f;
            inputContainer.style.paddingBottom = 2f;

            // 2. Body Content in the middle
            topContainer.Add(bodyContent);
            bodyContent.style.flexDirection = FlexDirection.Column;
            bodyContent.style.alignItems = Align.Stretch;
            bodyContent.style.paddingLeft = 10f;
            bodyContent.style.paddingRight = 10f;
            bodyContent.style.paddingTop = 4f;
            bodyContent.style.paddingBottom = 4f;

            // 3. Output Port at the bottom
            topContainer.Add(outputContainer);
            outputContainer.style.flexDirection = FlexDirection.Row;
            outputContainer.style.justifyContent = Justify.Center;
            outputContainer.style.minHeight = 20f;
            outputContainer.style.paddingTop = 2f;
            outputContainer.style.paddingBottom = 6f;

            // Apply styling to the entire node (mainContainer)
            mainContainer.style.borderTopWidth = 1f;
            mainContainer.style.borderBottomWidth = 1f;
            mainContainer.style.borderLeftWidth = 1f;
            mainContainer.style.borderRightWidth = 1f;
            mainContainer.style.borderTopColor = new Color(0.12f, 0.12f, 0.12f);
            mainContainer.style.borderBottomColor = new Color(0.12f, 0.12f, 0.12f);
            mainContainer.style.borderLeftColor = new Color(0.12f, 0.12f, 0.12f);
            mainContainer.style.borderRightColor = new Color(0.12f, 0.12f, 0.12f);
            mainContainer.style.borderTopLeftRadius = 8f;
            mainContainer.style.borderTopRightRadius = 8f;
            mainContainer.style.borderBottomLeftRadius = 8f;
            mainContainer.style.borderBottomRightRadius = 8f;

            // Setup elements inside bodyContent
            nodeTypeLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            nodeTypeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nodeTypeLabel.style.fontSize = 12f;
            nodeTypeLabel.style.marginBottom = 6f;
            nodeTypeLabel.style.color = new Color(0.95f, 0.95f, 0.95f);
            bodyContent.Add(nodeTypeLabel);

            nodeNameField.label = string.Empty;
            StyleField(nodeNameField, true);
            nodeNameField.style.marginBottom = 8f;
            nodeNameField.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (Model is not BehaviorTreeNodeModelBase model)
                {
                    return;
                }

                string newValue = nodeNameField.value ?? string.Empty;
                if (model.Title != newValue)
                {
                    applyChange?.Invoke("Edit BehaviorTree Node Title", () => model.Title = newValue);
                }
            });
            bodyContent.Add(nodeNameField);

            parameterContainer.style.flexDirection = FlexDirection.Column;
            parameterContainer.style.marginTop = 2f;
            bodyContent.Add(parameterContainer);

            // Cleanup hidden/unused GraphView containers
            extensionContainer.style.display = DisplayStyle.None;
        }

        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            var port = Port.Create<Edge>(orientation, direction, capacity, type);
            port.AddManipulator(new EdgeConnector<Edge>(connectorListener));
            return port;
        }

        private void ConfigurePorts(BehaviorTreeNodeModelBase model)
        {
            switch (model)
            {
                case BehaviorTreeRootNodeModel:
                    CreateFlowOutput(string.Empty, Port.Capacity.Single, Orientation.Vertical);
                    break;
                case BehaviorTreeCompositeNodeModel:
                    CreateFlowInput(string.Empty, Port.Capacity.Single, Orientation.Vertical);
                    CreateFlowOutput(string.Empty, Port.Capacity.Multi, Orientation.Vertical);
                    break;
                case BehaviorTreeConditionNodeModel:
                case BehaviorTreeServiceNodeModel:
                    CreateFlowInput(string.Empty, Port.Capacity.Single, Orientation.Vertical);
                    CreateFlowOutput(string.Empty, Port.Capacity.Single, Orientation.Vertical);
                    break;
                case BehaviorTreeActionNodeModel:
                    CreateFlowInput(string.Empty, Port.Capacity.Single, Orientation.Vertical);
                    break;
            }
        }

        private void BuildParameterFields(BehaviorTreeNodeModelBase model)
        {
            descriptionField = CreateCompactTextField("Description", value =>
            {
                if (Model is BehaviorTreeNodeModelBase nodeModel)
                {
                    nodeModel.Description = value;
                }
            });
            parameterContainer.Add(descriptionField);

            switch (model)
            {
                case BehaviorTreeCompositeNodeModel:
                    compositeModeField = CreateCompactEnumField("Mode", value =>
                    {
                        if (Model is BehaviorTreeCompositeNodeModel nodeModel)
                        {
                            nodeModel.CompositeMode = (BehaviorTreeCompositeMode)value;
                        }
                    }, BehaviorTreeCompositeMode.Sequence);
                    parameterContainer.Add(compositeModeField);
                    break;
                case BehaviorTreeConditionNodeModel:
                    blackboardKeyField = CreateBlackboardKeyField(value =>
                    {
                        if (Model is BehaviorTreeConditionNodeModel nodeModel)
                        {
                            nodeModel.BlackboardKey = value;
                            SyncConditionExpectedValueType(nodeModel);
                        }
                    });
                    parameterContainer.Add(blackboardKeyField);

                    comparisonField = CreateCompactEnumField("Compare", value =>
                    {
                        if (Model is BehaviorTreeConditionNodeModel nodeModel)
                        {
                            nodeModel.Comparison = (BehaviorTreeComparisonOperator)value;
                        }
                    }, BehaviorTreeComparisonOperator.IsSet);
                    parameterContainer.Add(comparisonField);

                    expectedValueContainer.style.flexDirection = FlexDirection.Column;
                    parameterContainer.Add(expectedValueContainer);
                    SyncConditionExpectedValueType((BehaviorTreeConditionNodeModel)model);
                    RebuildExpectedValueField((BehaviorTreeConditionNodeModel)model);

                    abortModeField = CreateCompactEnumField("Abort", value =>
                    {
                        if (Model is BehaviorTreeConditionNodeModel nodeModel)
                        {
                            nodeModel.AbortMode = (BehaviorTreeAbortMode)value;
                        }
                    }, BehaviorTreeAbortMode.Self);
                    parameterContainer.Add(abortModeField);
                    break;
                case BehaviorTreeServiceNodeModel:
                    serviceKeyField = CreateCompactTextField("Service", value =>
                    {
                        if (Model is BehaviorTreeServiceNodeModel nodeModel)
                        {
                            nodeModel.ServiceKey = value;
                        }
                    });
                    parameterContainer.Add(serviceKeyField);

                    intervalField = new FloatField("Interval");
                    StyleField(intervalField);
                    intervalField.RegisterValueChangedCallback(evt =>
                        applyChange?.Invoke("Edit Interval", () =>
                        {
                            if (Model is BehaviorTreeServiceNodeModel nodeModel)
                            {
                                nodeModel.IntervalSeconds = evt.newValue;
                            }
                        }));
                    parameterContainer.Add(intervalField);
                    break;
                case BehaviorTreeActionNodeModel:
                    taskKeyField = CreateCompactTextField("Task", value =>
                    {
                        if (Model is BehaviorTreeActionNodeModel nodeModel)
                        {
                            nodeModel.TaskKey = value;
                        }
                    });
                    parameterContainer.Add(taskKeyField);
                    break;
            }
        }

        private TextField CreateCompactTextField(string label, Action<string> applyValue)
        {
            TextField field = new TextField(label);
            StyleField(field);
            field.RegisterCallback<FocusOutEvent>(evt =>
            {
                string newValue = field.value ?? string.Empty;
                applyChange?.Invoke($"Edit {label}", () => applyValue(newValue));
            });
            return field;
        }

        private PopupField<string> CreateBlackboardKeyField(Action<string> applyValue)
        {
            List<string> choices = BuildBlackboardKeyChoices(string.Empty);
            PopupField<string> field = new PopupField<string>("Blackboard Key", choices, choices[0]);
            StylePopupField(field);
            field.RegisterValueChangedCallback(evt =>
                applyChange?.Invoke("Edit Blackboard Key", () => applyValue(evt.newValue == "<None>" ? string.Empty : evt.newValue)));
            return field;
        }

        private EnumField CreateCompactEnumField<TEnum>(string label, Action<Enum> applyValue, TEnum initialValue)
            where TEnum : Enum
        {
            EnumField field = new EnumField(label, initialValue);
            StyleField(field);
            field.RegisterValueChangedCallback(evt =>
                applyChange?.Invoke($"Edit {label}", () => applyValue((Enum)evt.newValue)));
            return field;
        }

        private void RefreshBlackboardChoices(string currentValue)
        {
            if (blackboardKeyField == null)
            {
                return;
            }

            List<string> choices = BuildBlackboardKeyChoices(currentValue);
            blackboardKeyField.choices = choices;
            blackboardKeyField.SetValueWithoutNotify(string.IsNullOrEmpty(currentValue) ? "<None>" : currentValue);
        }

        private List<string> BuildBlackboardKeyChoices(string currentValue)
        {
            List<string> choices = new List<string> { "<None>" };

            if (graphAsset != null)
            {
                choices.AddRange(graphAsset.BlackboardEntries
                    .Select(entry => entry?.Key)
                    .Where(key => !string.IsNullOrWhiteSpace(key))
                    .Distinct());
            }

            if (!string.IsNullOrWhiteSpace(currentValue) && !choices.Contains(currentValue))
            {
                choices.Add(currentValue);
            }

            return choices;
        }

        private void RefreshExpectedValueField(BehaviorTreeConditionNodeModel model)
        {
            SyncConditionExpectedValueType(model);

            expectedValueContainer.style.display = model.Comparison == BehaviorTreeComparisonOperator.IsSet
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            BehaviorTreeBlackboardValueType valueType = ResolveConditionExpectedValueType(model);
            if (expectedBoolField == null && expectedIntField == null && expectedFloatField == null && expectedTextField == null ||
                currentExpectedValueType != valueType)
            {
                RebuildExpectedValueField(model);
            }

            switch (valueType)
            {
                case BehaviorTreeBlackboardValueType.Bool:
                    if (expectedBoolField != null)
                    {
                        expectedBoolField.SetValueWithoutNotify(model.ExpectedValueData.BoolValue);
                    }
                    break;
                case BehaviorTreeBlackboardValueType.Int:
                    UpdateFieldIfNoFocus(expectedIntField, model.ExpectedValueData.IntValue);
                    break;
                case BehaviorTreeBlackboardValueType.Float:
                    UpdateFieldIfNoFocus(expectedFloatField, model.ExpectedValueData.FloatValue);
                    break;
                default:
                    UpdateFieldIfNoFocus(expectedTextField, model.ExpectedValueData.StringValue ?? string.Empty);
                    break;
            }
        }

        private void RebuildExpectedValueField(BehaviorTreeConditionNodeModel model)
        {
            expectedValueContainer.Clear();
            expectedBoolField = null;
            expectedIntField = null;
            expectedFloatField = null;
            expectedTextField = null;

            currentExpectedValueType = ResolveConditionExpectedValueType(model);

            switch (currentExpectedValueType)
            {
                case BehaviorTreeBlackboardValueType.Bool:
                    expectedBoolField = new Toggle("Expected");
                    expectedBoolField.RegisterValueChangedCallback(evt =>
                        applyChange?.Invoke("Edit Expected", () =>
                        {
                            if (Model is BehaviorTreeConditionNodeModel nodeModel)
                            {
                                nodeModel.ExpectedValueData.BoolValue = evt.newValue;
                            }
                        }));
                    expectedValueContainer.Add(expectedBoolField);
                    break;

                case BehaviorTreeBlackboardValueType.Int:
                    expectedIntField = new IntegerField("Expected");
                    StyleField(expectedIntField);
                    expectedIntField.RegisterCallback<FocusOutEvent>(_ =>
                        applyChange?.Invoke("Edit Expected", () =>
                        {
                            if (Model is BehaviorTreeConditionNodeModel nodeModel)
                            {
                                nodeModel.ExpectedValueData.IntValue = expectedIntField.value;
                            }
                        }));
                    expectedValueContainer.Add(expectedIntField);
                    break;

                case BehaviorTreeBlackboardValueType.Float:
                    expectedFloatField = new FloatField("Expected");
                    StyleField(expectedFloatField);
                    expectedFloatField.RegisterCallback<FocusOutEvent>(_ =>
                        applyChange?.Invoke("Edit Expected", () =>
                        {
                            if (Model is BehaviorTreeConditionNodeModel nodeModel)
                            {
                                nodeModel.ExpectedValueData.FloatValue = expectedFloatField.value;
                            }
                        }));
                    expectedValueContainer.Add(expectedFloatField);
                    break;

                default:
                    expectedTextField = CreateCompactTextField("Expected", value =>
                    {
                        if (Model is BehaviorTreeConditionNodeModel nodeModel)
                        {
                            nodeModel.ExpectedValueData.StringValue = value;
                        }
                    });
                    expectedValueContainer.Add(expectedTextField);
                    break;
            }
        }

        private void SyncConditionExpectedValueType(BehaviorTreeConditionNodeModel model)
        {
            if (model == null)
            {
                return;
            }

            BehaviorTreeBlackboardValueType valueType = ResolveConditionExpectedValueType(model);
            model.ExpectedValueData ??= BehaviorTreeValueData.CreateDefault(valueType);
            if (model.ExpectedValueData.ValueType != valueType)
            {
                model.ExpectedValueData.SetValueType(valueType, true);
            }
        }

        private BehaviorTreeBlackboardValueType ResolveConditionExpectedValueType(BehaviorTreeConditionNodeModel model)
        {
            if (graphAsset != null)
            {
                BehaviorTreeBlackboardEntry entry = graphAsset.BlackboardEntries
                    .FirstOrDefault(item => item != null && item.Key == model.BlackboardKey);
                if (entry != null)
                {
                    return entry.ValueType;
                }
            }

            return model.ExpectedValueData?.ValueType ?? BehaviorTreeBlackboardValueType.String;
        }

        private static void StyleField(BaseField<string> field, bool centerText = false)
        {
            field.style.minHeight = 24f;
            field.style.unityFontStyleAndWeight = FontStyle.Normal;

            VisualElement inputElement = field.Q(className: "unity-text-input");
            if (inputElement != null)
            {
                inputElement.style.minHeight = 20f;
                inputElement.style.backgroundColor = new Color(0.10f, 0.10f, 0.10f, 0.95f);
                inputElement.style.color = Color.white;
                inputElement.style.paddingTop = 2f;
                inputElement.style.paddingBottom = 2f;
                if (centerText)
                {
                    inputElement.style.unityTextAlign = TextAnchor.MiddleCenter;
                }
            }
        }

        private static void StyleField(BaseField<Enum> field)
        {
            field.style.minHeight = 24f;
            VisualElement inputElement = field.Q(className: "unity-base-popup-field__input");
            if (inputElement != null)
            {
                inputElement.style.minHeight = 20f;
                inputElement.style.backgroundColor = new Color(0.10f, 0.10f, 0.10f, 0.95f);
                inputElement.style.color = Color.white;
            }
        }

        private static void StyleField(BaseField<float> field)
        {
            field.style.minHeight = 24f;
            VisualElement inputElement = field.Q(className: "unity-text-input");
            if (inputElement != null)
            {
                inputElement.style.minHeight = 20f;
                inputElement.style.backgroundColor = new Color(0.10f, 0.10f, 0.10f, 0.95f);
                inputElement.style.color = Color.white;
            }
        }

        private static void StyleField(BaseField<int> field)
        {
            field.style.minHeight = 24f;
            VisualElement inputElement = field.Q(className: "unity-text-input");
            if (inputElement != null)
            {
                inputElement.style.minHeight = 20f;
                inputElement.style.backgroundColor = new Color(0.10f, 0.10f, 0.10f, 0.95f);
                inputElement.style.color = Color.white;
            }
        }

        private static void StylePopupField(PopupField<string> field)
        {
            field.style.minHeight = 24f;
            VisualElement inputElement = field.Q(className: "unity-base-popup-field__input");
            if (inputElement != null)
            {
                inputElement.style.minHeight = 20f;
                inputElement.style.backgroundColor = new Color(0.10f, 0.10f, 0.10f, 0.95f);
                inputElement.style.color = Color.white;
            }
        }

        private void ApplyNodeColor(BehaviorTreeNodeModelBase model)
        {
            Color backgroundColor = model switch
            {
                BehaviorTreeRootNodeModel => new Color(0.22f, 0.28f, 0.36f),
                BehaviorTreeCompositeNodeModel => new Color(0.18f, 0.36f, 0.65f),
                BehaviorTreeConditionNodeModel => new Color(0.17f, 0.50f, 0.33f),
                BehaviorTreeServiceNodeModel => new Color(0.61f, 0.45f, 0.16f),
                BehaviorTreeActionNodeModel => new Color(0.63f, 0.38f, 0.17f),
                _ => new Color(0.22f, 0.22f, 0.22f)
            };

            mainContainer.style.backgroundColor = backgroundColor;
        }
    }
}
