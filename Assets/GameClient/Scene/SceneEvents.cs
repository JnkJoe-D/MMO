using Game.Framework;

namespace Game.Scene
{
    // ============================================================
    // 场景加载参数
    // ============================================================

    /// <summary>
    /// 混合场景加载参数包
    /// </summary>
    public class SceneTransitionParams
    {
        /// <summary>目标场景文件路径</summary>
        public string SceneName { get; set; }
        
        /// <summary>场景文件中不包含、但进入场景必须预先加载好的强依赖资源列表（如玩家模型、主界面的常驻UI等）</summary>
        public System.Collections.Generic.List<string> RequiredAssets { get; set; } = new();

        /// <summary>是否显示加载进度界面</summary>
        public bool ShowLoading { get; set; } = true;

        /// <summary>向新场景传递的自定义参数（如出生点坐标、副本难度等）</summary>
        public object CustomData { get; set; }
    }

    // ============================================================
    // 场景流转相关事件
    // ============================================================

    /// <summary>
    /// 场景切换开始事件
    /// UI 系统监听此事件以弹起 Loading 界面并准备显示进度
    /// </summary>
    public struct SceneChangeBeginEvent : IGameEvent
    {
        public SceneTransitionParams TransitionParams;
    }

    /// <summary>
    /// 场景加载进度事件
    /// 每帧广播加载进度（0~1），驱动 Loading 进度条动画
    /// </summary>
    public struct SceneLoadProgressEvent : IGameEvent
    {
        public float Progress;
        public string LoadingText;
    }

    /// <summary>
    /// 场景切换完成事件
    /// UI 系统监听此事件以关闭 Loading 界面，业务逻辑层启动新场景逻辑
    /// </summary>
    public struct SceneChangeEndEvent : IGameEvent
    {
        public string SceneName;
        public bool   Success;
    }
}
