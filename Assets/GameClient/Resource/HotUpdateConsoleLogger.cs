using UnityEngine;
using Game.Framework;

namespace Game.Resource
{
    /// <summary>
    /// 热更新进度日志记录器
    /// 挂在 GameRoot 上，用于在没有 UI 的情况下通过 Console 观察热更流程
    /// 验证通过后可以删除
    /// </summary>
    public class HotUpdateConsoleLogger : MonoBehaviour
    {
        private void Start()
        {
            EventCenter.Subscribe<HotUpdateCheckStartEvent>(OnCheckStart);
            EventCenter.Subscribe<HotUpdateRequireConfirmEvent>(OnFoundUpdate);
            EventCenter.Subscribe<HotUpdateProgressEvent>(OnProgress);
            EventCenter.Subscribe<HotUpdateCompletedEvent>(OnCompleted);
            EventCenter.Subscribe<HotUpdateFailedEvent>(OnFailed);
            EventCenter.Subscribe<ResourceInitializedEvent>(OnInited);
        }

        private void OnDestroy()
        {
            EventCenter.Unsubscribe<HotUpdateCheckStartEvent>(OnCheckStart);
            EventCenter.Unsubscribe<HotUpdateRequireConfirmEvent>(OnFoundUpdate);
            EventCenter.Unsubscribe<HotUpdateProgressEvent>(OnProgress);
            EventCenter.Unsubscribe<HotUpdateCompletedEvent>(OnCompleted);
            EventCenter.Unsubscribe<HotUpdateFailedEvent>(OnFailed);
            EventCenter.Unsubscribe<ResourceInitializedEvent>(OnInited);
        }

        private void OnCheckStart(HotUpdateCheckStartEvent e)
        {
            Debug.Log("<color=cyan>[HotUpdate] 开始检查资源更新...</color>");
        }

        private void OnFoundUpdate(HotUpdateRequireConfirmEvent e)
        {
            Debug.Log($"<color=yellow>[HotUpdate] 发现新资源！文件数: {e.FileCount}, 总大小: {e.TotalDownloadBytes / 1024f / 1024f:F2} MB</color>");
        }

        private void OnProgress(HotUpdateProgressEvent e)
        {
            Debug.Log($"[HotUpdate] 下载进度: {e.Progress * 100:F1}% ({e.CurrentDownloadCount}/{e.TotalDownloadCount})");
        }

        private void OnCompleted(HotUpdateCompletedEvent e)
        {
            Debug.Log($"<color=green>[HotUpdate] 热更新完成！是否有更新: {e.HasUpdate}</color>");
        }

        private void OnFailed(HotUpdateFailedEvent e)
        {
            Debug.LogError($"<color=red>[HotUpdate] 热更新失败！原因: {e.Reason}, 详情: {e.Message}</color>");
        }

        private void OnInited(ResourceInitializedEvent e)
        {
            Debug.Log("<color=lime>[Resource] 资源系统加载就绪，可以开始游戏逻辑。</color>");
        }
    }
}
