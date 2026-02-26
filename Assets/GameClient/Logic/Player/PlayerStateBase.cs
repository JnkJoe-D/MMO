using Game.FSM;

namespace Game.Logic.Player
{
    /// <summary>
    /// 玩家自身行为节点基类
    /// 封装快捷属性以便派生出各种子状态(Run, Attack, Dash)用
    /// </summary>
    public abstract class PlayerStateBase : IFSMState<PlayerEntity>
    {
        protected FSMSystem<PlayerEntity> Machine;
        protected PlayerEntity Entity => Machine.Owner;

        public virtual void OnInit(FSMSystem<PlayerEntity> fsm)
        {
            Machine = fsm;
        }

        public virtual void OnEnter() { }

        public virtual void OnUpdate(float deltaTime) { }

        public virtual void OnFixedUpdate(float fixedDeltaTime) { }

        public virtual void OnExit() { }

        public virtual void OnDestroy() { }
    }
}
