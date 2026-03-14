using System;
using System.Globalization;

namespace Game.AI
{
    [Serializable]
    /// <summary>
    /// 强类型值容器，用来统一表示黑板默认值和条件常量。
    /// </summary>
    public sealed class BehaviorTreeValueData
    {
        public BehaviorTreeBlackboardValueType ValueType = BehaviorTreeBlackboardValueType.Bool;
        public bool BoolValue;
        public int IntValue;
        public float FloatValue;
        public string StringValue = string.Empty;

        /// <summary>
        /// 创建指定类型的默认值容器。
        /// </summary>
        /// <param name="valueType">值类型。</param>
        /// <returns>新的值容器。</returns>
        public static BehaviorTreeValueData CreateDefault(BehaviorTreeBlackboardValueType valueType)
        {
            return new BehaviorTreeValueData
            {
                ValueType = valueType
            };
        }

        /// <summary>
        /// 从旧的字符串序列化格式恢复一个值容器。
        /// </summary>
        /// <param name="valueType">目标类型。</param>
        /// <param name="legacyValue">旧格式字符串。</param>
        /// <returns>恢复后的值容器。</returns>
        public static BehaviorTreeValueData FromLegacyString(BehaviorTreeBlackboardValueType valueType, string legacyValue)
        {
            BehaviorTreeValueData data = CreateDefault(valueType);
            data.SetFromObject(legacyValue);
            return data;
        }

        /// <summary>
        /// 复制一个完整的值容器。
        /// </summary>
        /// <returns>副本。</returns>
        public BehaviorTreeValueData Clone()
        {
            return new BehaviorTreeValueData
            {
                ValueType = ValueType,
                BoolValue = BoolValue,
                IntValue = IntValue,
                FloatValue = FloatValue,
                StringValue = StringValue
            };
        }

        /// <summary>
        /// 转换为运行时对象值。
        /// </summary>
        /// <returns>运行时对象。</returns>
        public object ToObject()
        {
            return ValueType switch
            {
                BehaviorTreeBlackboardValueType.Bool => BoolValue,
                BehaviorTreeBlackboardValueType.Int => IntValue,
                BehaviorTreeBlackboardValueType.Float => FloatValue,
                _ => StringValue ?? string.Empty
            };
        }

        /// <summary>
        /// 转换为兼容旧资产的字符串格式。
        /// </summary>
        /// <returns>旧格式字符串。</returns>
        public string ToLegacyString()
        {
            return ValueType switch
            {
                BehaviorTreeBlackboardValueType.Bool => BoolValue ? "true" : "false",
                BehaviorTreeBlackboardValueType.Int => IntValue.ToString(CultureInfo.InvariantCulture),
                BehaviorTreeBlackboardValueType.Float => FloatValue.ToString(CultureInfo.InvariantCulture),
                _ => StringValue ?? string.Empty
            };
        }

        /// <summary>
        /// 转换为调试友好的显示字符串。
        /// </summary>
        /// <returns>显示字符串。</returns>
        public string ToDisplayString()
        {
            return ValueType switch
            {
                BehaviorTreeBlackboardValueType.Bool => BoolValue ? "True" : "False",
                BehaviorTreeBlackboardValueType.Int => IntValue.ToString(CultureInfo.InvariantCulture),
                BehaviorTreeBlackboardValueType.Float => FloatValue.ToString("0.###", CultureInfo.InvariantCulture),
                _ => StringValue ?? string.Empty
            };
        }

        /// <summary>
        /// 修改值类型，并可选尝试保留当前值。
        /// </summary>
        /// <param name="valueType">新的值类型。</param>
        /// <param name="convertCurrentValue">是否尝试把当前值转换到新类型。</param>
        public void SetValueType(BehaviorTreeBlackboardValueType valueType, bool convertCurrentValue)
        {
            object currentValue = convertCurrentValue ? ToObject() : null;
            ValueType = valueType;
            if (convertCurrentValue)
            {
                SetFromObject(currentValue);
            }
        }

        /// <summary>
        /// 按当前值类型把对象写入容器。
        /// </summary>
        /// <param name="rawValue">待写入的原始对象。</param>
        public void SetFromObject(object rawValue)
        {
            switch (ValueType)
            {
                case BehaviorTreeBlackboardValueType.Bool:
                    BoolValue = TryConvertToBool(rawValue, out bool boolValue) && boolValue;
                    break;

                case BehaviorTreeBlackboardValueType.Int:
                    IntValue = TryConvertToInt(rawValue, out int intValue) ? intValue : 0;
                    break;

                case BehaviorTreeBlackboardValueType.Float:
                    FloatValue = TryConvertToFloat(rawValue, out float floatValue) ? floatValue : 0f;
                    break;

                default:
                    StringValue = rawValue?.ToString() ?? string.Empty;
                    break;
            }
        }

        /// <summary>
        /// 尝试把对象转换为布尔值。
        /// </summary>
        /// <param name="rawValue">原始对象。</param>
        /// <param name="value">转换后的布尔值。</param>
        /// <returns>是否转换成功。</returns>
        private static bool TryConvertToBool(object rawValue, out bool value)
        {
            if (rawValue is bool boolValue)
            {
                value = boolValue;
                return true;
            }

            if (bool.TryParse(rawValue?.ToString(), out bool parsedBool))
            {
                value = parsedBool;
                return true;
            }

            if (int.TryParse(rawValue?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedInt))
            {
                value = parsedInt != 0;
                return true;
            }

            value = false;
            return false;
        }

        /// <summary>
        /// 尝试把对象转换为整型。
        /// </summary>
        /// <param name="rawValue">原始对象。</param>
        /// <param name="value">转换后的整型值。</param>
        /// <returns>是否转换成功。</returns>
        private static bool TryConvertToInt(object rawValue, out int value)
        {
            if (rawValue is int intValue)
            {
                value = intValue;
                return true;
            }

            if (int.TryParse(rawValue?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedInt))
            {
                value = parsedInt;
                return true;
            }

            if (float.TryParse(rawValue?.ToString(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float parsedFloat))
            {
                value = (int)parsedFloat;
                return true;
            }

            value = 0;
            return false;
        }

        /// <summary>
        /// 尝试把对象转换为浮点型。
        /// </summary>
        /// <param name="rawValue">原始对象。</param>
        /// <param name="value">转换后的浮点值。</param>
        /// <returns>是否转换成功。</returns>
        private static bool TryConvertToFloat(object rawValue, out float value)
        {
            if (rawValue is float floatValue)
            {
                value = floatValue;
                return true;
            }

            if (rawValue is int intValue)
            {
                value = intValue;
                return true;
            }

            if (float.TryParse(rawValue?.ToString(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float parsedFloat))
            {
                value = parsedFloat;
                return true;
            }

            value = 0f;
            return false;
        }
    }
}
