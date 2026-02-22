using Game.Framework;
using UnityEngine;
using YooAsset;

namespace Game.Resource
{
    // ============================================================
    // 资源管理相关事件
    // ============================================================

    /// <summary>
    /// 资源管理器初始化完成事件（本地包已就绪，可以开始加载资源）
    /// </summary>
    public struct ResourceInitializedEvent : IGameEvent { }

    /// <summary>
    /// 热更检查开始事件
    /// </summary>
    public struct HotUpdateCheckStartEvent : IGameEvent { }

    /// <summary>
    /// 发现需要下载的更新文件
    /// </summary>
    public struct HotUpdateFoundEvent : IGameEvent
    {
        /// <summary>待下载文件数量</summary>
        public int FileCount;
        /// <summary>待下载总字节数</summary>
        public long TotalBytes;
    }

    /// <summary>
    /// 热更下载进度事件（每帧由 Downloader 回调触发）
    /// </summary>
    public struct HotUpdateProgressEvent : IGameEvent
    {
        public int TotalDownloadCount;
        public int CurrentDownloadCount;
        public long TotalDownloadBytes;
        public long CurrentDownloadBytes;

        /// <summary>下载进度 0~1</summary>
        public float Progress => TotalDownloadBytes > 0
            ? (float)CurrentDownloadBytes / TotalDownloadBytes
            : 0f;
    }

    /// <summary>
    /// 热更完成事件
    /// </summary>
    public struct HotUpdateCompletedEvent : IGameEvent
    {
        /// <summary>是否有实际下载（false 表示无需更新）</summary>
        public bool HasUpdate;
    }

    /// <summary>
    /// 热更失败事件
    /// </summary>
    public struct HotUpdateFailedEvent : IGameEvent
    {
        public HotUpdateFailReason Reason;
        public string Message;
    }

    public enum HotUpdateFailReason
    {
        InitializeFailed,
        VersionRequestFailed,
        ManifestUpdateFailed,
        DownloadFailed,
    }

    /// <summary>
    /// 资源加载失败事件（可由业务层订阅统一处理缺失资源）
    /// </summary>
    public struct AssetLoadFailedEvent : IGameEvent
    {
        public string AssetPath;
        public string Error;
    }
}
