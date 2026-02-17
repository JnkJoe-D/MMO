using Game.MAnimSystem;
using UnityEngine;
using System;
using System.Collections.Generic;
namespace SkillEditor
{
    /// <summary>
    /// 运行时：动画片段 Process 骨架
    /// 仅控制播放状态和速度，不控制权重
    /// </summary>
    [ProcessBinding(typeof(SkillAnimationClip), PlayMode.Runtime)]
    public class RuntimeAnimationProcess : ProcessBase<SkillAnimationClip>
    {
        // TODO: 替换为实际的 AnimComponent 类型
        private AnimComponent animComp;
        public override void OnEnable()
        {
            animComp = context.GetComponent<AnimComponent>();
            animComp?.Initialize(); // 确保 AnimComponent 已初始化
            animComp?.InitializeGraph(); // 确保动画图已创建
        }

        public override void OnEnter()
        {
            if (clip.avatarMask != null)
            {
                context.PushLayerMask((int)clip.layer, clip.avatarMask, animComp);
                animComp.SetLayerMask((int)clip.layer, clip.avatarMask);
            }
            // 调用 AnimComponent 播放控制 + 设置速度
            animComp.Play(clip.animationClip, (int)clip.layer, clip.blendInDuration);
            //这里的update频率比monoupdate低，所以在onenter先同步一次播放速度，确保动画按预期速度开始播放
            animComp.SetLayerSpeed((int)clip.layer, clip.playbackSpeed * context.GlobalPlaySpeed);
            Debug.Log($"[OnEnter] Play at time: {UnityEngine.Time.time}");
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
            // 仅控制播放状态和速度
            animComp.SetLayerSpeed((int)clip.layer, clip.playbackSpeed * context.GlobalPlaySpeed); // 叠加全局播放速度
        }

        public override void OnExit()
        {
            context.PopLayerMask((int)clip.layer, clip.avatarMask, animComp);
            Debug.Log($"[OnExit] OnExit at time: {UnityEngine.Time.time}");
            // 可选：停止当前动画片段
        }

        public override void Reset()
        {
            base.Reset();
            animComp = null;
        }
    }
}
