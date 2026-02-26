using UnityEngine;

namespace Game.Logic.Player.Config
{
    /// <summary>
    /// 全局动画配置的单例管理访问器
    /// 它负责在游戏启动时把配置 SO 从 Resources（或 Addressable）拉起常驻内存
    /// </summary>
    public class AnimationConfigManager
    {
        public static AnimationConfigManager Instance { get; private set; }

        public GlobalAnimationConfig ConfigData { get; private set; }

        public void Initialize()
        {
            Instance = this;

            // TODO: 未来换 Addressable 异步加载
            // 目前先为了跑通管线，采用快捷的 Resources 兜底法
            // 需要策划把那张 GlobalAnimationConfig.asset 丢进 Assets/Resources 文件夹里
            ConfigData = Resources.Load<GlobalAnimationConfig>("GlobalAnimationConfig");

            if (ConfigData != null)
            {
                // 建哈希内存池 O(1)
                ConfigData.InitializeCache();
                Debug.Log("[AnimationConfigManager] 全局动画动作库加载并预热完毕。");
            }
            else
            {
                Debug.LogWarning("[AnimationConfigManager] 未能在 Resources 根目录找到名叫 'GlobalAnimationConfig' 的配置资源，基础动画将无法下发！");
            }
        }

        public void Shutdown()
        {
            ConfigData = null;
            Instance = null;
            Debug.Log("[AnimationConfigManager] 已关闭");
        }

        /// <summary>
        /// 供 Entity 获取它的那一套连招跑跳字典
        /// </summary>
        public AnimSetEntry AcquireSet(int roleId, int weaponType)
        {
            if (ConfigData == null) return null;
            return ConfigData.GetAnimSet(roleId, weaponType);
        }
    }
}
