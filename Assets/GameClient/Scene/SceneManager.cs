using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Framework;
using Game.Resource;
using Game.Pool;

namespace Game.Scene
{
    /// <summary>
    /// 全局场景管理器
    /// 
    /// 职责：
    ///   1. 统筹场景生命周期，协调旧资源清理与新资源加载
    ///   2. 对外提供统一的场景切换接口
    ///   3. 广播场景切换进度回调给 UI 系统
    /// </summary>
    public class SceneManager
    {
        public static SceneManager Instance { get; private set; }

        private MonoBehaviour _coroutineHost;

        public string CurrentSceneName { get; private set; } = string.Empty;
        public bool IsLoading { get; private set; } = false;

        public void Initialize(MonoBehaviour host)
        {
            Instance = this;
            _coroutineHost = host;
            Debug.Log("[SceneManager] 初始化完成");
        }

        /// <summary>
        /// 切换主场景（Single 模式，自动清理旧资源）
        /// </summary>
        /// <param name="sceneName">目标场景名称或路径</param>
        /// <param name="showLoading">是否显示过场 Loading</param>
        public void ChangeScene(string sceneName, bool showLoading = true)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneManager] 当前正在加载场景中，忽略请求: {sceneName}");
                return;
            }

            _coroutineHost.StartCoroutine(ChangeSceneRoutine(sceneName, showLoading));
        }

        private IEnumerator ChangeSceneRoutine(string sceneName, bool showLoading)
        {
            IsLoading = true;
            Debug.Log($"[SceneManager] 开始切换场景: {CurrentSceneName} -> {sceneName}");

            // 1. 广播开始事件
            EventCenter.Publish(new SceneChangeBeginEvent
            {
                FromScene = CurrentSceneName,
                ToScene = sceneName,
                ShowLoading = showLoading
            });

            // 给 UI 面板弹出的时间留 1 帧缓冲
            yield return null;

            // 2. 清理旧数据缓存（清空对象池空闲对象）
            GlobalPoolManager.ClearAll();

            // 3. 开始异步加载新场景并监听进度
            bool isLoadSuccess = false;
            yield return ResourceManager.Instance.LoadSceneAsync(
                sceneName,
                onComplete: () => isLoadSuccess = true,
                onProgress: progress => EventCenter.Publish(new SceneLoadProgressEvent { Progress = progress }),
                isAdditive: false
            );

            if (!isLoadSuccess)
            {
                EventCenter.Publish(new SceneChangeEndEvent { SceneName = sceneName, Success = false });
                IsLoading = false;
                yield break;
            }

            // 4. 保证最后发出 1.0 的进度
            EventCenter.Publish(new SceneLoadProgressEvent { Progress = 1f });

            // 5. 记录并卸载旧资源
            CurrentSceneName = sceneName;
            ResourceManager.Instance.UnloadUnused();
            
            // 6. 广播完成事件
            IsLoading = false;
            EventCenter.Publish(new SceneChangeEndEvent { SceneName = sceneName, Success = true });
            Debug.Log($"[SceneManager] 场景切换完成: {sceneName}");
        }

        /// <summary>
        /// 异步叠加加载场景（通常用于常驻 UI 场景或副场景）
        /// </summary>
        public void LoadAdditiveScene(string sceneName)
        {
            _coroutineHost.StartCoroutine(LoadAdditiveSceneRoutine(sceneName));
        }

        private IEnumerator LoadAdditiveSceneRoutine(string sceneName)
        {
            Debug.Log($"[SceneManager] 开始叠加加载场景: {sceneName}");
            
            bool isLoadSuccess = false;
            yield return ResourceManager.Instance.LoadSceneAsync(
                sceneName,
                onComplete: () => isLoadSuccess = true,
                onProgress: null,
                isAdditive: true
            );
            
            if (isLoadSuccess)
            {
                Debug.Log($"[SceneManager] 叠加场景加载成功: {sceneName}");
            }
        }
        
        /// <summary>
        /// 关闭管理器
        /// </summary>
        public void Shutdown()
        {
            Instance = null;
        }
    }
}
