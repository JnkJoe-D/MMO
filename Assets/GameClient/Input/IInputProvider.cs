using System;
using UnityEngine;

namespace Game.Input
{
    /// <summary>
    /// 标准化玩家输入接口
    /// 遵循依赖倒置原则（DIP），将具体的输入实现设备（键盘鼠标/行为树AI/网络帧）与具体的业务解耦。
    /// 所有需要操控人物的模块仅需要引用此类。
    /// </summary>
    public interface IInputProvider
    {
        // ==========================================
        // 轮询状态属性 (适合 Update、FSM 主动拉取)
        // ==========================================

        /// <summary>
        /// 获取当前移动方向（归一化后的二维向量）
        /// 支持手柄摇杆与 WASD 的通用读取。
        /// </summary>
        Vector2 GetMovementDirection();

        /// <summary>
        /// 是否有移动意图
        /// </summary>
        bool HasMovementInput();

        // ==========================================
        // 瞬间触发事件 (适合按键按下/抬起等一次性行为)
        // 这些事件未来可被 Unity New Input System 改键
        // ==========================================

        /// <summary>跳跃指令触发 (如 Space 键)</summary>
        event Action OnJumpStarted;
        
        /// <summary>冲刺/翻滚指令触发 (如 Shift 键)</summary>
        event Action OnDashStarted;

        /// <summary>基础普攻指令触发 (如 鼠标左键)</summary>
        event Action OnBasicAttackStarted;

        /// <summary>技能插槽 1 触发 (如 数字键 1)</summary>
        event Action OnSkill1Started;
        
        /// <summary>技能插槽 2 触发 (如 数字键 2)</summary>
        event Action OnSkill2Started;
        
        /// <summary>技能插槽 3 触发 (如 数字键 3)</summary>
        event Action OnSkill3Started;
        
        /// <summary>技能插槽 4 触发 (如 数字键 4)</summary>
        event Action OnSkill4Started;
    }
}
