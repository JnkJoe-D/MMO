using System;
using System.Collections.Generic;
using Game.MAnimSystem;
using UnityEngine;
namespace SkillEditor.Editor
{
    /// <summary>
    /// 编辑器预览：动画片段 Process
    /// 调用 AnimComponent 的播放控制与采样方法
    /// </summary>
    [ProcessBinding(typeof(SkillAnimationClip), PlayMode.EditorPreview)]
    public class EditorAnimationProcess : ProcessBase<SkillAnimationClip>
    {
        // TODO: 替换为实际的 AnimComponent 类型
        private AnimComponent animComp;
        private Dictionary<Type, System.Object> cache = new Dictionary<Type, System.Object>();
        public override void OnEnable()
        {
            animComp = context.GetComponent<AnimComponent>();
            animComp.Initialize();
            animComp.InitializeGraph();
            // 注册系统级清理（多个动画 Process 共享同一个 key，仅执行一次）
            context.RegisterCleanup("ClearPlaygraph", () => animComp.ClearPlayGraph()); // 注册退出时的清理
        }

        public override void OnEnter()
        {
            animComp.Play(clip.animationClip, clip.blendInDuration);// 调用 AnimComponent 播放控制
            animComp.SetLayerSpeed((int)clip.layer,0f); // 先暂停，等待 OnUpdate 采样
            if (clip.avatarMask != null)
            {
                cache[typeof(AvatarMask)] = clip.avatarMask; // 缓存遮罩数据
                animComp.SetLayerMask((int)clip.layer, clip.avatarMask);
            }
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
            // 采样到 currentTime 对应的动画帧
            // float localTime = currentTime - clip.startTime;
            // animComp.Sample(clip.animationClip, localTime * clip.playbackSpeed);
            animComp.Evaluate(currentTime-clip.startTime);
            // 手动驱动层逻辑（如权重 Fade），确保 Mixer 输入权重正确
            animComp.ManualUpdate(deltaTime);
        }

        public override void OnExit()
        {
            // 停止当前片段的采样
            if (cache.TryGetValue(typeof(AvatarMask), out object maskObj) && maskObj is AvatarMask avatarMask)
            {
                animComp.SetLayerMask((int)clip.layer, avatarMask); // 恢复原始遮罩
            }
        }
        public override void OnDisable()
        {
            // 额外的清理（如果需要）
        }
        public override void Reset()
        {
            base.Reset();
            animComp = null;
            cache.Clear(); // 清空缓存
        }
    }
}
