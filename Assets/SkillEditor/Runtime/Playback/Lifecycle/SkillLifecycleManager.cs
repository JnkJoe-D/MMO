using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor
{
    /// <summary>
    /// Mono 单例：仅用于非帧同步运行时，在 Update 中驱动所有注册的 SkillRunner
    /// 帧同步模式不经过此管理器，由外部框架直接调用 Runner.Tick()
    /// </summary>
    public class SkillLifecycleManager : MonoBehaviour
    {
        private static SkillLifecycleManager instance;

        public static SkillLifecycleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("[SkillLifecycleManager]");
                    instance = go.AddComponent<SkillLifecycleManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private List<SkillRunner> activeRunners = new List<SkillRunner>();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            // 倒序遍历，允许 Runner 在 Tick 中自行注销
            for (int i = activeRunners.Count - 1; i >= 0; i--)
            {
                activeRunners[i].Tick(dt);
            }
        }

        /// <summary>
        /// 注册 Runner（开始接受 Update 驱动）
        /// </summary>
        public void Register(SkillRunner runner)
        {
            if (runner != null && !activeRunners.Contains(runner))
            {
                activeRunners.Add(runner);
            }
        }

        /// <summary>
        /// 注销 Runner
        /// </summary>
        public void Unregister(SkillRunner runner)
        {
            activeRunners.Remove(runner);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
