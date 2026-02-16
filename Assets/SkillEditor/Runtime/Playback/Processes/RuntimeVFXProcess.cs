namespace SkillEditor
{
    /// <summary>
    /// 运行时：VFX 片段 Process 骨架
    /// 具体实例化/回收逻辑由用户后续对接项目的特效系统
    /// </summary>
    [ProcessBinding(typeof(VFXClip), PlayMode.Runtime)]
    public class RuntimeVFXProcess : ProcessBase<VFXClip>
    {
        public override void OnEnter()
        {
            // TODO: 运行时特效生成
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
            // TODO: 位置跟随等
        }

        public override void OnExit()
        {
            // TODO: 回收特效实例
        }
    }
}
