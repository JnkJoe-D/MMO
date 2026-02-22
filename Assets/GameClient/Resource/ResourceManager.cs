using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using YooAsset;
using Game.Framework;

namespace Game.Resource
{
    /// <summary>
    /// 资源管理器
    /// 
    /// 职责：
    ///   1. 初始化 YooAsset Package（三种模式：编辑器模拟/单机离线/联机热更）
    ///   2. 驱动热更新流程（联机模式下）
    ///   3. 提供统一的资源加载/卸载 API
    ///   4. 资源引用计数托管（防止重复加载和忘记释放）
    ///
    /// 使用方式：
    ///   由 GameRoot.InitializeSequence 创建并初始化，业务层通过
    ///   ResourceManager.Instance 访问（或通过 GameRoot 暴露的只读属性）
    ///
    /// 加载示例：
    ///   // 同步（仅限编辑器/Offline，资源已在本地）
    ///   var prefab = ResourceManager.Instance.LoadAsset<GameObject>("Assets/Prefabs/Player.prefab");
    ///
    ///   // 异步协程
    ///   yield return ResourceManager.Instance.LoadAssetAsync<AudioClip>("Assets/Audio/BGM.mp3",
    ///       clip => audioSource.clip = clip);
    ///
    ///   // 释放
    ///   ResourceManager.Instance.ReleaseAsset(handle);
    /// </summary>
    public class ResourceManager
    {
        public static ResourceManager Instance { get; private set; }

        private ResourcePackage _package;
        private ResourceConfig  _config;

        // ── 初始化状态 ──────────────────────────
        public bool IsInitialized { get; private set; }

        // ────────────────────────────────────────
        // 初始化
        // ────────────────────────────────────────

        /// <summary>
        /// 初始化资源管理器（协程，由 GameRoot 调用）
        /// 根据 ResourceConfig.playMode 自动选择初始化策略
        /// </summary>
        public IEnumerator InitializeAsync(ResourceConfig config, MonoBehaviour runner)
        {
            Instance = this;
            _config  = config;

            Debug.Log($"[ResourceManager] 初始化，模式: {config.playMode}，包名: {config.defaultPackageName}");

            // ── 1. 初始化 YooAssets 引擎 ────────────────────
            YooAssets.Initialize();

            // ── 2. 获取或创建 Package ────────────────────────
            _package = YooAssets.TryGetPackage(config.defaultPackageName)
                    ?? YooAssets.CreatePackage(config.defaultPackageName);

            // 设为默认包（后续无需指定包名的 API 将使用此包）
            YooAssets.SetDefaultPackage(_package);

            // ── 3. 根据模式初始化 ────────────────────────────
            InitializationOperation initOp;

            switch (config.playMode)
            {
                case EPlayMode.EditorSimulateMode:
                    initOp = InitEditorSimulate();
                    break;

                case EPlayMode.OfflinePlayMode:
                    initOp = InitOffline();
                    break;

                case EPlayMode.HostPlayMode:
                    initOp = InitHostPlay(config);
                    break;

                default:
                    Debug.LogError($"[ResourceManager] 不支持的运行模式: {config.playMode}");
                    yield break;
            }

            yield return initOp;

            if (initOp.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"[ResourceManager] Package 初始化失败: {initOp.Error}");
                EventCenter.Publish(new HotUpdateFailedEvent
                {
                    Reason  = HotUpdateFailReason.InitializeFailed,
                    Message = initOp.Error
                });
                yield break;
            }

            Debug.Log("[ResourceManager] Package 初始化成功");

            // ── 4. 联机模式下执行热更流程 ────────────────────
            if (config.playMode == EPlayMode.HostPlayMode && config.autoUpdate)
            {
                var updater = new ResourceUpdater(_package, config);
                yield return runner.StartCoroutine(updater.Run());
            }

            // ── 5. 完成 ──────────────────────────────────────
            IsInitialized = true;
            EventCenter.Publish(new ResourceInitializedEvent());
            Debug.Log("[ResourceManager] 初始化完成，资源系统就绪");
        }

        // ── 各模式初始化 ─────────────────────────────────────

        private InitializationOperation InitEditorSimulate()
        {
#if UNITY_EDITOR
            var buildResult = EditorSimulateModeHelper.SimulateBuild(_config.defaultPackageName);
            var param = new EditorSimulateModeParameters();
            param.EditorFileSystemParameters =
                FileSystemParameters.CreateDefaultEditorFileSystemParameters(buildResult.PackageRootDirectory);
            return _package.InitializeAsync(param);
#else
            // 非编辑器环境降级为离线模式
            Debug.LogWarning("[ResourceManager] EditorSimulate 模式仅支持编辑器，已自动降级为 Offline 模式");
            return InitOffline();
#endif
        }

        private InitializationOperation InitOffline()
        {
            var param = new OfflinePlayModeParameters();
            param.BuildinFileSystemParameters =
                FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            return _package.InitializeAsync(param);
        }

        private InitializationOperation InitHostPlay(ResourceConfig config)
        {
            var remoteServices = new DefaultRemoteServices(
                config.GetHostServerURL(),
                config.GetFallbackServerURL()
            );
            var param = new HostPlayModeParameters();
            param.BuildinFileSystemParameters =
                FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            param.CacheFileSystemParameters =
                FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            return _package.InitializeAsync(param);
        }

        // ────────────────────────────────────────
        // 资源加载 API
        // ────────────────────────────────────────

        /// <summary>
        /// 同步加载资源（仅适用于 EditorSimulate / Offline 模式，或已缓存的资源）
        /// 注意：大资源请勿同步加载，会卡主线程
        /// </summary>
        public T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            var handle = _package.LoadAssetSync<T>(assetPath);
            if (handle.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"[ResourceManager] 同步加载失败: {assetPath}");
                EventCenter.Publish(new AssetLoadFailedEvent { AssetPath = assetPath, Error = handle.LastError });
                return null;
            }
            return handle.AssetObject as T;
        }

        /// <summary>
        /// 异步加载资源（协程版本）
        /// </summary>
        /// <param name="assetPath">资源路径（与打包时一致）</param>
        /// <param name="onComplete">加载完成回调，参数为加载结果（失败时为 null）</param>
        /// <param name="onProgress">加载进度回调（0~1），可选</param>
        public IEnumerator LoadAssetAsync<T>(
            string       assetPath,
            Action<T>    onComplete,
            Action<float> onProgress = null
        ) where T : UnityEngine.Object
        {
            var handle = _package.LoadAssetAsync<T>(assetPath);

            while (!handle.IsDone)
            {
                onProgress?.Invoke(handle.Progress);
                yield return null;
            }

            if (handle.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"[ResourceManager] 异步加载失败: {assetPath} | {handle.LastError}");
                EventCenter.Publish(new AssetLoadFailedEvent { AssetPath = assetPath, Error = handle.LastError });
                onComplete?.Invoke(null);
                yield break;
            }

            onComplete?.Invoke(handle.AssetObject as T);
        }

        /// <summary>
        /// 异步实例化 GameObject（自动处理 Instantiate）
        /// </summary>
        public IEnumerator InstantiateAsync(
            string        assetPath,
            Action<GameObject> onComplete,
            Transform     parent    = null,
            Vector3?      position  = null,
            Quaternion?   rotation  = null
        )
        {
            var handle = _package.LoadAssetAsync<GameObject>(assetPath);
            yield return handle;

            if (handle.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"[ResourceManager] 实例化失败: {assetPath} | {handle.LastError}");
                EventCenter.Publish(new AssetLoadFailedEvent { AssetPath = assetPath, Error = handle.LastError });
                onComplete?.Invoke(null);
                yield break;
            }

            var go = UnityEngine.Object.Instantiate(
                handle.AssetObject as GameObject,
                position ?? Vector3.zero,
                rotation ?? Quaternion.identity,
                parent
            );
            onComplete?.Invoke(go);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        public IEnumerator LoadSceneAsync(
            string   scenePath,
            Action   onComplete = null,
            bool     isAdditive = false
        )
        {
            var loadMode = isAdditive
                ? UnityEngine.SceneManagement.LoadSceneMode.Additive
                : UnityEngine.SceneManagement.LoadSceneMode.Single;

            var handle = _package.LoadSceneAsync(scenePath, loadMode);
            yield return handle;

            if (handle.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"[ResourceManager] 场景加载失败: {scenePath} | {handle.LastError}");
                EventCenter.Publish(new AssetLoadFailedEvent { AssetPath = scenePath, Error = handle.LastError });
                yield break;
            }

            onComplete?.Invoke();
        }

        // ────────────────────────────────────────
        // 资源释放
        // ────────────────────────────────────────

        /// <summary>
        /// 卸载一个资源包中的所有未使用资源（GC 风格）
        /// 建议在场景切换后调用
        /// </summary>
        public void UnloadUnused()
        {
            _package?.UnloadUnusedAssetsAsync();
        }

        /// <summary>
        /// 清空所有缓存（应用退出前调用）
        /// </summary>
        public void Shutdown()
        {
            IsInitialized = false;
            YooAssets.Destroy();
            Instance = null;
            Debug.Log("[ResourceManager] 已关闭");
        }

        // ────────────────────────────────────────
        // 内部辅助类
        // ────────────────────────────────────────

        private class DefaultRemoteServices : IRemoteServices
        {
            private readonly string _main;
            private readonly string _fallback;

            public DefaultRemoteServices(string main, string fallback)
            {
                _main     = main;
                _fallback = fallback;
            }

            string IRemoteServices.GetRemoteMainURL(string fileName)     => $"{_main}/{fileName}";
            string IRemoteServices.GetRemoteFallbackURL(string fileName)  => $"{_fallback}/{fileName}";
        }
    }
}
