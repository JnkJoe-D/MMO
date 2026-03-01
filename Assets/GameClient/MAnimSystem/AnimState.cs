using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace Game.MAnimSystem
{
    /// <summary>
    /// 单个动画片段 (AnimationClip) 的状态封装。
    /// </summary>
    public class AnimState : StateBase
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
        /// 构造一个新的 State。
        /// </summary>
        /// <param name="clip">要播放的动画片段</param>
        private float _cachedLength;
        private bool _cachedIsLooping;

        /// <summary>
        /// 构造一个新的 ClipState。
        /// </summary>
        /// <param name="clip">要播放的动画片段</param>
        public AnimState(AnimationClip clip)
        {
            Clip = clip;
            if (Clip != null)
            {
                _cachedLength = Clip.length;
                _cachedIsLooping = Clip.isLooping;
            }
        }

        /// <summary>
        /// 创建 AnimationClipPlayable。
        /// </summary>
        protected override Playable CreatePlayable(PlayableGraph graph)
        {
            if (Clip == null) return Playable.Null;
            _clipPlayable = AnimationClipPlayable.Create(graph, Clip);
            return _clipPlayable;
        }

        /// <summary>
        /// 获取动画片段的长度。
        /// </summary>
        public float Length => _cachedLength;

        /// <summary>
        /// 是否循环播放。
        /// </summary>
        public bool IsLooping
        {
            get => _cachedIsLooping;
            set => _cachedIsLooping = value;
        }

        public float NormalizedTime
        {
            get
            {
                float len = Length;
                return len > 0 ? Mathf.Clamp01(Time / len) : 0f;
            }
            set
            {
                float len = Length;
                if (len > 0)
                {
                    Time = Mathf.Clamp01(value) * len;
                }
            }
        }
        
        /// <summary>
        /// 辅助属性：检查动画是否已播放完毕 (非循环模式且时间超过长度)。
        /// </summary>
        public bool IsDone => !IsLooping && Time >= Length;

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            if (IsDone)
            {
                if (OnEnd != null)
                {
                    OnEnd.Invoke(this);
                    OnEnd = null;
                }
            }
        }
    }
}
