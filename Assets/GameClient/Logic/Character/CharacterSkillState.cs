using Game.FSM;
using Game.Logic.Skill.Config;
using SkillEditor;
using UnityEngine;

namespace Game.Logic.Character
{


    /// <summary>
    /// 角色的顶层层级状态：技能释放状态，接管 SkillRunner 的运行并监听按键连接
    /// </summary>
    public class CharacterSkillState : CharacterStateBase
    {
        private SkillEditor.SkillRunner _runner;
        private ProcessContext _context;
        private bool _isSkillFinished;
        
        // 连招缓冲：当玩家在 InputWindow 开启前提前输入时记录
        private BufferedInputType _bufferedInput = BufferedInputType.None;
        private float _bufferedInputTime = -999f;
        
        // 防止玩家由于狂按导致双击判定或前一按键未能及时释放，引入防抖
        private float _skillStartTime;

        private float PRE_INPUT_INTERVAL = 0.5f;
        private SkillConfigSO currentSkill;
        private bool isBasicAttackHold;
        public override void OnEnter()
        {
            _isSkillFinished = false;
            _bufferedInput = BufferedInputType.None;
            Entity.IsComboInputOpen = false;

            // 监听普攻连接和时间轴发出的逻辑事件
            if (Entity.InputProvider != null)
            {
                Entity.InputProvider.OnBasicAttackStarted += OnBasicAttackRequest;
                Entity.InputProvider.OnBasicAttackCanceled += OnBasicAttackRequestCancel;
                Entity.InputProvider.OnBasicAttackHoldStart += OnBasicAttackRequestHoldStart;
                Entity.InputProvider.OnBasicAttackHold += OnBasicAttackRequestHold;
                Entity.InputProvider.OnBasicAttackHoldCancel += OnBasicAttackRequestHoldCancel;
                Entity.InputProvider.OnSpecialAttack += OnSpecialAttackRequest;
                Entity.InputProvider.OnUltimate += OnUltimateRequest;
            }
                
            Entity.OnSkillTimelineEvent += OnReceiveTimelineEvent;

            PlayCurrentSkill();
        }

        private void PlayCurrentSkill()
        {
            _isSkillFinished = false;
            _skillStartTime = Time.time;

            var skillConfig = Entity.NextSkillToCast;
            if (skillConfig == null)
            {
                _isSkillFinished = true; return;
            }

            var timeline = Game.Logic.Skill.SkillManager.Instance.GetOrLoadTimeline(skillConfig);
            if (timeline == null)
            {
                _isSkillFinished = true; return;
            }

            _runner = Game.Logic.Skill.SkillManager.Instance.GetRunner(Entity);
            _context = Game.Logic.Skill.SkillManager.Instance.GetContext(Entity);

            _runner.OnEnd -= OnSkillEnd;
            _runner.OnEnd += OnSkillEnd;

            // 切面刷新面朝向，技能释放前将自己转向摇杆方向
            var inputDir = Entity.InputProvider?.GetMovementDirection() ?? Vector2.zero;
            if (inputDir.sqrMagnitude > 0.01f && Entity.CameraController != null)
            {
                Vector3 worldDir = Entity.CameraController.GetForward() * inputDir.y + Entity.CameraController.GetRight() * inputDir.x;
                worldDir.y = 0;
                if(worldDir != Vector3.zero)
                {
                    Entity.transform.forward = worldDir.normalized;
                }
            }
            Debug.Log($"<color=#0FFFFF>[Combo] PlayCurrentSkill {Entity.NextSkillToCast}</color>");
            Debug.Log($"<color=#0FFFFF>[Combo] PlayCurrentSkill {skillConfig.TimelineAsset.name}</color>");
            _runner.Play(timeline, _context);
            currentSkill = skillConfig;
            Debug.Log($"<color=#E2243C>PlaySkill!!!</color>");
        }

        private void OnReceiveTimelineEvent(string eventName)
        {
            // 这是在时间轴里自定义的字符串，开启连段输入窗口
            if (eventName == "InputAvailable")
            {
                Entity.IsComboInputOpen = true;

                // 如果在这之前玩家已经提前输入过了，且距离此刻不超过预输入阀值，则允许成功缓冲发招
                if (_bufferedInput != BufferedInputType.None)
                {
                    if (Time.time - _bufferedInputTime <= PRE_INPUT_INTERVAL)
                    {
                        TryConsumeBufferedInput();
                    }
                    else
                    {
                        _bufferedInput = BufferedInputType.None;
                    }
                }
            }
        }

        private void TryConsumeBufferedInput()
        {
            var input = _bufferedInput;
            Debug.Log($"<color=#FF8C00>[Input] TryConsumeBufferedInput() called. _bufferedInput={input}, isHold={isBasicAttackHold}</color>");

            // 【核心修正】如果预输入的是单击，并且玩家此刻正**按住**按键没有松手，且当前技能**配置了长按选项**
            if (input == BufferedInputType.BasicAttack && isBasicAttackHold)
            {
                bool hasHoldTransition = currentSkill != null && currentSkill.OutTransitions != null 
                    && currentSkill.OutTransitions.Count > 0 
                    && currentSkill.OutTransitions.Exists(t => t.RequiredCommand == BufferedInputType.BasicAttackHold);

                if (hasHoldTransition)
                {
                    Debug.Log($"<color=#FF8C00>[Input] TryConsumeBufferedInput() - Deferred tap! Waiting for Hold or Release.</color>");
                    return;
                }
            }

            _bufferedInput = BufferedInputType.None;
            TryAdvanceComboFromTransitions(input);
        }

        private void OnBasicAttackRequest()
        {
            if (Time.time - _skillStartTime < 0.1f) return;

            Debug.Log($"<color=#32CD32>[Input] OnBasicAttackRequest() (Tap Pressed). IsComboInputOpen={Entity.IsComboInputOpen}</color>");
            if (Entity.IsComboInputOpen)
            {
                TryAdvanceComboFromTransitions(BufferedInputType.BasicAttack);
            }
            else
            {
                _bufferedInput = BufferedInputType.BasicAttack;
                _bufferedInputTime = Time.time;
            }
        }

        private void OnBasicAttackRequestCancel()
        {
            Debug.Log($"<color=#32CD32>[Input] OnBasicAttackRequestCancel() (Tap Released). _bufferedInput={_bufferedInput}, IsComboInputOpen={Entity.IsComboInputOpen}</color>");
            if (_bufferedInput == BufferedInputType.BasicAttack)
            {
                if (Entity.IsComboInputOpen)
                {
                    TryAdvanceComboFromTransitions(BufferedInputType.BasicAttack);
                    _bufferedInput = BufferedInputType.None; // 触发后清空
                }
            }
        }
        private void OnBasicAttackRequestHoldStart()
        {
            if (Time.time - _skillStartTime < 0.1f) return;

            isBasicAttackHold = true;
        }
        private void OnBasicAttackRequestHold()
        {
            if (Time.time - _skillStartTime < 0.1f) return;

            if (Entity.IsComboInputOpen)
            {
                TryAdvanceComboFromTransitions(BufferedInputType.BasicAttackHold);
                Debug.Log("<color=#DC143C>TryAdvanceComboFromTransitions</color>");
            }
            else
            {
                _bufferedInput = BufferedInputType.BasicAttackHold;
                _bufferedInputTime = Time.time;
                Debug.Log($"<color=#DC143C>_bufferedInput:{_bufferedInput}</color>");
            }
        }
        private void OnBasicAttackRequestHoldCancel()
        {
            isBasicAttackHold = false;
        }
        private void OnSpecialAttackRequest()
        {
            if (Time.time - _skillStartTime < 0.1f) return;

            if (Entity.IsComboInputOpen)
            {
                TryAdvanceComboFromTransitions(BufferedInputType.SpecialAttack);
            }
            else
            {
                _bufferedInput = BufferedInputType.SpecialAttack;
                _bufferedInputTime = Time.time;
            }
        }

        private void OnUltimateRequest()
        {
            if (Time.time - _skillStartTime < 0.1f) return;

            if (Entity.IsComboInputOpen)
            {
                TryAdvanceComboFromTransitions(BufferedInputType.Ultimate);
            }
            else
            {
                _bufferedInput = BufferedInputType.Ultimate;
                _bufferedInputTime = Time.time;
            }
        }

        private void TryAdvanceComboFromTransitions(BufferedInputType inputCommand)
        {
            Debug.Log($"<color=#00FFFF>[Combo] TryAdvanceComboFromTransitions({inputCommand}) called.</color>");
            if (currentSkill == null || currentSkill.OutTransitions == null || currentSkill.OutTransitions.Count == 0) 
            {
                _bufferedInput = BufferedInputType.None;
                return;
            }

            // 按列表顺序查表 (优先级向下遍历)
            foreach (var transition in currentSkill.OutTransitions)
            {
                if (transition.Evaluate(inputCommand, Entity))
                {
                    Debug.Log($"<color=#00FFFF>[Combo] Match Found! Transitioning to {inputCommand}</color>");
                    Debug.Log($"<color=#00FFFF>[Combo] Match Found! Transitioning to {transition.RequiredCommand}</color>");
                    Debug.Log($"<color=#00FFFF>[Combo] Match Found! Transitioning to {transition.NextSkill?.name ?? "NULL"}</color>");
                    // 命中！成功找到符合按键和状态的下一个招式
                    Entity.NextSkillToCast = transition.NextSkill;
                    Debug.Log($"<color=#00FFFF>[Combo] Match Found! Transitioning to {Entity.NextSkillToCast}</color>");

                    // 清空本地缓存与输入锁
                    _bufferedInput = BufferedInputType.None;
                    Entity.IsComboInputOpen = false;
                    
                    // 因为复用了状态机，不需要走 FSM 重进，直接播即可
                    _runner.Stop();
                    PlayCurrentSkill();
                    return;
                }
            }

            // 如果遍历完发现没有匹配的派生（比如没有配特殊技分支），则清空当前错误缓冲
            _bufferedInput = BufferedInputType.None;
        }

        public override void OnUpdate(float deltaTime)
        {
            // 如果当前已经进入后摇允许连招的阶段，同时玩家输入了方向键，则允许通过移动打断当前技能的后摇
            if (Entity.IsComboInputOpen &&_bufferedInput==BufferedInputType.None&& Entity.InputProvider != null && Entity.InputProvider.HasMovementInput())
            {
                if (Entity.MovementController != null && Entity.MovementController.IsGrounded)
                    Machine.ChangeState<CharacterGroundState>();
                else
                    Machine.ChangeState<CharacterAirborneState>();
                return;
            }
            if (_isSkillFinished)
            {
                if (Entity.MovementController != null && Entity.MovementController.IsGrounded)
                    Machine.ChangeState<CharacterGroundState>();
                else
                    Machine.ChangeState<CharacterAirborneState>();
                return;
            }
            if (currentSkill!=null &&_context!=null)  //速率同步
            {
                if(currentSkill.Category==SkillCategory.LightAttack 
                || currentSkill.Category == SkillCategory.HeavyAttack
                || currentSkill.Category == SkillCategory.DashAttack)
                {
                    _context.GlobalPlaySpeed=Entity.Config.AttackMultipier;
                }
                else
                {
                    _context.GlobalPlaySpeed = Entity.Config.SkillMultipier;
                }
            }
            _runner?.Tick(deltaTime);
        }

        public override void OnExit()
        {
            if (_runner != null)
            {
                _runner.OnEnd -= OnSkillEnd;
                _runner.Stop();
                _runner = null;
            }
            _context = null;
            Entity.NextSkillToCast = null;
            currentSkill = null;
            // 清理监听
            if (Entity.InputProvider != null)
            {
                Entity.InputProvider.OnBasicAttackStarted -= OnBasicAttackRequest;
                Entity.InputProvider.OnBasicAttackCanceled -= OnBasicAttackRequestCancel;
                Entity.InputProvider.OnBasicAttackHoldStart -= OnBasicAttackRequestHoldStart;
                Entity.InputProvider.OnBasicAttackHold -= OnBasicAttackRequestHold;
                Entity.InputProvider.OnBasicAttackHoldCancel -= OnBasicAttackRequestHoldCancel;
                Entity.InputProvider.OnSpecialAttack -= OnSpecialAttackRequest;
                Entity.InputProvider.OnUltimate -= OnUltimateRequest;
            }
            Entity.OnSkillTimelineEvent -= OnReceiveTimelineEvent;
            
            Entity.IsComboInputOpen = false;
            _bufferedInput = BufferedInputType.None;
        }

        private void OnSkillEnd()
        {
            _isSkillFinished = true;
            currentSkill = null;
        }
    }
}
