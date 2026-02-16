using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace Game.MAnimSystem
{
    /// <summary>
    /// 单个动画片段 (AnimationClip) 的状态封装。
    /// </summary>
    public class ClipState : AnimState
    {
        /// <summary>
        /// 引用的动画片段资源。
        /// </summary>
        public AnimationClip Clip { get; private set; }

        /// <summary>
        /// 具体的 AnimationClipPlayable 实例。
        /// 这是主要的数据存储，_playableCache 是其缓存副本。
        /// </summary>
        private AnimationClipPlayable _clipPlayable;

        /// <summary>
        /// 构造一个新的 ClipState。
        /// </summary>
        /// <param name="clip">要播放的动画片段</param>
        public ClipState(AnimationClip clip)
        {
            Clip = clip;
        }

        /// <summary>
        /// 创建 AnimationClipPlayable。
        /// </summary>
        protected override Playable CreatePlayable(PlayableGraph graph)
        {
            _clipPlayable = AnimationClipPlayable.Create(graph, Clip);
            return _clipPlayable;
        }

        /// <summary>
        /// 获取动画片段的长度。若 Clip 为空则返回 0。
        /// </summary>
        public override float Length => Clip != null ? Clip.length : 0f;

        /// <summary>
        /// 是否循环播放。
        /// 默认情况下 AnimationClipPlayable 会遵循 AnimationClip.settings 中的设置。
        /// </summary>
        public override bool IsLooping
        {
            get => Clip != null && Clip.isLooping;
            set 
            {
                // AnimationClipPlayable 通常自动遵循 Clip 资产的设置。
                // 如果需要运行时强制覆盖循环模式，通常需要操作 Playable 底层的 SetWrapMode (Pre/PostExtrapolation)。
                // 为保持简洁，此处暂未实现强制覆盖逻辑。
            }
        }
        
        /// <summary>
        /// 辅助属性：检查动画是否已播放完毕 (非循环模式且时间超过长度)。
        /// </summary>
        public bool IsDone => !IsLooping && Time >= Length;
    }
}
