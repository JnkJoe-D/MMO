using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Game.AI
{
    /// <summary>
    /// 一次黑板值变化事件的数据载体。
    /// </summary>
    public readonly struct BehaviorTreeBlackboardChange
    {
        /// <summary>
        /// 构造一条黑板变化记录。
        /// </summary>
        /// <param name="key">变化的键。</param>
        /// <param name="oldValue">旧值。</param>
        /// <param name="newValue">新值。</param>
        /// <param name="valueType">声明的值类型。</param>
        public BehaviorTreeBlackboardChange(
            string key,
            object oldValue,
            object newValue,
            BehaviorTreeBlackboardValueType? valueType)
        {
            Key = key ?? string.Empty;
            OldValue = oldValue;
            NewValue = newValue;
            ValueType = valueType;
        }

        public string Key { get; }
        public object OldValue { get; }
        public object NewValue { get; }
        public BehaviorTreeBlackboardValueType? ValueType { get; }
    }

    /// <summary>
    /// 行为树运行时黑板，负责保存键值、类型定义和变更事件。
    /// </summary>
    public sealed class BehaviorTreeBlackboard
    {
        private readonly Dictionary<string, BehaviorTreeBlackboardEntry> definitions =
            new Dictionary<string, BehaviorTreeBlackboardEntry>(StringComparer.Ordinal);

        private readonly Dictionary<string, object> values =
            new Dictionary<string, object>(StringComparer.Ordinal);

        public event Action<BehaviorTreeBlackboardChange> ValueChanged;

        /// <summary>
        /// 创建一个空黑板。
        /// </summary>
        public BehaviorTreeBlackboard()
        {
        }

        /// <summary>
        /// 用给定条目集合初始化黑板。
        /// </summary>
        /// <param name="entries">初始黑板条目。</param>
        public BehaviorTreeBlackboard(IEnumerable<BehaviorTreeBlackboardEntry> entries)
        {
            Initialize(entries);
        }

        public int Count => values.Count;
        public IEnumerable<string> Keys => values.Keys;
        public IEnumerable<KeyValuePair<string, object>> Entries => values;
        public IEnumerable<BehaviorTreeBlackboardEntry> Definitions => definitions.Values.Select(CloneEntry);

        /// <summary>
        /// 清空并按指定条目重新初始化黑板。
        /// </summary>
        /// <param name="entries">新的黑板条目集合。</param>
        public void Initialize(IEnumerable<BehaviorTreeBlackboardEntry> entries)
        {
            definitions.Clear();
            values.Clear();
            RegisterEntries(entries, preserveExistingValues: false);
        }

        /// <summary>
        /// 批量注册黑板条目。
        /// </summary>
        /// <param name="entries">要注册的条目集合。</param>
        /// <param name="preserveExistingValues">是否保留已有值。</param>
        public void RegisterEntries(IEnumerable<BehaviorTreeBlackboardEntry> entries, bool preserveExistingValues = true)
        {
            if (entries == null)
            {
                return;
            }

            foreach (BehaviorTreeBlackboardEntry entry in entries.Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Key)))
            {
                RegisterEntry(entry, preserveExistingValues);
            }
        }

        /// <summary>
        /// 注册单个黑板条目。
        /// </summary>
        /// <param name="entry">要注册的条目。</param>
        /// <param name="preserveExistingValue">是否保留已有值。</param>
        public void RegisterEntry(BehaviorTreeBlackboardEntry entry, bool preserveExistingValue = true)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
            {
                return;
            }

            definitions[entry.Key] = CloneEntry(entry);

            object resolvedValue;
            if (preserveExistingValue && values.TryGetValue(entry.Key, out object existingValue))
            {
                resolvedValue = NormalizeValue(entry.Key, existingValue);
            }
            else
            {
                resolvedValue = entry.DefaultValueData?.ToObject() ?? GetDefaultValue(entry.ValueType);
            }

            values[entry.Key] = resolvedValue;
        }

        /// <summary>
        /// 把所有黑板值恢复到默认值。
        /// </summary>
        public void ResetToDefaults()
        {
            foreach (KeyValuePair<string, BehaviorTreeBlackboardEntry> pair in definitions)
            {
                BehaviorTreeBlackboardEntry definition = pair.Value;
                values[pair.Key] = definition.DefaultValueData?.ToObject() ?? GetDefaultValue(definition.ValueType);
            }
        }

        /// <summary>
        /// 把指定键恢复到默认值。
        /// </summary>
        /// <param name="key">目标键。</param>
        /// <returns>是否重置成功。</returns>
        public bool ResetValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || !definitions.TryGetValue(key, out BehaviorTreeBlackboardEntry definition))
            {
                return false;
            }

            object defaultValue = definition.DefaultValueData?.ToObject() ?? GetDefaultValue(definition.ValueType);
            return SetValueInternal(key, defaultValue, raiseEvent: true);
        }

        /// <summary>
        /// 判断黑板中是否存在指定键。
        /// </summary>
        /// <param name="key">目标键。</param>
        /// <returns>是否存在。</returns>
        public bool Contains(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && values.ContainsKey(key);
        }

        /// <summary>
        /// 尝试读取某个键的定义。
        /// </summary>
        /// <param name="key">目标键。</param>
        /// <param name="entry">输出的定义副本。</param>
        /// <returns>是否读取成功。</returns>
        public bool TryGetDefinition(string key, out BehaviorTreeBlackboardEntry entry)
        {
            if (string.IsNullOrWhiteSpace(key) || !definitions.TryGetValue(key, out BehaviorTreeBlackboardEntry foundEntry))
            {
                entry = null;
                return false;
            }

            entry = CloneEntry(foundEntry);
            return true;
        }

        /// <summary>
        /// 获取已注册键的声明类型。
        /// </summary>
        /// <param name="key">目标键。</param>
        /// <returns>值类型；若未注册则返回空。</returns>
        public BehaviorTreeBlackboardValueType? GetRegisteredValueType(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || !definitions.TryGetValue(key, out BehaviorTreeBlackboardEntry entry))
            {
                return null;
            }

            return entry.ValueType;
        }

        /// <summary>
        /// 尝试读取某个键的原始对象值。
        /// </summary>
        /// <param name="key">目标键。</param>
        /// <param name="value">输出的原始值。</param>
        /// <returns>是否读取成功。</returns>
        public bool TryGetRawValue(string key, out object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = null;
                return false;
            }

            return values.TryGetValue(key, out value);
        }

        /// <summary>
        /// 读取某个键的原始值，失败时返回默认值。
        /// </summary>
        /// <param name="key">目标键。</param>
        /// <param name="defaultValue">失败时返回的默认值。</param>
        /// <returns>原始对象值。</returns>
        public object GetRawValueOrDefault(string key, object defaultValue = null)
        {
            return TryGetRawValue(key, out object value) ? value : defaultValue;
        }

        /// <summary>
        /// 尝试把某个键读成 typed value。
        /// </summary>
        /// <param name="key">目标键。</param>
        /// <param name="valueData">输出的 typed value 副本。</param>
        /// <returns>是否读取成功。</returns>
        public bool TryGetValueData(string key, out BehaviorTreeValueData valueData)
        {
            if (!TryGetRawValue(key, out object rawValue))
            {
                valueData = null;
                return false;
            }

            BehaviorTreeBlackboardValueType valueType = GetRegisteredValueType(key) ?? InferValueType(rawValue);
            valueData = BehaviorTreeValueData.CreateDefault(valueType);
            valueData.SetFromObject(rawValue);
            return true;
        }

        /// <summary>
        /// 尝试读取强类型黑板值。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="key">目标键。</param>
        /// <param name="value">输出的强类型值。</param>
        /// <returns>是否读取成功。</returns>
        public bool TryGetValue<T>(string key, out T value)
        {
            if (!TryGetRawValue(key, out object rawValue) || !TryConvertValue(rawValue, typeof(T), out object convertedValue))
            {
                value = default;
                return false;
            }

            value = (T)convertedValue;
            return true;
        }

        /// <summary>
        /// 读取强类型黑板值，失败时返回默认值。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="key">目标键。</param>
        /// <param name="defaultValue">失败时返回的默认值。</param>
        /// <returns>读到的值或默认值。</returns>
        public T GetValueOrDefault<T>(string key, T defaultValue = default)
        {
            return TryGetValue(key, out T value) ? value : defaultValue;
        }

        /// <summary>
        /// 写入黑板值，并按已注册类型进行归一化。
        /// </summary>
        /// <param name="key">目标键。</param>
        /// <param name="value">要写入的值。</param>
        /// <returns>值是否发生变化。</returns>
        public bool SetValue(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            return SetValueInternal(key, NormalizeValue(key, value), raiseEvent: true);
        }

        /// <summary>
        /// 内部写值实现；仅在值变化时触发事件。
        /// </summary>
        /// <param name="key">目标键。</param>
        /// <param name="value">新值。</param>
        /// <param name="raiseEvent">是否触发变更事件。</param>
        /// <returns>值是否发生变化。</returns>
        private bool SetValueInternal(string key, object value, bool raiseEvent)
        {
            values.TryGetValue(key, out object oldValue);
            if (ValuesEqual(oldValue, value))
            {
                return false;
            }

            values[key] = value;

            if (raiseEvent)
            {
                ValueChanged?.Invoke(new BehaviorTreeBlackboardChange(
                    key,
                    oldValue,
                    value,
                    GetRegisteredValueType(key)));
            }

            return true;
        }

        /// <summary>
        /// 按黑板定义类型把值归一化。
        /// </summary>
        /// <param name="key">目标键。</param>
        /// <param name="value">原始值。</param>
        /// <returns>归一化后的值。</returns>
        private object NormalizeValue(string key, object value)
        {
            if (!definitions.TryGetValue(key, out BehaviorTreeBlackboardEntry entry))
            {
                return value;
            }

            BehaviorTreeValueData typedValue = entry.DefaultValueData?.Clone() ?? BehaviorTreeValueData.CreateDefault(entry.ValueType);
            typedValue.SetValueType(entry.ValueType, false);
            typedValue.SetFromObject(value);
            return typedValue.ToObject();
        }

        /// <summary>
        /// 克隆一个黑板条目定义。
        /// </summary>
        /// <param name="entry">原始条目。</param>
        /// <returns>克隆后的条目。</returns>
        private static BehaviorTreeBlackboardEntry CloneEntry(BehaviorTreeBlackboardEntry entry)
        {
            return new BehaviorTreeBlackboardEntry
            {
                Key = entry.Key,
                DisplayName = entry.DisplayName,
                SerializedTypeName = entry.SerializedTypeName,
                ValueType = entry.ValueType,
                DefaultValueData = entry.DefaultValueData?.Clone() ?? BehaviorTreeValueData.CreateDefault(entry.ValueType)
            };
        }

        /// <summary>
        /// 根据对象实例推断值类型。
        /// </summary>
        /// <param name="value">原始对象。</param>
        /// <returns>推断出的类型。</returns>
        private static BehaviorTreeBlackboardValueType InferValueType(object value)
        {
            return value switch
            {
                bool => BehaviorTreeBlackboardValueType.Bool,
                int => BehaviorTreeBlackboardValueType.Int,
                float => BehaviorTreeBlackboardValueType.Float,
                _ => BehaviorTreeBlackboardValueType.String
            };
        }

        /// <summary>
        /// 获取指定类型的默认值。
        /// </summary>
        /// <param name="valueType">值类型。</param>
        /// <returns>对应的默认值。</returns>
        private static object GetDefaultValue(BehaviorTreeBlackboardValueType valueType)
        {
            return valueType switch
            {
                BehaviorTreeBlackboardValueType.Bool => false,
                BehaviorTreeBlackboardValueType.Int => 0,
                BehaviorTreeBlackboardValueType.Float => 0f,
                _ => string.Empty
            };
        }

        /// <summary>
        /// 尝试把对象转换为目标类型。
        /// </summary>
        /// <param name="sourceValue">原始值。</param>
        /// <param name="targetType">目标类型。</param>
        /// <param name="convertedValue">输出的转换结果。</param>
        /// <returns>是否转换成功。</returns>
        private static bool TryConvertValue(object sourceValue, Type targetType, out object convertedValue)
        {
            if (targetType == typeof(string))
            {
                convertedValue = sourceValue?.ToString() ?? string.Empty;
                return true;
            }

            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                if (TryConvertToBool(sourceValue, out bool boolValue))
                {
                    convertedValue = boolValue;
                    return true;
                }
            }

            if (targetType == typeof(int) || targetType == typeof(int?))
            {
                if (TryConvertToInt(sourceValue, out int intValue))
                {
                    convertedValue = intValue;
                    return true;
                }
            }

            if (targetType == typeof(float) || targetType == typeof(float?))
            {
                if (TryConvertToFloat(sourceValue, out float floatValue))
                {
                    convertedValue = floatValue;
                    return true;
                }
            }

            if (sourceValue != null && targetType.IsInstanceOfType(sourceValue))
            {
                convertedValue = sourceValue;
                return true;
            }

            if (targetType == typeof(double) || targetType == typeof(double?))
            {
                convertedValue = double.TryParse(
                    sourceValue?.ToString(),
                    NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture,
                    out double doubleValue)
                    ? doubleValue
                    : 0d;
                return true;
            }

            convertedValue = null;
            return false;
        }

        /// <summary>
        /// 尝试把对象转换为布尔值。
        /// </summary>
        /// <param name="sourceValue">原始值。</param>
        /// <param name="value">输出的布尔值。</param>
        /// <returns>是否转换成功。</returns>
        private static bool TryConvertToBool(object sourceValue, out bool value)
        {
            if (sourceValue is bool boolValue)
            {
                value = boolValue;
                return true;
            }

            if (bool.TryParse(sourceValue?.ToString(), out bool parsedBool))
            {
                value = parsedBool;
                return true;
            }

            if (int.TryParse(sourceValue?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedInt))
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
        /// <param name="sourceValue">原始值。</param>
        /// <param name="value">输出的整型值。</param>
        /// <returns>是否转换成功。</returns>
        private static bool TryConvertToInt(object sourceValue, out int value)
        {
            if (sourceValue is int intValue)
            {
                value = intValue;
                return true;
            }

            if (int.TryParse(sourceValue?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedInt))
            {
                value = parsedInt;
                return true;
            }

            if (float.TryParse(sourceValue?.ToString(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float parsedFloat))
            {
                value = (int)parsedFloat;
                return true;
            }

            value = 0;
            return false;
        }

        /// <summary>
        /// 尝试把对象转换为浮点值。
        /// </summary>
        /// <param name="sourceValue">原始值。</param>
        /// <param name="value">输出的浮点值。</param>
        /// <returns>是否转换成功。</returns>
        private static bool TryConvertToFloat(object sourceValue, out float value)
        {
            if (sourceValue is float floatValue)
            {
                value = floatValue;
                return true;
            }

            if (sourceValue is int intValue)
            {
                value = intValue;
                return true;
            }

            if (float.TryParse(sourceValue?.ToString(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float parsedFloat))
            {
                value = parsedFloat;
                return true;
            }

            value = 0f;
            return false;
        }

        /// <summary>
        /// 判断两个值是否可视为相等；浮点值会做容差比较。
        /// </summary>
        /// <param name="left">左值。</param>
        /// <param name="right">右值。</param>
        /// <returns>是否相等。</returns>
        private static bool ValuesEqual(object left, object right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            if (left is float leftFloat && right is float rightFloat)
            {
                return Math.Abs(leftFloat - rightFloat) <= 0.0001f;
            }

            return Equals(left, right);
        }
    }
}
