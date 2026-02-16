namespace SkillEditor
{
    /// <summary>
    /// 伤害片段 Process（编辑器和运行时共用）
    /// </summary>
    [ProcessBinding(typeof(DamageClip), PlayMode.EditorPreview)]
    [ProcessBinding(typeof(DamageClip), PlayMode.Runtime)]
    public class DamageProcess : ProcessBase<DamageClip>
    {
        public override void OnEnter()
        {
            // TODO: 触发伤害检测逻辑
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
            // TODO: 持续伤害区域等
        }

        public override void OnExit()
        {
            // TODO: 结束伤害检测
        }
    }
}
