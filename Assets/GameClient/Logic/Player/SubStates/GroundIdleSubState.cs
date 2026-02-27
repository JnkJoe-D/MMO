using UnityEngine;

namespace Game.Logic.Player.SubStates
{
    public class GroundIdleSubState : GroundSubState
    {
        public override void OnEnter()
        {
            if (_ctx.HostEntity.CurrentAnimSet != null && _ctx.HostEntity.CurrentAnimSet.Idle != null)
            {
                _ctx.HostEntity.AnimController?.PlayAnim(_ctx.HostEntity.CurrentAnimSet.Idle);
            }
            
            // 确保玩家松手进入 Idle 瞬间是没有硬直的
            _ctx.ClearMoveLock();
        }

        public override void OnUpdate(float deltaTime)
        {
            var provider = _ctx.HostEntity.InputProvider;
            if (provider == null) return;

            // 只要推了摇杆，马上要破冰
            if (provider.HasMovementInput())
            {
                // 如果恰巧这刻他还死死按着 Shift，直接进入爆发冲刺
                if (provider.GetActionState(Game.Input.InputActionType.Dash))
                {
                    ChangeState(_ctx.DashState);
                }
                else
                {
                    // 否则自然转入慢跑
                    ChangeState(_ctx.JogState);
                }
            }
        }
    }
}
