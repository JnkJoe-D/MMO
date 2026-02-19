using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SkillEditor;
using UnityEditor;
using UnityEngine;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 轨道注册表：负责扫描和缓存带有 [TrackDefinition] 的轨道类型
    /// </summary>
    public static class TrackRegistry
    {
        public class TrackInfo
        {
            public Type TrackType;
            public TrackDefinitionAttribute Attribute;
        }

        private static List<TrackInfo> registeredTracks;

        /// <summary>
        /// 获取所有已注册的轨道信息
        /// </summary>
        public static List<TrackInfo> GetRegisteredTracks()
        {
            if (registeredTracks == null)
            {
                Initialize();
            }
            return registeredTracks;
        }

        private static void Initialize()
        {
            registeredTracks = new List<TrackInfo>();

            // 扫描所有程序集
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                // 简单的过滤，提升性能
                var asmName = asm.GetName().Name;
                if (asmName.StartsWith("System") || asmName.StartsWith("Unity") || 
                    asmName.StartsWith("mscorlib") || asmName.StartsWith("Mono"))
                    continue;

                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract || !type.IsSubclassOf(typeof(TrackBase)))
                        continue;

                    var attr = type.GetCustomAttribute<TrackDefinitionAttribute>();
                    if (attr != null)
                    {
                        registeredTracks.Add(new TrackInfo
                        {
                            TrackType = type,
                            Attribute = attr
                        });
                    }
                }
            }

            // 根据 Order 排序
            registeredTracks.Sort((a, b) => a.Attribute.Order.CompareTo(b.Attribute.Order));
        }

        /// <summary>
        /// 创建指定类型的轨道实例
        /// </summary>
        public static TrackBase CreateTrack(Type trackType)
        {
            if (trackType == null || !typeof(TrackBase).IsAssignableFrom(trackType))
                return null;

            return (TrackBase)Activator.CreateInstance(trackType);
        }

        /// <summary>
        /// 根据轨道类型名称获取图标
        /// </summary>
        public static string GetTrackIcon(string trackTypeName)
        {
            if (registeredTracks == null) Initialize();

            foreach (var info in registeredTracks)
            {
                if (info.TrackType.Name == trackTypeName)
                {
                    return info.Attribute.Icon;
                }
            }
            return "ScriptableObject Icon"; // 默认图标
        }
        /// <summary>
        /// 根据轨道类型获取关联的片段类型
        /// </summary>
        public static Type GetClipType(Type trackType)
        {
            if (registeredTracks == null) Initialize();

            foreach (var info in registeredTracks)
            {
                if (info.TrackType == trackType)
                {
                    return info.Attribute.ClipType;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据轨道类型名称获取颜色
        /// </summary>
        public static Color GetTrackColor(string trackTypeName)
        {
            if (registeredTracks == null) Initialize();

            foreach (var info in registeredTracks)
            {
                if (info.TrackType.Name == trackTypeName)
                {
                    Color color;
                    if (ColorUtility.TryParseHtmlString(info.Attribute.ColorHex, out color))
                    {
                        return color;
                    }
                }
            }
            return Color.gray;
        }

        /// <summary>
        /// 根据片段类型获取对应的轨道类型名称
        /// </summary>
        public static string GetTrackTypeByClipType(Type clipType)
        {
            if (registeredTracks == null) Initialize();

            foreach (var info in registeredTracks)
            {
                if (info.Attribute.ClipType == clipType)
                {
                    return info.TrackType.Name;
                }
            }
            return null;
        }
    }
}