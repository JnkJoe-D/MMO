using System.Collections;
using UnityEngine;
using Game.Pool;
using Game.Resource;
using Game.Network;
using Game.Scene;
using Game.UI;
using Game.Config;


namespace Game.Framework
{
    /// <summary>
    /// 游戏全局入口与生命周期管理器
    ///
    /// 职责：
    ///   1. 按正确顺序初始化所有全局子系统
    ///   2. 统一驱动各子系统的 Update / Shutdown
    ///   3. DontDestroyOnLoad，贯穿整个应用生命周期
    ///
    /// 初始化顺序（关键，不可随意调整）：
    ///   Asset → Config → Pool → Lua → Net → Audio → UI → Scene
    ///
    /// 使用方式：
    ///   将此脚本挂在场景中名为 "[GameRoot]" 的 GameObject 上
    ///   或通过 Resources.Load 动态创建（推荐热更接入后改为此方式）
    /// </summary>
    public class GameRoot : MonoBehaviour
    {
        // ── 单例（仅限框架内部访问，业务层通过子系统接口访问）
        private static GameRoot _instance;
        public static bool IsInitialized { get; private set; }

        // ── 子系统引用 ─────────────────────────
        private ResourceManager _resourceManager;
        private NetworkManager  _networkManager;
        private SceneManager    _sceneManager;
        private UIManager       _uiManager;
        // private LuaManager _luaManager;
        // private AudioManager _audioManager;

        [Header("资源管理配置")]
        [SerializeField] private ResourceConfig _resourceConfig;

        [Header("网络配置")]
        [SerializeField] private string _serverHost = "8.134.90.3";
        [SerializeField] private int    _tcpPort    = 33333;
        [SerializeField] private int    _udpPort    = 33334;

        // ────────────────────────────────────────
        // Unity 生命周期
        // ────────────────────────────────────────

        private void Awake()
        {
            // 单例保护
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            StartCoroutine(InitializeSequence());
        }

        private void Update()
        {
            if (!IsInitialized) return;

            // 驱动延迟事件队列（每帧刷新一次）
            EventCenter.FlushPending();

            // 驱动网络管理器（主线程消息分发、心跳、重连）
            _networkManager?.Update();

            // TODO: 驱动其他子系统
            // _luaManager?.Update();
        }

        private void OnApplicationQuit()
        {
            Shutdown();
        }

        // ────────────────────────────────────────
        // 初始化流水线
        // ────────────────────────────────────────

        /// <summary>
        /// 异步初始化序列
        /// 使用协程保证顺序执行，并支持未来接入进度条 UI
        /// </summary>
        private IEnumerator InitializeSequence()
        {
            Debug.Log("[GameRoot] ===== 游戏启动 =====");

            // ── Step 1: 资源管理器（YooAsset）────────────────
            _resourceManager = new ResourceManager();
            yield return StartCoroutine(
                _resourceManager.InitializeAsync(_resourceConfig, this)
            );
            if (!_resourceManager.IsInitialized)
            {
                Debug.LogError("[GameRoot] 资源管理器初始化失败，游戏终止");
                yield break;
            }
            Debug.Log("[GameRoot] [1/9] Assets ... OK");
            yield return null;

            // ── Step 2: 基础配置加载 ──────────────────
            var configMgrTask = ConfigManager.Instance.InitializeAsync();
            while (!configMgrTask.IsCompleted) yield return null;
            Debug.Log("[GameRoot] [2/9] Config ... OK");
            yield return null;

            // ── Step 3: 全局对象池 ────────────────────
            InitPool();
            Debug.Log("[GameRoot] [3/9] Pool ... OK");
            yield return null;

            // ── Step 4: Lua 虚拟机 ────────────────────
            // TODO: 启动 XLua 环境，加载 main.lua
            // _luaManager = new LuaManager();
            // _luaManager.Init();
            Debug.Log("[GameRoot] [4/7] Lua ... (TODO: XLua)");
            yield return null;

            // ── Step 5: 网络管理器 ────────────────────
            _networkManager = new NetworkManager();
            _networkManager.Initialize(_serverHost, _tcpPort, _udpPort);
            Debug.Log("[GameRoot] [5/7] Network ... OK");
            yield return null;

            // ── Step 6: 音频管理器 ────────────────────
            // TODO: 初始化 AudioManager，预加载 BGM/SFX
            // _audioManager = new AudioManager();
            // _audioManager.Init();
            Debug.Log("[GameRoot] [6/7] Audio ... (TODO: AudioManager)");
            yield return null;

            // ── Step 7: UI 管理器 ─────────────────────
            _uiManager = new UIManager();
            _uiManager.Initialize(this);
            Debug.Log("[GameRoot] [7/9] UI ... OK");
            yield return null;

            // ── Step 8: 场景管理器 ────────────────────
            _sceneManager = new SceneManager();
            _sceneManager.Initialize(this);
            Debug.Log("[GameRoot] [8/9] Scene ... OK");
            yield return null;

            // ── 完成 ──────────────────────────────────
            IsInitialized = true;
            Debug.Log("[GameRoot] ===== 初始化完成 =====");

            // 发布初始化完成事件，各系统可以订阅此事件做后置操作
            EventCenter.Publish(new GameInitializedEvent());
        }

        // ────────────────────────────────────────
        // 子系统初始化函数
        // ────────────────────────────────────────

        private void InitPool()
        {
            GlobalPoolManager.Initialize();
        }

        // ────────────────────────────────────────
        // 关闭流程
        // ────────────────────────────────────────

        private void Shutdown()
        {
            Debug.Log("[GameRoot] ===== 游戏关闭 =====");

            IsInitialized = false;

            // 各子系统 Shutdown（顺序与初始化相反）
            // _uiManager?.Shutdown();
            _networkManager?.Shutdown();
            // _luaManager?.Dispose();

            _uiManager?.Shutdown();
            _sceneManager?.Shutdown();
            _resourceManager?.Shutdown();
            EventCenter.ClearAll();
            GlobalPoolManager.DisposeAll();

            Debug.Log("[GameRoot] ===== 关闭完成 =====");
        }

        // ────────────────────────────────────────
        // 场景切换钩子（供 SceneManager 调用）
        // ────────────────────────────────────────

        /// <summary>
        /// 场景切换前调用：清理当前场景的事件订阅和对象池缓存
        /// TODO: 由 SceneManager 在切换前调用
        /// </summary>
        public static void OnSceneUnload()
        {
            // 只清理对象池缓存（空闲对象），不清理订阅
            GlobalPoolManager.ClearAll();
            // 卸载场景结束后未使用的资源
            ResourceManager.Instance?.UnloadUnused();
        }
    }

    // ────────────────────────────────────────────────────────────
    // 框架内置事件（放在同文件，避免碎片化文件）
    // ────────────────────────────────────────────────────────────

    /// <summary>
    /// 游戏初始化完成事件
    /// 所有子系统初始化完毕后由 GameRoot 发布
    /// </summary>
    public struct GameInitializedEvent : IGameEvent { }
}
