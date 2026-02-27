using UnityEngine;

namespace Game.Logic.Player.SubStates
{
    public class GroundDashSubState : GroundSubState
    {
        private bool _isDashStable = false;

        public override void OnEnter()
        {
            _isDashStable = false; // 每次新切入 Dash，稳定锁防抖重新复位

            if (_ctx.HostEntity.CurrentAnimSet != null && _ctx.HostEntity.CurrentAnimSet.Dash != null)
            {
                _ctx.HostEntity.AnimController?.PlayAnim(_ctx.HostEntity.CurrentAnimSet.Dash, 0.2f, () => 
                {
                    // 黑盒回调确认动画成功渐变铺满
                    _isDashStable = true;
                });
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            var provider = _ctx.HostEntity.InputProvider;
            if (provider == null) return;

            // 1. 松摇杆触发物理急停
            if (!provider.HasMovementInput())
            {
                _ctx.StopState.SetBrakeParams(isFromDash: true, isDashStable: _isDashStable);
                ChangeState(_ctx.StopState);
                return;
            }

            // 2. 长按 Shift 折断：松手了但还在滑推摇杆，降级落回 Jog
            if (!provider.GetActionState(Game.Input.InputActionType.Dash))
            {
                ChangeState(_ctx.JogState);
                return;
            }

            // 3. 强力冲刺移动
            Vector2 inputDir = provider.GetMovementDirection();
            Vector3 worldDir = _ctx.CalculateWorldDirection(inputDir);
            
            _ctx.HostEntity.MovementController?.Move(worldDir * _ctx.DashSpeed * deltaTime);
            _ctx.HostEntity.MovementController?.FaceTo(worldDir);
        }
    }
}
