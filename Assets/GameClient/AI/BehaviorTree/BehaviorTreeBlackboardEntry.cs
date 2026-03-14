using System;
using Game.GraphTools;
using UnityEngine;

namespace Game.AI
{
    /// <summary>
    /// 黑板值的支持类型。
    /// </summary>
    public enum BehaviorTreeBlackboardValueType
    {
        Bool,
        Int,
        Float,
        String
    }

    [Serializable]
    /// <summary>
    /// 单个黑板键的定义，包含键名、值类型和默认值。
    /// </summary>
    public sealed class BehaviorTreeBlackboardEntry : BlackboardEntryBase, ISerializationCallbackReceiver
    {
        public BehaviorTreeBlackboardValueType ValueType = BehaviorTreeBlackboardValueType.Bool;

        [HideInInspector]
        public string DefaultValue = string.Empty;

        public BehaviorTreeValueData DefaultValueData = BehaviorTreeValueData.CreateDefault(BehaviorTreeBlackboardValueType.Bool);

        /// <summary>
        /// 序列化前同步旧格式默认值字符串，保证兼容旧资产。
        /// </summary>
        public void OnBeforeSerialize()
        {
            if (DefaultValueData == null)
            {
                DefaultValueData = BehaviorTreeValueData.CreateDefault(ValueType);
            }

            DefaultValueData.ValueType = ValueType;
            DefaultValue = DefaultValueData.ToLegacyString();
        }

        /// <summary>
        /// 反序列化后恢复 typed value。
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (DefaultValueData == null)
            {
                DefaultValueData = BehaviorTreeValueData.FromLegacyString(ValueType, DefaultValue);
            }
            else
            {
                DefaultValueData.ValueType = ValueType;
            }
        }
    }
}
