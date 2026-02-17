using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections.Generic;

namespace Game.MAnimSystem
{
    /// <summary>
    /// 所有可播放节点（Clip, Mixer, BlendTree等）的抽象基类。
    /// 封装了 Playable 的生命周期、权重控制和时间管理。
    /// </summary>
    public abstract class AnimState
    {
        /// <summary>
        /// 底层 Playable 的缓存引用。
        /// 提供统一的 Playable 访问接口。
        /// 注意：子类应维护自己的具体类型 Playable 字段作为主存储。
        /// </summary>
        protected Playable _playableCache;

        /// <summary>
        /// 获取底层 Playable 对象（基类视图）。
        /// </summary>
        public Playable Playable => _playableCache;

        /// <summary>
        /// 该状态所属的动画层。
        /// </summary>
        public AnimLayer ParentLayer { get; private set; }

        /// <summary>
        /// 该状态在父级 Mixer 中的输入端口索引。
        /// </summary>
        public int PortIndex { get; private set; } = -1;

        /// <summary>
        /// 播放完成事件 (当 Time >= Length 时触发)。
        /// 注意：循环动画通常不会触发此事件，除非手动调用。
        /// </summary>
        public System.Action OnEnd;

        /// <summary>
        /// 过渡完成事件 (当权重达到 1.0 时触发)。
        /// 表示该状态已完全进入。
        /// </summary>
        public System.Action OnFadeComplete;
        /// <summary>
        /// 自定义事件调度表，允许在特定时间点触发回调。
        /// </summary>
        private Dictionary<float, System.Action> _scheduledEvents = new Dictionary<float, System.Action>();

        /// <summary>
        /// 获取或设置该状态的权重 (0.0 ~ 1.0)。
        /// 修改此值会直接设置到底层的 Mixer 输入端口上。
        /// </summary>
        public float Weight
        {
            get => _playableCache.IsValid() && ParentLayer != null ? ParentLayer.GetInputWeight(PortIndex) : 0f;
            set
            {
                if (_playableCache.IsValid() && ParentLayer != null)
                {
                    ParentLayer.SetInputWeight(PortIndex, value);
                }
            }
        }

        /// <summary>
        /// 获取或设置播放速度。
        /// </summary>
        public float Speed
        {
            get => _playableCache.IsValid() ? (float)_playableCache.GetSpeed() : 0f;
            set
            {
                if (_playableCache.IsValid()) _playableCache.SetSpeed(value);
            }
        }

        /// <summary>
        /// 获取或设置当前播放时间 (秒)。
        /// </summary>
        public virtual float Time
        {
            get => _playableCache.IsValid() ? (float)_playableCache.GetTime() : 0f;
            set
            {
                if (_playableCache.IsValid()) _playableCache.SetTime(value);
            }
        }

        /// <summary>
        /// 动画的总长度 (秒)。
        /// </summary>
        public abstract float Length { get; }

        /// <summary>
        /// 是否循环播放。
        /// </summary>
        public virtual bool IsLooping { get; set; }

        /// <summary>
        /// 获取或设置归一化播放时间 (0.0 ~ 1.0)。
        /// 表示当前播放进度占动画总长度的比例。
        /// </summary>
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
        /// 是否暂停播放。
        /// 设置为 true 时速度设为 0，设置为 false 时恢复为 1。
        /// </summary>
        public bool IsPaused
        {
            get => Speed == 0f;
            set => Speed = value ? 0f : 1f;
        }

        /// <summary>
        /// 暂停播放。
        /// </summary>
        public void Pause()
        {
            Speed = 0f;
        }

        /// <summary>
        /// 恢复播放（速度设为 1）。
        /// </summary>
        public void Resume()
        {
            Speed = 1f;
        }

        /// <summary>
        /// 初始化状态，创建 Playable 并绑定到层级。
        /// </summary>
        /// <param name="layer">所属的动画层</param>
        /// <param name="graph">所属的 PlayableGraph</param>
        public void Initialize(AnimLayer layer, PlayableGraph graph)
        {
            ParentLayer = layer;
            _playableCache = CreatePlayable(graph);
            OnInitialized();
        }

        /// <summary>
        /// 创建底层的 Playable 实例。由子类实现具体类型 (AnimationClipPlayable, AnimationMixerPlayable 等)。
        /// </summary>
        /// <param name="graph">PlayableGraph</param>
        /// <returns>创建的 Playable</returns>
        protected abstract Playable CreatePlayable(PlayableGraph graph);

        /// <summary>
        /// 初始化完成后的回调，用于子类进行进一步设置。
        /// </summary>
        protected virtual void OnInitialized() { }

        /// <summary>
        /// 将此状态连接到父层的指定端口。
        /// </summary>
        /// <param name="portIndex">端口索引</param>
        public void ConnectToLayer(int portIndex)
        {
            PortIndex = portIndex;
            // 注意：实际的 Graph 连接逻辑由 Layer 处理，确保 Mixer 调用正确的 Connect 方法
        }

        /// <summary>
        /// 每帧更新逻辑。
        /// 主要用于触发事件 (如 OnEnd) 或更新自定义逻辑。
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public virtual void OnUpdate(float deltaTime)
        {
            // 简单的播放结束检测
            if (!IsLooping && Length > 0 && Time >= Length)
            {
                // 防止重复触发，通常由外部管理器处理，这里作为自检
                if (OnEnd != null)
                {
                    OnEnd.Invoke();
                    OnEnd = null; // 触发一次后清空，避免反复触发
                }
            }
            
            // 检查并触发自定义事件
            foreach (var kvp in _scheduledEvents)
            {
                if (Time >= kvp.Key && kvp.Key >= Time - deltaTime) // 确保事件只触发一次
                {
                    kvp.Value?.Invoke();
                    _scheduledEvents.Remove(kvp.Key);
                    break;
                }
            }
        }
        public void AddScheduledEvent(float triggerTime, System.Action callback)
        {
            if (triggerTime < 0) return; // 不允许负时间
            if (_scheduledEvents.ContainsKey(triggerTime))
            {
                _scheduledEvents[triggerTime] += callback;
            }
            else
            {
                _scheduledEvents[triggerTime] = callback;
            }
        }
        public void RemoveScheduledEvent(float triggerTime, System.Action callback)
        {
            if (_scheduledEvents.ContainsKey(triggerTime))
            {
                _scheduledEvents[triggerTime] -= callback;
                if (_scheduledEvents[triggerTime] == null)
                {
                    _scheduledEvents.Remove(triggerTime);
                }
            }
        }
        public void RemoveScheduledEvents(float triggerTime)
        {
            if (_scheduledEvents.ContainsKey(triggerTime))
            {
                _scheduledEvents.Remove(triggerTime);
            }
        }
        /// <summary>
        /// 销毁状态，清理底层的 Playable。
        /// </summary>
        public virtual void Destroy()
        {
            if (_playableCache.IsValid())
            {
                _playableCache.Destroy();
            }
            OnEnd = null;
            OnFadeComplete = null;
            _scheduledEvents.Clear();
        }
    }
}
