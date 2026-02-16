using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 技能编辑器事件系统 (EventBus)
    /// </summary>
    public class SkillEditorEvents
    {
        // 选中状态改变
        public Action OnSelectionChanged;
        
        // 数据模型改变 (增删、重构)
        public Action OnTimelineDataModified;

        /// <summary>
        /// 触发界面重绘请求
        /// </summary>
        public Action OnRepaintRequest;

        /// <summary>
        /// 触发通知：数据已修改，需要刷新所有视图并标记脏
        /// </summary>
        public void NotifyDataChanged()
        {
            OnTimelineDataModified?.Invoke();
            OnRepaintRequest?.Invoke();
        }

        /// <summary>
        /// 触发通知：选中项发生变化
        /// </summary>
        public void NotifySelectionChanged()
        {
            OnSelectionChanged?.Invoke();
            OnRepaintRequest?.Invoke();
        }
    }
}
