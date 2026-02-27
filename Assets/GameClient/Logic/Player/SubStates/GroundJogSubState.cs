using UnityEngine;

namespace Game.Logic.Player.SubStates
{
    public class GroundJogSubState : GroundSubState
    {
        public override void OnEnter()
        {
            if (_ctx.HostEntity.CurrentAnimSet != null && _ctx.HostEntity.CurrentAnimSet.Jog != null)
            {
                // 可以传一个固定的渐变时长，也可由配表提供，在此暂定使用 0.3f 让步态切换柔和
                _ctx.HostEntity.AnimController?.PlayAnim(_ctx.HostEntity.CurrentAnimSet.Jog, 0.3f);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            var provider = _ctx.HostEntity.InputProvider;
            if (provider == null) return;

            // 1. 玩家停止推摇杆：触发由于慢跑产生的物理惯性刹车
            if (!provider.HasMovementInput())
            {
                _ctx.StopState.SetBrakeParams(isFromDash: false, isDashStable: false);
                ChangeState(_ctx.StopState);
                return;
            }

            // 2. 玩家在慢跑时扣紧了 Shift，进入冲刺猛跑
            if (provider.GetActionState(Game.Input.InputActionType.Dash))
            {
                ChangeState(_ctx.DashState);
                return;
            }

            // 3. 执行推移
            Vector2 inputDir = provider.GetMovementDirection();
            Vector3 worldDir = _ctx.CalculateWorldDirection(inputDir);
            
            _ctx.HostEntity.MovementController?.Move(worldDir * _ctx.JogSpeed * deltaTime);
            _ctx.HostEntity.MovementController?.FaceTo(worldDir);
        }
    }
}
