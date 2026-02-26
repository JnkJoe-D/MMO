using UnityEngine;

namespace Game.Logic.Player
{
    /// <summary>
    /// 包含所有的地面运动（Idle、Run），融合为一个状态
    /// 它的关注点仅仅是基于摇杆算速度并播放合适的基础移动动画
    /// </summary>
    public class PlayerGroundState : PlayerStateBase
    {
        // 移除了之前硬编码绑在这儿的 IdleClip 和 MoveClip，改为请求配置
        public float MoveSpeed = 5.0f;
        
        private bool _isMoving = false;

        public override void OnEnter()
        {
            _isMoving = false;
            
            // 进场默认播一次待机
            if (Entity.CurrentAnimSet != null && Entity.CurrentAnimSet.Idle != null)
            {
                Entity.AnimController?.PlayAnim(Entity.CurrentAnimSet.Idle);
            }
            
            // 订阅跳跃
            var provider = Entity.InputProvider;
            if (provider != null)
            {
                provider.OnJumpStarted += HandleJump;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            var provider = Entity.InputProvider;
            if (provider == null) return;

            // TODO: 未来整合地表射线检测 `if (!IsGrounded) ChangeState<PlayerAirborneState>();` 自由落体

            bool hasInput = provider.HasMovementInput();
            
            // 动画状态切换 (使用从实体配置中拿到的动作)
            if (hasInput && !_isMoving)
            {
                _isMoving = true;
                if (Entity.CurrentAnimSet != null && Entity.CurrentAnimSet.Run != null)
                    Entity.AnimController?.PlayAnim(Entity.CurrentAnimSet.Run);
            }
            else if (!hasInput && _isMoving)
            {
                _isMoving = false;
                if (Entity.CurrentAnimSet != null && Entity.CurrentAnimSet.Idle != null)
                    Entity.AnimController?.PlayAnim(Entity.CurrentAnimSet.Idle);
            }

            // 执行移动推送
            if (hasInput)
            {
                Vector2 inputDir = provider.GetMovementDirection();
                
                // 将 2D 摇杆输入映射到该玩家实体的视觉主前/右向上
                Vector3 worldDir;
                if (Entity.CameraController != null)
                {
                    Vector3 camForward = Entity.CameraController.GetForward();
                    Vector3 camRight = Entity.CameraController.GetRight();
                    worldDir = (camForward * inputDir.y + camRight * inputDir.x).normalized;
                }
                else
                {
                    // Fallback 兜底（在没有专门相机探头时直接映射到全局地平线北/东方）
                    worldDir = new Vector3(inputDir.x, 0, inputDir.y).normalized;
                }
                
                Entity.MovementController?.Move(worldDir * MoveSpeed * deltaTime);
                Entity.MovementController?.FaceTo(worldDir);
            }
        }

        private void HandleJump()
        {
            // 给物理起跳指令，然后自己甩手切给空中状态
            // Entity.MovementController?.Jump(JumpForce);
            Machine.ChangeState<PlayerAirborneState>();
        }

        public override void OnExit()
        {
            if (Entity.InputProvider != null)
            {
                Entity.InputProvider.OnJumpStarted -= HandleJump;
            }
        }
    }
}
