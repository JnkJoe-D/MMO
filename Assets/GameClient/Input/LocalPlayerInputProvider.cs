using System;
using UnityEngine;

namespace Game.Input
{
    /// <summary>
    /// 依附于本地玩家 GameObject 上的输入捕获提供者
    /// 将全局/底层的键鼠信号转化为 IInputProvider 标准事件
    /// </summary>
    public class LocalPlayerInputProvider : MonoBehaviour, IInputProvider
    {
        public event Action OnJumpStarted;
        public event Action OnDashStarted;
        public event Action OnBasicAttackStarted;
        public event Action OnSkill1Started;
        public event Action OnSkill2Started;
        public event Action OnSkill3Started;
        public event Action OnSkill4Started;

        private Vector2 _currentMoveInput;

        private void OnEnable()
        {
            // TODO: 当 InputManager 拥有 Actions 后，在这里进行注册
            // var actions = InputManager.Instance.Actions.Player;
            // actions.Move.performed += OnMovePerformed;
            // actions.Move.canceled += OnMoveCanceled;
            // actions.Jump.started += _ => OnJumpStarted?.Invoke();
            // actions.Attack.started += _ => OnAttackStarted?.Invoke();
            // actions.Dash.started += _ => OnDashStarted?.Invoke();
        }

        private void OnDisable()
        {
            // TODO: 对应的反注册逻辑，防止内存泄漏
            // var actions = InputManager.Instance?.Actions?.Player;
            // if (actions == null) return;
            // actions.Move.performed -= OnMovePerformed;
            // actions.Move.canceled -= OnMoveCanceled;
            // 等等...
        }

        // ==========================================
        // 具体按键映射逻辑 (等待 Unity 新 Input 处理)
        // ==========================================
        
        /*
        private void OnMovePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            _currentMoveInput = ctx.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            _currentMoveInput = Vector2.zero;
        }
        */

        // ==========================================
        // 实现 IInputProvider 接口
        // ==========================================
        
        public Vector2 GetMovementDirection()
        {
            return _currentMoveInput;
        }

        public bool HasMovementInput()
        {
            return _currentMoveInput.sqrMagnitude > 0.01f;
        }

        // --- 临时为了不装配 InputSystem 也能测试而写的 Fallback 逻辑 ---
        private void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            // 兼容性 Fallback：确保在这个框架没建好 Action表 之前跑得通
            float h = UnityEngine.Input.GetAxisRaw("Horizontal");
            float v = UnityEngine.Input.GetAxisRaw("Vertical");
            _currentMoveInput = new Vector2(h, v).normalized;

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) OnJumpStarted?.Invoke();
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftShift)) OnDashStarted?.Invoke();
            if (UnityEngine.Input.GetMouseButtonDown(0)) OnBasicAttackStarted?.Invoke();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1)) OnSkill1Started?.Invoke();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2)) OnSkill2Started?.Invoke();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3)) OnSkill3Started?.Invoke();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4)) OnSkill4Started?.Invoke();
#endif
        }
    }
}
