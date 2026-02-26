using UnityEngine;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 编辑器模式下的事件发送预览。
    /// 仅在控制台打印日志，帮助策划/程序确认事件的派发时机与参数。
    /// </summary>
    [ProcessBinding(typeof(EventClip), PlayMode.EditorPreview)]
    public class EditorEventProcess : ProcessBase<EventClip>
    {
        public override void OnEnter()
        {
            Debug.Log($"[SkillEditor Preview] <color=cyan>Event Dispatched!</color> Name: {clip.eventName}");
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
            // Event 是瞬发逻辑，无需 Update
        }
    }
}
