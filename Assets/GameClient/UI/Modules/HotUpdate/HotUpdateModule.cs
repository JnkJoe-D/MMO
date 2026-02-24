using UnityEngine;
using Game.Framework;
using Game.Resource;
using TMPro;

namespace Game.UI.Modules.HotUpdate
{
    [UIPanel(ViewPrefab = "Assets/Resources/Prefab/UI/PanelView/HotUpdatePanel.prefab", Layer = UILayer.Loading, IsFullScreen = true)]
    public class HotUpdateModule : UIModule<HotUpdateView, HotUpdateModel>
    {
        protected override void OnCreate()
        {   
            // 订阅资源下载进度事件
            EventCenter.Subscribe<HotUpdateProgressEvent>(OnDownloadProgress);
            // 订阅资源初始化完毕事件（用于跳进登录界面）
            EventCenter.Subscribe<ResourceInitializedEvent>(OnResourceReady);
            // 订阅需要用户确认更新的事件（弹出 MessageBox）
            EventCenter.Subscribe<HotUpdateRequireConfirmEvent>(OnRequireConfirm);
            // 订阅更新失败事件
            EventCenter.Subscribe<HotUpdateFailedEvent>(OnUpdateFailed);
            // 订阅状态阶段事件（如检查版本，更新清单的进度等）
            EventCenter.Subscribe<HotUpdateStatusEvent>(OnStatusUpdate);
            // 订阅更新完成事件以自动卸载自身
            EventCenter.Subscribe<HotUpdateCompletedEvent>(OnUpdateCompleted);
            
            RefreshView();
        }

        protected override void OnRemove()
        {
            base.OnRemove();
            EventCenter.Unsubscribe<HotUpdateProgressEvent>(OnDownloadProgress);
            EventCenter.Unsubscribe<ResourceInitializedEvent>(OnResourceReady);
            EventCenter.Unsubscribe<HotUpdateRequireConfirmEvent>(OnRequireConfirm);
            EventCenter.Unsubscribe<HotUpdateFailedEvent>(OnUpdateFailed);
            EventCenter.Unsubscribe<HotUpdateStatusEvent>(OnStatusUpdate);
            EventCenter.Unsubscribe<HotUpdateCompletedEvent>(OnUpdateCompleted);
        }

        private void OnDownloadProgress(HotUpdateProgressEvent e)
        {
            Model.DownloadProgress = e.Progress;
            Model.StatusText = $"正在下载资源：{e.CurrentDownloadBytes / 1048576f:F2} MB / {e.TotalDownloadBytes / 1048576f:F2} MB";
            
            // 计算简单的速度 (可根据真实时间 Delta 计算)
            Model.SpeedText = "下载中...";
            
            RefreshView();
        }

        private void OnResourceReady(ResourceInitializedEvent e)
        {
            Debug.Log("[HotUpdateModule] 资源就绪，准备进入登录界面");
            // 这里通常等待 0.5s 平滑过渡，然后关闭本界面，打开 LoginPanel
            // 暂时由于没有 LoginPanel，我们只打印 Log
        }

        private void OnRequireConfirm(HotUpdateRequireConfirmEvent e)
        {
            // 这是关键点：不再局限在 Widget，而是调用全局的 MessageBox
            UIManager.Instance.Open<Common.MessageBoxModule>(new Common.MessageBoxModel
            {
                Title = "发现新版本",
                Content = $"本次需要更新 {e.TotalDownloadBytes / 1048576f:F10} MB 资源，是否立即下载？",
                ConfirmText = "立即更新",
                CancelText = "退出游戏",
                OnConfirm = () => 
                {
                    // 通知底层继续下载
                    e.ConfirmAction?.Invoke();
                },
                OnCancel = () => 
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
            });
        }

        private void OnStatusUpdate(HotUpdateStatusEvent e)
        {
            Model.DownloadProgress = e.Progress;
            Model.StatusText = e.StatusText;
            Model.SpeedText = ""; // 检查阶段不需要网络速度
            RefreshView();
        }

        private void OnUpdateCompleted(HotUpdateCompletedEvent e)
        {
            Debug.Log("[HotUpdateModule] 收到更新完成事件，自动关闭热更界面。");
            UIManager.Instance.Close(this);
        }

        private void OnUpdateFailed(HotUpdateFailedEvent e)
        {
            UIManager.Instance.Open<Common.MessageBoxModule>(new Common.MessageBoxModel
            {
                Title = "更新失败",
                Content = $"遇到错误：{e.Message}\n请检查网络后重试。",
                ConfirmText = "重试",
                OnConfirm = () => 
                {
                    // 触发某些重试逻辑，本示例简化
                    Debug.Log("重试下载...");
                }
            });
        }

        private void RefreshView()
        {
            if (View == null) return;

            /*
            // -------------------------------------------------------------
            // 【正式代码示例】通过配置表加载静态文本与图片背景 (假设拥有 TbUIConfig)
            // -------------------------------------------------------------
            // 1. 获取配表数据
            // var uiConfig = ConfigManager.Instance.Tables.TbUIConfig.Get(1001); 
            // 
            // 2. 加载 Sprite 图集并赋值给 Background
            // var bgHandle = ResourceManager.Instance.LoadAssetAsync<Sprite>(uiConfig.BgPath, sprite => {
            //      View.Background.sprite = sprite;
            // });
            //
            // 3. 赋值多语言 / 硬编码文字
            // View.HeaderText.text = uiConfig.TitleText;
            */

            // -------------------------------------------------------------
            // 本地测试：直接操作 View 组件中绑定的对象
            // 由于上面未开启正式加载代码，背景图片将直接使用预制体默认指定的引用
            // -------------------------------------------------------------
            
            if (View.ProgressImage != null)
            {
                View.ProgressImage.fillAmount = Model.DownloadProgress;
            }

            if (View.ProcessText != null && !string.IsNullOrEmpty(Model.StatusText))
            {
                View.ProcessText.text = Model.StatusText;
            }

            if (View.VersionText != null)
            {
                View.VersionText.text = Model.VersionText;
            }

            if (View.SpeedText != null)
            {
                View.SpeedText.text = Model.SpeedText;
            }

            if (View.HeaderText != null)
            {
                View.HeaderText.text = "检测更新"; // 硬编码测试
            }
        }
    }
}
