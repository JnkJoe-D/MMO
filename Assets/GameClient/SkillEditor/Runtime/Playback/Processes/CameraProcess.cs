namespace SkillEditor
{
    /// <summary>
    /// 摄像机片段 Process（编辑器和运行时共用）
    /// </summary>
    [ProcessBinding(typeof(CameraClip), PlayMode.EditorPreview)]
    [ProcessBinding(typeof(CameraClip), PlayMode.Runtime)]
    public class CameraProcess : ProcessBase<CameraClip>
    {
        public override void OnEnter()
        {
            // TODO: 摄像机效果开始
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
            // TODO: 更新摄像机（抖动、跟随等）
        }

        public override void OnExit()
        {
            // TODO: 重置摄像机
        }
    }
}
