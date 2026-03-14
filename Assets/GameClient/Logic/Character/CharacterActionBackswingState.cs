using Game.FSM;
using SkillEditor;
using UnityEngine;

namespace Game.Logic.Character
{
    /// <summary>
    /// 动作后摇等待态 (Action Backswing State / Recovery State)
    /// 职责：扮演“逻辑上的待机，表现上的延续”。
    /// 1. 允许通过位移输入瞬间打断（回 GroundState）。
    /// 2. 允许接收普攻/闪避/技能等全量指令并进行连段裁决（回 SkillState/EvadeState）。
    /// 3. 若无任何操作且当前动作播放完毕，则自然回归地面。
    /// </summary>
    public class CharacterActionBackswingState : CharacterStateBase
    {
        public override void OnEnter()
        {
        //     // 订阅所有手感指令，与 Idle/SkillState 逻辑对齐
        //     if (Entity.InputProvider != null)
        //     {
        //         Entity.InputProvider.OnBasicAttackStarted += OnBasicAttackRequest;
        //         Entity.InputProvider.OnBasicAttackHoldStart += OnBasicAttackHoldStart;
        //         Entity.InputProvider.OnBasicAttackHold += OnBasicAttackHold;
        //         Entity.InputProvider.OnBasicAttackHoldCancel += OnBasicAttackHoldCancel;
        //         Entity.InputProvider.OnSpecialAttack += OnSpecialAttackRequest;
        //         Entity.InputProvider.OnUltimate += OnUltimateRequest;
        //         Entity.InputProvider.OnEvadeFrontStarted += OnEvadeFrontRequest;
        //         Entity.InputProvider.OnEvadeBackStarted += OnEvadeBackRequest;
        //     }
        }

        public override void OnUpdate(float deltaTime)
        {
            // 1. 打断逻辑：只要有摇杆推入，立即视为进入移动请求
            if (Entity.InputProvider != null && Entity.InputProvider.HasMovementInput())
            {
                ReturnToGround();
                return;
            }

            // 2. 自然结束逻辑：
            // 现在由 ComboController.OnWindowExit(Fallback) 确定性地驱动，无需轮询探测。
        }

        private void ReturnToGround()
        {
            if (Entity.MovementController != null && Entity.MovementController.IsGrounded)
            {
                Machine.ChangeState<CharacterGroundState>();
            }
            else
            {
                Machine.ChangeState<CharacterAirborneState>();
            }
        }

        // private void OnBasicAttackRequest() => Entity.ComboController.OnInput(BufferedInputType.BasicAttack);
        // private void OnBasicAttackHoldStart() {}
        // private void OnBasicAttackHold() => Entity.ComboController.OnInput(BufferedInputType.BasicAttackHold);
        // private void OnBasicAttackHoldCancel() {}
        // private void OnSpecialAttackRequest() => Entity.ComboController.OnInput(BufferedInputType.SpecialAttack);
        // private void OnUltimateRequest() => Entity.ComboController.OnInput(BufferedInputType.Ultimate);
        // private void OnEvadeFrontRequest() => Entity.ComboController.OnInput(BufferedInputType.EvadeFront);
        // private void OnEvadeBackRequest() => Entity.ComboController.OnInput(BufferedInputType.EvadeBack);

        public override void OnExit()
        {
            // // 如果即将进入的是新的技能/闪避状态，我们不需要在这里 StopAction，
            // // 因为新的状态会自己根据需要接管播放器。
            // // 但如果回到了地面（Idle），通常需要保证动作彻底清场。
            // if (Machine.NextState is CharacterGroundState || Machine.NextState is CharacterAirborneState)
            // {
            //     Entity.ActionPlayer.StopAction();
            // }

            // if (Entity.InputProvider != null)
            // {
            //     Entity.InputProvider.OnBasicAttackStarted -= OnBasicAttackRequest;
            //     Entity.InputProvider.OnBasicAttackHoldStart -= OnBasicAttackHoldStart;
            //     Entity.InputProvider.OnBasicAttackHold -= OnBasicAttackHold;
            //     Entity.InputProvider.OnBasicAttackHoldCancel -= OnBasicAttackHoldCancel;
            //     Entity.InputProvider.OnSpecialAttack -= OnSpecialAttackRequest;
            //     Entity.InputProvider.OnUltimate -= OnUltimateRequest;
            //     Entity.InputProvider.OnEvadeFrontStarted -= OnEvadeFrontRequest;
            //     Entity.InputProvider.OnEvadeBackStarted -= OnEvadeBackRequest;
            // }
        }
    }
}
