using UnityEngine;
using Game.Framework;
using Game.Network;
using Game.Network.Protocol;
using Game.UI;

namespace Game.Test
{
    /// <summary>
    /// 网络功能测试脚本
    /// 挂载到场景中即可通过快捷键测试网络联通性
    /// </summary>
    public class NetworkTest : MonoBehaviour
    {
        [Header("快捷键配置")]
        public KeyCode connectKey = KeyCode.C;
        public KeyCode disconnectKey = KeyCode.D;
        public KeyCode loginTestKey = KeyCode.L;

        public KeyCode uiTestKey    = KeyCode.U;

        private void Start()
        {
            // 订阅网络事件
            EventCenter.Subscribe<NetConnectedEvent>(OnConnected);
            EventCenter.Subscribe<NetDisconnectedEvent>(OnDisconnected);
            EventCenter.Subscribe<HeartbeatResponseEvent>(OnHeartbeat);
            EventCenter.Subscribe<ServerErrorEvent>(OnServerError);
            EventCenter.Subscribe<NetReconnectingEvent>(OnReconnecting);
            EventCenter.Subscribe<NetReconnectedEvent>(OnReconnected);
        }

        private void OnDestroy()
        {
            // 取消订阅
            EventCenter.Unsubscribe<NetConnectedEvent>(OnConnected);
            EventCenter.Unsubscribe<NetDisconnectedEvent>(OnDisconnected);
            EventCenter.Unsubscribe<HeartbeatResponseEvent>(OnHeartbeat);
            EventCenter.Unsubscribe<ServerErrorEvent>(OnServerError);
            EventCenter.Unsubscribe<NetReconnectingEvent>(OnReconnecting);
            EventCenter.Unsubscribe<NetReconnectedEvent>(OnReconnected);
        }

        private void Update()
        {
            if (Input.GetKeyDown(connectKey))
            {
                Debug.Log("[NetworkTest] 尝试连接 TCP...");
                NetworkManager.Instance?.ConnectTcp();
            }

            if (Input.GetKeyDown(disconnectKey))
            {
                Debug.Log("[NetworkTest] 尝试主动断开 TCP...");
                NetworkManager.Instance?.DisconnectTcp();
            }

            if (Input.GetKeyDown(loginTestKey))
            {
                // 注意：由于当前只有手写的核心消息，login.proto 还没编译，
                // 这里只能演示发包结构，暂时无法发送真正的 C2S_Login 对象
                // 除非手动在 GeneratedMessages.cs 里也手写一份 C2S_Login
                Debug.Log("[NetworkTest] 登录协议尚未编译，目前仅支持心跳测试");
            }

            if (Input.GetKeyDown(uiTestKey))
            {
                Debug.Log("[NetworkTest] 尝试打开测试 UI...");
                UIManager.Instance.Open<Game.UI.Test.LoginModule>();
            }
        }

        // ── 事件回调 ────────────────────────────

        private void OnConnected(NetConnectedEvent evt)
        {
            Debug.Log($"<color=green>[NetworkTest] 连接成功！Host: {evt.Host}, Port: {evt.Port}</color>");
        }

        private void OnDisconnected(NetDisconnectedEvent evt)
        {
            Debug.Log($"<color=red>[NetworkTest] 连接断开！原因: {evt.Reason}, 消息: {evt.Message}</color>");
        }

        private void OnHeartbeat(HeartbeatResponseEvent evt)
        {
            Debug.Log($"<color=cyan>[NetworkTest] 收到心跳响应 | RTT: {evt.RttMs}ms | ServerTime: {evt.ServerTime}</color>");
        }

        private void OnServerError(ServerErrorEvent evt)
        {
            Debug.Log($"<color=yellow>[NetworkTest] 服务端报错 | Code: {evt.Code}, Msg: {evt.Message}</color>");
        }

        private void OnReconnecting(NetReconnectingEvent evt)
        {
            Debug.Log($"[NetworkTest] 正在尝试重连... 第 {evt.Attempt} 次 | 等待 {evt.WaitSeconds:F1}s");
        }

        private void OnReconnected(NetReconnectedEvent evt)
        {
            Debug.Log("<color=green>[NetworkTest] 重连成功！</color>");
        }
    }
}
