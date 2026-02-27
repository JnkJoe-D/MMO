using UnityEngine;

namespace Game.Logic.Player.SubStates
{
    /// <summary>
    /// 地表子状态基类。
    /// 生命周期由外层的 PlayerGroundState 负责调度，享用外层传递进来的上下文。
    /// </summary>
    public abstract class GroundSubState
    {
        protected PlayerGroundState _ctx;

        public void Initialize(PlayerGroundState context)
        {
            _ctx = context;
        }

        public virtual bool CanEnter() { return true; }
        public virtual bool CanExit() { return true; }

        public virtual void OnEnter() { }
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnExit() { }
        
        /// <summary>
        /// 方便子状态请求父容器切换状态
        /// </summary>
        protected bool ChangeState(GroundSubState newState)
        {
            return _ctx.ChangeSubState(newState);
        }
    }
}
