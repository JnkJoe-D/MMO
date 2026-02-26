using System.Collections;
using UnityEngine;
using Game.Pool;
using Game.Resource;
using Game.Network;
using Game.Scene;
using Game.UI;
using Game.Config;
using Game.FSM;


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
        private FSMManager      _fsmManager;
        private Game.Input.InputManager _inputManager;
        private Game.Camera.GameCameraManager _cameraManager;
        // private LuaManager _luaManager;
        // private AudioManager _audioManager;

        [Header("资源管理配置")]
        [SerializeField] private ResourceConfig _resourceConfig;

        [Header("网络配置")]
        [SerializeField] private string _serverHost = "127.0.0.1";
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
            _cameraManager?.Update(Time.deltaTime);

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

            // ── Step 1: 全局对象池 ────────────────────
            InitPool();
            Debug.Log("[GameRoot] [1/9] Pool ... OK");
            yield return null;

            // ── Step 2: 核心底层服务启机 ─────────────────────
            _fsmManager = gameObject.GetComponent<FSMManager>();
            if (_fsmManager == null)
            {
                _fsmManager = gameObject.AddComponent<FSMManager>();
            }
            _fsmManager.Initialize();
            Debug.Log("[GameRoot] [2/11] FSM ... OK");

            _networkManager = new NetworkManager();
            _networkManager.Initialize(_serverHost, _tcpPort, _udpPort);
            Debug.Log("[GameRoot] [3/11] Network (Prepared) ... OK");

            _uiManager = new UIManager();
            _uiManager.Initialize(this);
            Debug.Log("[GameRoot] [4/11] UI ... OK");
            yield return null;

            // ── Step 3: 唤起热更新界面以接收事件 ────────
            _uiManager.Open<Game.UI.Modules.HotUpdate.HotUpdateModule>();  //Resource加载
            yield return null;

            // ── Step 4: 资源管理器（YooAsset）────────────────
            _resourceManager = new ResourceManager();
            yield return StartCoroutine(
                _resourceManager.InitializeAsync(_resourceConfig, this)
            );
            if (!_resourceManager.IsInitialized)
            {
                Debug.LogError("[GameRoot] 资源管理器初始化失败，游戏终止");
                yield break;
            }
            Debug.Log("[GameRoot] [4/9] Assets ... OK");
            yield return null;

            // ── Step 5: 基础配置加载 ──────────────────
            var configMgrTask = ConfigManager.Instance.InitializeAsync();
             while (!configMgrTask.IsCompleted) yield return null;
            Debug.Log("[GameRoot] [5/9] Config ... OK");
            yield return null;

            // ── Step 6: Lua 音频等 ───────────────
            Debug.Log("[GameRoot] [6/9] Lua ... (TODO: XLua)");
            
            // ── Step 7: 保留位置 ───────────────
            Debug.Log("[GameRoot] [7/9] System Placeholder ... OK");

            // ── Step 8: 场景管理器 ────────────────────
            _sceneManager = new SceneManager();
            _sceneManager.Initialize(this);
            Debug.Log("[GameRoot] [8/10] Scene ... OK");
            
            
            // ── Step 9: 输入管理器 ────────────────────
            _inputManager = new Game.Input.InputManager();
            _inputManager.Initialize();
            Debug.Log("[GameRoot] [9/11] Input ... OK");

            // ── Step 10: 相机管理器 ───────────────────
            _cameraManager = new Game.Camera.GameCameraManager(); // Retained assignment to field
            _cameraManager.Initialize();

            // ── Step 11: 全局动画库 ───────────────────
            var animConfigManager = new Game.Logic.Player.Config.AnimationConfigManager();
            animConfigManager.Initialize();
            Debug.Log("[GameRoot] [11/11] Animation Configs ... OK");

            // 发布初始化完成事件，各系统可以订阅此事件做后置操作
            EventCenter.Publish(new GameInitializedEvent());

            // ── Step 12: 发起网络握手与切入登录流 ──────────────────────────────────
            Debug.Log("[GameRoot] [12/12] 流水线执行完毕，通知界面切换至连网状态...");
            EventCenter.Publish(new GameLoginStageStartEvent());
            
            // 为了让玩家看清前面的文字动画稍微驻留 0.5s，也可以不加，但这里加个缓冲让画面不切太抖
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log("[GameRoot] 发起基于 TCP 的主服务连接...");
            _networkManager.ConnectTcp();
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
            Game.Logic.Player.Config.AnimationConfigManager.Instance?.Shutdown();
            _inputManager?.Shutdown();
            _cameraManager?.Shutdown();
            _sceneManager?.Shutdown();
            _fsmManager?.Shutdown();
            
            // _luaManager?.Dispose();
            _uiManager?.Shutdown();
            _resourceManager?.Shutdown();
            _networkManager?.Shutdown();
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
    
    /// <summary>
    /// 游戏即将进入登录流程大阶段
    /// 这代表前置资源完全就绪，允许热更面板或其他 UI 转场以显示“连接服务器中...”
    /// </summary>
    public struct GameLoginStageStartEvent : IGameEvent { }
}
