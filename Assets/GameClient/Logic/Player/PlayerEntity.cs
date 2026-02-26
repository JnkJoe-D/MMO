using Game.FSM;
using Game.Input;
using UnityEngine;

namespace Game.Logic.Player
{
    /// <summary>
    /// 主角或玩家控制体的核心枢纽
    /// 管理状态机、持有各项解耦接口（Input、移动、动画），并对这些零件起粘合协调作用
    /// </summary>
    public class PlayerEntity : MonoBehaviour
    {
        // === 对底层组件的松散引用 ===
        public IInputProvider InputProvider { get; private set; }
        public IMovementController MovementController { get; private set; }
        public IAnimController AnimController { get; private set; }
        // 专门处理该实体视角的组件（不依赖全局管理，哪怕是没相机的服务器克隆体也可以模拟前向）
        public ICameraController CameraController { get; private set; }
        
        // === 状态机引用 ===
        public FSMSystem<PlayerEntity> StateMachine { get; private set; }

        private void Awake()
        {
            // 在实际工业架构中，它们通过依赖注入容器或 Awake GetComponent 汇聚到实体上
            InputProvider = GetComponent<IInputProvider>();
            MovementController = GetComponent<IMovementController>();
            AnimController = GetComponent<IAnimController>();
            // 实体视听组件
            CameraController = GetComponent<ICameraController>();

            if (InputProvider == null || MovementController == null || AnimController == null)
            {
                Debug.LogWarning($"[PlayerEntity] {gameObject.name} 缺少部分控制组件！");
            }
        }

        // （测试用）代表它是主角模型类型
        private int _roleId = 1001;
        // （测试用）代表现在空手或者手握单手剑
        private int _currentWeaponType = 0;

        // --- 供 State 拿取配置动作 ---
        public Game.Logic.Player.Config.AnimSetEntry CurrentAnimSet { get; private set; }

        private void Start()
        {
            // ===== 1. 请求加载这具身躯与装备对应的移动动画包 =====
            var animSet = Game.Logic.Player.Config.AnimationConfigManager.Instance?.AcquireSet(_roleId, _currentWeaponType);
            if (animSet != null)
            {
                CurrentAnimSet = animSet;
                Debug.Log($"[PlayerEntity] 基础动画集获取成功：Role={_roleId}, Weapon={_currentWeaponType}");
            }
            else
            {
                Debug.LogError("[PlayerEntity] 无法获得动画配置。如果移动播放报错，请检查 Resources/GlobalAnimationConfig 是否配有正确数据！");
            }

            if (StateMachine == null)
            {
                var fsmMgr = Game.FSM.FSMManager.Instance;
                if (fsmMgr != null)
                {
                    StateMachine = fsmMgr.CreateFSM<PlayerEntity>(this);
                    StateMachine.AddState(new PlayerGroundState());
                    StateMachine.AddState(new PlayerAirborneState());
                    StateMachine.ChangeState<PlayerGroundState>();
                }
                else
                {
                    Debug.LogError("[PlayerEntity] 无法创建状态机，找不到 FSMManager 单例！");
                }
            }

            // 实体入场时，认领当前全局相机的跟随聚焦
            Game.Camera.GameCameraManager.Instance?.SetTarget(this.transform);
        }

        private void OnDestroy()
        {
            if (FSMManager.Instance != null && StateMachine != null)
            {
                // 回收该角色的所有计算轮组
                FSMManager.Instance.DestroyFSM(StateMachine);
                StateMachine = null;
            }
        }
    }
}
