using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic.Player.Config
{
    /// <summary>
    /// 标准的扁平化动画组条目
    /// 被用来存放在唯一的大库中供组合推算
    /// </summary>
    [Serializable]
    public class AnimSetEntry
    {
        [Tooltip("适用于哪个角色模型 (例如: 1001=主角男, 1002=萝莉)")]
        public int RoleID;

        [Tooltip("手持什么武器类型的特化 (例如: 0=通用/空手, 1=重剑, 2=双枪)")]
        public int WeaponType;

        [Header("基础移动表现与 Locomotion 覆盖")]
        public AnimUnitConfig Idle;
        public AnimUnitConfig JogStart;
        public AnimUnitConfig Jog;
        public AnimUnitConfig JogStop;
        public AnimUnitConfig DashStart;
        public AnimUnitConfig Dash;
        public AnimUnitConfig DashStop;
        public AnimUnitConfig DodgeFront;
        public AnimUnitConfig DodgeBack;
        public AnimUnitConfig JumpStart;
        public AnimUnitConfig FallLoop;
        public AnimUnitConfig Land;

        [Header("控制手感与硬直配置 (秒)")]
        [Tooltip("触发跑停动作时，禁止角色推摇杆挪动的硬直时间")]
        [Range(0,1f)]
        public float JogStopLockTime = 0.25f;
        [Tooltip("触发冲刺急停动作时，禁止角色推摇杆挪动的长硬直时间")]
        [Range(0, 1f)]
        public float DashStopLockTime = 0.5f;
    }

    /// <summary>
    /// 全局动画资产配置表库 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "GlobalAnimationConfig", menuName = "Config/AnimationSet Library")]
    public class GlobalAnimationConfig : ScriptableObject
    {
        // === 面向策划的编辑树 - 最坚固原生序列化的铺平 List ===
        [Tooltip("请将所有的角色待机、跑步拆散组合配置在这里")]
        public List<AnimSetEntry> Entries = new List<AnimSetEntry>();

        // === 运行时高速内存 ===
        // 游戏启动时生成的 HashMap，不参与序列化
        private Dictionary<(int role, int weapon), AnimSetEntry> _cache;

        /// <summary>
        /// (由单例管线调用) 预热装载：O(N) 构建全部字典，换取运行时的 O(1) 效率
        /// </summary>
        public void InitializeCache()
        {
            _cache = new Dictionary<(int role, int weapon), AnimSetEntry>();
            if (Entries == null) return;

            foreach (var entry in Entries)
            {
                var key = (entry.RoleID, entry.WeaponType);
                if (!_cache.ContainsKey(key))
                {
                    _cache.Add(key, entry);
                }
                else
                {
                    Debug.LogWarning($"[GlobalAnimationConfig] 发现重复配置: Role={entry.RoleID}, Weapon={entry.WeaponType}！新条目已被抛弃。");
                }
            }
        }

        /// <summary>
        /// O(1) 多维安全提款，自带完美降级 Fallback。
        /// </summary>
        /// <param name="roleId">角色身体编号</param>
        /// <param name="weaponType">当下手持的武器模式</param>
        public AnimSetEntry GetAnimSet(int roleId, int weaponType)
        {
            if (_cache == null)
            {
                Debug.LogError("[GlobalAnimationConfig] 初始化未完成就被索要。将强制进行初始化。");
                InitializeCache();
            }

            // 1. 最高优先级：【对应角色 + 具体武器特化姿图】(例如：但丁拿巨剑的跑)
            if (_cache.TryGetValue((roleId, weaponType), out AnimSetEntry specificEntry))
            {
                return specificEntry;
            }

            // 2. Fallback 降级：【对应角色 + 无武器通用兜底】(例如：但丁空手跑)
            if (weaponType != 0 && _cache.TryGetValue((roleId, 0), out AnimSetEntry fallbackEntry))
            {
                // （可选：可以注释掉以免日志太长发水）
                Debug.Log($"[GlobalAnimationConfig] 查找 Role({roleId}),Weapon({weaponType}) 失败，安全退化至 Weapon=0 的默认基础套。");
                return fallbackEntry;
            }

            // 3. 彻底失败
            Debug.LogError($"[GlobalAnimationConfig] 彻底找不到基础动画配置！连兜底通用包 Role={roleId}, Weapon=0 都不存在，角色表现将发生崩溃。");
            return null;
        }
    }
}
