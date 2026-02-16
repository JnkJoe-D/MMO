namespace SkillEditor
{
    /// <summary>
    /// 位移片段 Process（编辑器和运行时共用）
    /// </summary>
    [ProcessBinding(typeof(MovementClip), PlayMode.EditorPreview)]
    [ProcessBinding(typeof(MovementClip), PlayMode.Runtime)]
    public class MovementProcess : ProcessBase<MovementClip>
    {
        public override void OnEnter()
        {
            // TODO: 开始位移
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
            // TODO: 更新位移（冲刺、闪避等）
        }

        public override void OnExit()
        {
            // TODO: 结束位移
        }
    }
}
