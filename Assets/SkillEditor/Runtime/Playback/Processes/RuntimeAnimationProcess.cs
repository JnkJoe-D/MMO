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
        // private AnimComponent animComp;

        public override void OnEnable()
        {
            // animComp = context.GetComponent<AnimComponent>();
        }

        public override void OnEnter()
        {
            // 调用 AnimComponent 播放控制 + 设置速度
            // animComp.Play(clip.animationClip, clip.playbackSpeed);
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
            // 仅控制播放状态和速度，不控制权重
        }

        public override void OnExit()
        {
            // 可选：停止当前动画片段
        }

        public override void Reset()
        {
            base.Reset();
            // animComp = null;
        }
    }
}
