using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Test
{
    public class LoginView : UIView
    {
        public Button btnLogin;
        public Button btnClose;
        public Text   txtStatus;

        public override void OnInit()
        {
            // 真实项目中这里通常用代码生成器或手动拖拽
            // 为了测试，我们尝试通过 Find 去获取
            btnLogin  = transform.Find("BtnLogin")?.GetComponent<Button>();
            btnClose  = transform.Find("BtnClose")?.GetComponent<Button>();
            txtStatus = transform.Find("TxtStatus")?.GetComponent<Text>();
        }

        public void BindStatus(string status)
        {
            if (txtStatus != null)
                txtStatus.text = status;
        }
    }

    public class LoginModel : UIModel
    {
        private string _status;
        public string Status
        {
            get => _status;
            set { _status = value; NotifyChanged(); }
        }

        public override void Reset()
        {
            _status = string.Empty;
        }
    }

    [UIPanel(ViewPrefab = "Assets/GameClient/UI/Test/Prefab/LoginPanel.prefab", Layer = UILayer.Window, IsFullScreen = true)]
    public class LoginModule : UIModule<LoginView, LoginModel>
    {
        protected override void OnCreate()
        {
            if (View.btnClose != null)
                View.btnClose.onClick.AddListener(OnCloseClick);
            
            if (View.btnLogin != null)
                View.btnLogin.onClick.AddListener(OnLoginClick);

            Model.OnChanged += RefreshView;
        }

        protected override void OnShow(object data)
        {
            Model.Status = "请输入账号密码";
        }

        private async void OnLoginClick()
        {
            Model.Status = "正在登录中...";
            // 模拟登录
            await SimulateLogin();
        }

        private async System.Threading.Tasks.Task SimulateLogin()
        {
            await System.Threading.Tasks.Task.Delay(1000);
            Model.Status = "登录成功！";
        }

        private void OnCloseClick()
        {
            UIManager.Instance.Close(this);
        }

        private void RefreshView()
        {
            View.BindStatus(Model.Status);
        }
    }
}
