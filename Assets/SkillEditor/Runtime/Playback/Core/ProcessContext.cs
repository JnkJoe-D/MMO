using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor
{
    /// <summary>
    /// 播放上下文，为 Process 提供依赖注入：
    /// - 目标角色的 GameObject / Transform
    /// - 组件惰性缓存
    /// - 系统级清理注册（同 key 去重）
    /// </summary>
    public class ProcessContext
    {
        /// <summary>
        /// 目标角色
        /// </summary>
        public GameObject Owner { get; private set; }

        /// <summary>
        /// 目标角色的 Transform
        /// </summary>
        public Transform OwnerTransform { get; private set; }

        /// <summary>
        /// 当前播放模式
        /// </summary>
        public PlayMode PlayMode { get; private set; }

        /// <summary>
        /// 可选扩展数据（外部注入业务相关对象）
        /// </summary>
        public object UserData { get; set; }
        public float GlobalPlaySpeed { get; set; } = 1f; // 全局播放速度控制

        // 组件缓存
        private Dictionary<Type, Component> componentCache = new Dictionary<Type, Component>();

        // 系统级清理注册（同 key 去重）
        private Dictionary<string, Action> cleanupActions = new Dictionary<string, Action>();

        public ProcessContext(GameObject owner, PlayMode playMode)
        {
            Owner = owner;
            OwnerTransform = owner != null ? owner.transform : null;
            PlayMode = playMode;
        }

        /// <summary>
        /// 获取组件（惰性查找 + 缓存）
        /// </summary>
        public T GetComponent<T>() where T : Component
        {
            var type = typeof(T);
            if (!componentCache.TryGetValue(type, out var comp))
            {
                if (Owner != null)
                {
                    comp = Owner.GetComponentInChildren<T>();
                    if (comp != null)
                    {
                        componentCache[type] = comp;
                    }
                }
            }
            return (T)comp;
        }

        /// <summary>
        /// 注册系统级清理操作（同 key 去重，后注册覆盖前注册）
        /// Process 在 OnEnable 中调用，Runner 结束时统一执行
        /// </summary>
        /// <param name="key">清理标识（如 "AnimComponent"），同类 Process 注册相同 key</param>
        /// <param name="cleanup">清理回调</param>
        public void RegisterCleanup(string key, Action cleanup)
        {
            cleanupActions[key] = cleanup;
        }

        /// <summary>
        /// 执行所有注册的系统级清理（Runner 结束时调用）
        /// </summary>
        internal void ExecuteCleanups()
        {
            foreach (var action in cleanupActions.Values)
            {
                action?.Invoke();
            }
            cleanupActions.Clear();
        }

        /// <summary>
        /// 清空组件缓存和清理注册
        /// </summary>
        internal void Clear()
        {
            componentCache.Clear();
            cleanupActions.Clear();
        }
    }
}
