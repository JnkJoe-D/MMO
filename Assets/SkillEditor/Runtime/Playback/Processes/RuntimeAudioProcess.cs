namespace SkillEditor
{
    /// <summary>
    /// 运行时：音频片段 Process 骨架
    /// 具体播放逻辑由用户后续填充
    /// </summary>
    [ProcessBinding(typeof(AudioClip), PlayMode.Runtime)]
    public class RuntimeAudioProcess : ProcessBase<AudioClip>
    {
        public override void OnEnter()
        {
            // TODO: 运行时音频播放逻辑
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
            // 音频由引擎自驱动
        }

        public override void OnExit()
        {
            // TODO: 停止音频
        }
    }
}
