using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace Game.MAnimSystem
{
    public enum AnimBlendMode
    {
        Linear = 0,   // 线性混合，简单直接，但可能导致骨骼扭曲
        SmoothStep = 1 // 平滑混合，使用 S 形曲线过渡，减少骨骼扭曲
    }
    /// <summary>
    /// 管理单个动画层（例如：基础层、上半身层）。
    /// 负责在该层内部管理多个状态的混合、过渡 (Fade) 和生命周期。
    /// 支持中断过渡、状态缓存和自动清理。
    /// 支持多层混合：AvatarMask、Additive 模式、层权重淡入淡出。
    /// </summary>
    public class AnimLayer
    {
        /// <summary>
        /// 被中断状态的淡出速度倍率。
        /// </summary>
        private const float INTERRUPT_SPEED_MULTIPLIER = 2f;

        /// <summary>
        /// 状态缓存最大数量。
        /// </summary>
        private const int MAX_CACHE_SIZE = 32;

        /// <summary>
        /// 待清理状态的延迟时间（秒）。
        /// </summary>
        private const float CLEANUP_DELAY = 2f;

        /// <summary>
        /// 所属的 PlayableGraph 引用。
        /// </summary>
        public PlayableGraph Graph { get; private set; }

        /// <summary>
        /// 该层的总混合器 (AnimationMixerPlayable)。
        /// 所有该层管理的状态都会连接到这个 Mixer 的输入端口。
        /// </summary>
        public AnimationMixerPlayable Mixer { get; private set; }

        /// <summary>
        /// 该层的索引 (用于多层混合时的排序)。
        /// </summary>
        public int LayerIndex { get; private set; }

        // --- 层属性 (用于多层混合) ---

        /// <summary>
        /// 层混合器引用（用于设置层权重、Mask、Additive）。
        /// </summary>
        private AnimationLayerMixerPlayable _layerMixer;

        /// <summary>
        /// 层权重 (0 ~ 1)。
        /// </summary>
        private float _weight = 1f;

        /// <summary>
        /// 获取或设置层权重。
        /// </summary>
        public float Weight
        {
            get => _weight;
            set => SetWeight(value);
        }
        public double PlaybackSpeed
        {
            get => Mixer.GetSpeed();
            set => Mixer.SetSpeed(value);
        }
        /// <summary>
        /// 骨骼遮罩。
        /// </summary>
        private AvatarMask _mask;

        /// <summary>
        /// 获取或设置骨骼遮罩。
        /// </summary>
        public AvatarMask Mask
        {
            get => _mask == null ? new AvatarMask() : _mask; // 创建默认遮罩，避免返回 null
            set => SetMask(value?? new AvatarMask()); // 确保不设置 null
        }

        /// <summary>
        /// 是否为叠加模式。
        /// </summary>
        private bool _isAdditive = false;

        /// <summary>
        /// 获取或设置是否为叠加模式。
        /// true: 叠加到前面层的动画上
        /// false: 覆盖前面层的动画
        /// </summary>
        public bool IsAdditive
        {
            get => _isAdditive;
            set => SetAdditive(value);
        }
        /// <summary>
        /// 动画混合模式。
        /// </summary>
        // 为了解决线性混合导致的骨骼扭曲问题，默认使用 SmoothStep 平滑混合。
        public AnimBlendMode BlendMode { get; set; } = AnimBlendMode.SmoothStep;
        // --- 层淡入淡出 ---

        /// <summary>
        /// 层淡入淡出的目标权重。
        /// </summary>
        private float _targetLayerWeight;

        /// <summary>
        /// 层淡入淡出的速度。
        /// </summary>
        private float _layerFadeSpeed;

        /// <summary>
        /// 是否正在进行层淡入淡出。
        /// </summary>
        private bool _isLayerFading;

        /// <summary>
        /// 该层管理的所有动画状态列表。
        /// </summary>
        private List<AnimState> _states = new List<AnimState>();

        /// <summary>
        /// 可重用的空闲输入端口列表 (避免端口无限增长)。
        /// </summary>
        private List<int> _freePorts = new List<int>();

        // --- 过渡 (Fade) 相关状态 ---

        /// <summary>
        /// 淡出状态信息结构体。
        /// </summary>
        private struct FadingState
        {
            public AnimState State;
            public float FadeSpeed;
            public bool IsInterrupted;
        }

        /// <summary>
        /// 当前目标状态（正在淡入的状态）。
        /// </summary>
        private AnimState _targetState;

        /// <summary>
        /// 目标状态的淡入进度 (0 ~ 1)。
        /// </summary>
        private float _targetFadeProgress;

        /// <summary>
        /// 当前过渡速度 (1.0 / duration)。
        /// </summary>
        private float _fadeSpeed;

        /// <summary>
        /// 所有正在淡出的状态列表。
        /// </summary>
        private List<FadingState> _fadingStates = new List<FadingState>();

        // --- 状态缓存 ---

        /// <summary>
        /// AnimationClip 到 ClipState 的缓存映射。
        /// </summary>
        private Dictionary<AnimationClip, ClipState> _clipStateCache = new Dictionary<AnimationClip, ClipState>();

        // --- 状态清理 ---

        /// <summary>
        /// 待清理状态及其清理时间的映射。
        /// </summary>
        private Dictionary<AnimState, float> _pendingCleanup = new Dictionary<AnimState, float>();

        /// <summary>
        /// 构造一个新的动画层。
        /// </summary>
        /// <param name="graph">所属图</param>
        /// <param name="layerIndex">层级索引</param>
        /// <param name="layerMixer">层混合器（可选，用于多层混合）</param>
        public AnimLayer(PlayableGraph graph, int layerIndex, AnimationLayerMixerPlayable layerMixer = default)
        {
            Graph = graph;
            LayerIndex = layerIndex;
            _layerMixer = layerMixer;
            Mixer = AnimationMixerPlayable.Create(graph, 0);

            // 设置初始层权重
            if (_layerMixer.IsValid())
            {
                _layerMixer.SetInputWeight((int)layerIndex, _weight);
            }
        }

        // --- 层属性设置方法 ---

        /// <summary>
        /// 设置层权重。
        /// </summary>
        /// <param name="weight">权重值 (0 ~ 1)</param>
        public void SetWeight(float weight)
        {
            _weight = Mathf.Clamp01(weight);

            // 同步到 LayerMixer
            if (_layerMixer.IsValid())
            {
                _layerMixer.SetInputWeight((int)LayerIndex, _weight);
            }
        }
        public AvatarMask GetMask()
        {
            return _mask;
        }
        /// <summary>
        /// 设置骨骼遮罩。
        /// </summary>
        /// <param name="mask">AvatarMask 实例</param>
        public void SetMask(AvatarMask mask)
        {
            _mask = mask;

            // 同步到 LayerMixer
            if (_layerMixer.IsValid() && mask != null)
            {
                _layerMixer.SetLayerMaskFromAvatarMask((uint)LayerIndex, mask);
            }
        }

        /// <summary>
        /// 设置叠加模式。
        /// </summary>
        /// <param name="additive">true 为叠加模式，false 为覆盖模式</param>
        public void SetAdditive(bool additive)
        {
            _isAdditive = additive;

            // 同步到 LayerMixer
            if (_layerMixer.IsValid())
            {
                _layerMixer.SetLayerAdditive((uint)LayerIndex, additive);
            }
        }

        // --- 层淡入淡出 ---

        /// <summary>
        /// 开始层权重淡入淡出。
        /// </summary>
        /// <param name="targetWeight">目标权重 (0 ~ 1)</param>
        /// <param name="duration">过渡时长 (秒)</param>
        public void StartFade(float targetWeight, float duration)
        {
            targetWeight = Mathf.Clamp01(targetWeight);

            // 如果目标权重与当前权重相同，无需淡入淡出
            if (Mathf.Abs(targetWeight - _weight) < 0.001f)
            {
                _isLayerFading = false;
                return;
            }

            // 如果 duration 为 0，立即设置权重
            if (duration <= 0f)
            {
                SetWeight(targetWeight);
                _isLayerFading = false;
                return;
            }

            _targetLayerWeight = targetWeight;
            _layerFadeSpeed = 1f / duration;
            _isLayerFading = true;
        }

        /// <summary>
        /// 更新层淡入淡出。
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void UpdateLayerFade(float deltaTime)
        {
            if (!_isLayerFading) return;

            // 计算新权重 (线性变化)
            // 层Fade通常用于开关层，线性即可，若需平滑也可应用但暂时保留线性以确保可控
            float direction = _targetLayerWeight > _weight ? 1f : -1f;
            float newWeight = _weight + direction * _layerFadeSpeed * deltaTime;

            // 检查是否完成
            if ((direction > 0 && newWeight >= _targetLayerWeight) ||
                (direction < 0 && newWeight <= _targetLayerWeight))
            {
                SetWeight(_targetLayerWeight);
                _isLayerFading = false;
            }
            else
            {
                SetWeight(newWeight);
            }
        }

        // --- 核心 API ---

        /// <summary>
        /// 播放一个状态，使用默认过渡时间 (0.25s)。
        /// </summary>
        /// <param name="state">目标状态</param>
        public void Play(AnimState state)
        {
            Play(state, 0.25f);
        }

        /// <summary>
        /// 播放一个状态，并指定过渡时间。
        /// 支持中断正在进行的过渡，确保平滑切换。
        /// </summary>
        /// <param name="state">目标状态</param>
        /// <param name="fadeDuration">过渡时长 (秒)</param>
        public void Play(AnimState state, float fadeDuration)
        {
            if (state == null) return;
            if (state == _targetState) return;

            // 1. 确保状态已连接到本层的 Mixer
            if (!IsStateConnected(state))
            {
                ConnectState(state);
            }

            // 2. 将当前目标状态加入淡出列表（如果尚未存在）
            if (_targetState != null)
            {
                AddToFadingStates(_targetState, _fadeSpeed);
            }

            // 3. 所有正在淡出的状态标记为"被中断"并加速
            for (int i = 0; i < _fadingStates.Count; i++)
            {
                var fs = _fadingStates[i];
                if (!fs.IsInterrupted)
                {
                    fs.IsInterrupted = true;
                    fs.FadeSpeed *= INTERRUPT_SPEED_MULTIPLIER;
                    _fadingStates[i] = fs;
                }
            }

            // 4. 设置新目标状态
            _targetState = state;
            _fadeSpeed = 1.0f / Mathf.Max(fadeDuration, 0.001f);
            _targetFadeProgress = 0f;

            // 5. 重置目标状态（从头开始播放）
            _targetState.Time = 0;
            if (_targetState.Playable.IsValid())
            {
                _targetState.Playable.SetDone(false);
            }
            _targetState.Weight = 0f;

            // 6. 如果是瞬间切换
            if (fadeDuration <= 0)
            {
                _targetState.Weight = 1f;
                _targetFadeProgress = 1f;

                // 清理所有淡出状态的权重
                foreach (var fs in _fadingStates)
                {
                    fs.State.Weight = 0f;
                }
                _fadingStates.Clear();

                _targetState.OnFadeComplete?.Invoke();
            }
        }

        /// <summary>
        /// 将状态添加到淡出列表（如果已存在则更新速度）。
        /// </summary>
        /// <param name="state">要淡出的状态</param>
        /// <param name="fadeSpeed">淡出速度</param>
        private void AddToFadingStates(AnimState state, float fadeSpeed)
        {
            // 检查是否已在淡出列表中
            for (int i = 0; i < _fadingStates.Count; i++)
            {
                var fs = _fadingStates[i];
                if (fs.State == state)
                {
                    // 已存在，更新速度（取较大值，确保更快淡出）
                    fs.FadeSpeed = Mathf.Max(fs.FadeSpeed, fadeSpeed);
                    fs.IsInterrupted = true;
                    _fadingStates[i] = fs;
                    return;
                }
            }

            // 不存在，添加新记录
            _fadingStates.Add(new FadingState
            {
                State = state,
                FadeSpeed = fadeSpeed,
                IsInterrupted = true
            });
        }

        /// <summary>
        /// 快捷方法：直接播放一个 AnimationClip。
        /// 优先使用缓存中的状态，避免重复创建。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <param name="fadeDuration">过渡时长</param>
        /// <returns>ClipState 实例</returns>
        public ClipState Play(AnimationClip clip, float fadeDuration = 0.25f)
        {
            if (clip == null) return null;

            // 优先使用缓存
            if (!_clipStateCache.TryGetValue(clip, out var state))
            {
                state = new ClipState(clip);
                state.Initialize(this, Graph);

                // 限制缓存大小
                if (_clipStateCache.Count >= MAX_CACHE_SIZE)
                {
                    CleanupOldestCachedState();
                }

                _clipStateCache[clip] = state;
            }

            Play(state, fadeDuration);
            return state;
        }

        // --- 状态查询 ---

        /// <summary>
        /// 获取当前播放的状态。
        /// </summary>
        /// <returns>当前目标状态，无则返回 null</returns>
        public AnimState GetCurrentState()
        {
            return _targetState;
        }

        /// <summary>
        /// 获取当前播放的动画片段。
        /// </summary>
        /// <returns>当前动画片段，无则返回 null</returns>
        public AnimationClip GetCurrentClip()
        {
            if (_targetState is ClipState clipState)
            {
                return clipState.Clip;
            }
            return null;
        }

        /// <summary>
        /// 检查是否正在播放指定片段。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <returns>是否正在播放</returns>
        public bool IsPlaying(AnimationClip clip)
        {
            return GetCurrentClip() == clip;
        }

        /// <summary>
        /// 获取当前播放时间。
        /// </summary>
        /// <returns>当前时间（秒）</returns>
        public float GetCurrentTime()
        {
            return _targetState?.Time ?? 0f;
        }

        /// <summary>
        /// 获取当前播放进度。
        /// </summary>
        /// <returns>归一化进度 (0~1)</returns>
        public float GetCurrentProgress()
        {
            return _targetState?.NormalizedTime ?? 0f;
        }

        /// <summary>
        /// 设置当前动画的播放速度。
        /// </summary>
        /// <param name="speed">速度因子 (1.0 = 正常速度)</param>
        public void SetSpeed(float speed)
        {
            PlaybackSpeed = speed;
        }

        // --- 内部更新 (由 Component 驱动) ---
        public void Update(float deltaTime)
        {
            OnUpdate(deltaTime);
        }
        /// <summary>
        /// 每帧更新逻辑。负责处理权重淡入淡出和状态更新。
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void OnUpdate(float deltaTime)
        {
            // 0. 更新层淡入淡出
            UpdateLayerFade(deltaTime);

            // 1. 更新目标状态淡入
            if (_targetState != null && _targetFadeProgress < 1f)
            {
                _targetFadeProgress += _fadeSpeed * deltaTime;
                _targetFadeProgress = Mathf.Clamp01(_targetFadeProgress);

                if(BlendMode == AnimBlendMode.SmoothStep)
                {
                    // 使用 SmoothStep 曲线调整权重
                    _targetState.Weight = Mathf.SmoothStep(0f, 1f, _targetFadeProgress);
                }
                else
                {
                    // 线性过渡
                    _targetState.Weight = _targetFadeProgress;
                }
                _targetState.Weight = _targetFadeProgress;

                if (_targetFadeProgress >= 1f)
                {
                    _targetState.OnFadeComplete?.Invoke();
                }
            }

            // 2. 更新所有淡出状态
            float totalFadeOutWeight = 0f;
            for (int i = _fadingStates.Count - 1; i >= 0; i--)
            {
                var fs = _fadingStates[i];
                
                // 安全检查：确保状态有效
                if (fs.State == null)
                {
                    _fadingStates.RemoveAt(i);
                    continue;
                }

                // 淡出状态我们也应用同样的混合曲线反向逻辑吗？
                // 传统的 CrossFade 通常要求 Sum(Weights) = 1。
                // 如果 Target 使用 SmoothStep (S形)，FadeOut 最好也是 (1 - SmoothStep)。
                // 这里我们简化处理：FadeOut 依旧线性衰减，最后通过 NormalizeWeights 归一化来强制互补。
                // 这样 FadeOut 会自动“填充” FadeIn 留下的空间。
                
                float newWeight = fs.State.Weight - fs.FadeSpeed * deltaTime;

                if (newWeight <= 0f)
                {
                    fs.State.Weight = 0f;
                    MarkForCleanup(fs.State);
                    _fadingStates.RemoveAt(i);
                }
                else
                {
                    fs.State.Weight = newWeight;
                    totalFadeOutWeight += newWeight;
                }
            }

            // 3. 权重归一化（确保总和为1）
            // 注意：如果使用了 SmoothStep，TargetWeight 会非线性变化。
            // 我们通过归一化 FadeOut 状态来配合 TargetWeight。
            NormalizeWeights(totalFadeOutWeight);

            // 4. 更新所有活动状态的逻辑 (检测播放结束事件等)
            for (int i = 0; i < _states.Count; i++)
            {
                var s = _states[i];
                if (s.Weight > 0.001f || s == _targetState)
                {
                    s.OnUpdate(deltaTime);
                }
            }

            // 5. 处理待清理状态
            ProcessCleanupQueue();
        }

        /// <summary>
        /// 权重归一化，确保所有状态权重之和为 1.0。
        /// </summary>
        /// <param name="totalFadeOutWeight">所有淡出状态的总权重</param>
        private void NormalizeWeights(float totalFadeOutWeight)
        {
            // 获取当前目标状态的实际应用权重 (已经经过 SmoothStep 处理)
            float currentTargetWeight = _targetState != null ? _targetState.Weight : 0f;
            
            float totalWeight = currentTargetWeight + totalFadeOutWeight;

            // 处理权重总和异常的情况
            if (totalWeight < 0.001f)
            {
                // 总权重接近 0，强制将目标状态设为 1
                if (_targetState != null)
                {
                    _targetState.Weight = 1f;
                    // _targetFadeProgress = 1f; // 不修改进度，只修权重
                }
                return;
            }

            if (Mathf.Abs(totalWeight - 1f) > 0.001f && totalFadeOutWeight > 0.001f)
            {
                // 权重总和不为 1，需要归一化
                // 目标权重是"主导"的，我们缩放 FadeOut 权重来适配它
                float scale = (1f - currentTargetWeight) / totalFadeOutWeight;// 使用实际权重计算剩余空间
                for (int i = 0; i < _fadingStates.Count; i++)
                {
                    var fs = _fadingStates[i];
                    fs.State.Weight *= scale;
                }
            }
        }

        // --- 图管理 ---

        /// <summary>
        /// 检查状态是否已经正确连接到了本层。
        /// </summary>
        private bool IsStateConnected(AnimState state)
        {
            return _states.Contains(state) && state.PortIndex != -1;
        }

        /// <summary>
        /// 将状态连接到 Mixer 的一个可用端口上。
        /// </summary>
        private void ConnectState(AnimState state)
        {
            // 寻找空闲端口或扩展新端口
            int port = -1;
            if (_freePorts.Count > 0)
            {
                port = _freePorts[_freePorts.Count - 1];
                _freePorts.RemoveAt(_freePorts.Count - 1);
            }
            else
            {
                port = Mixer.GetInputCount();
                Mixer.SetInputCount(port + 1);
            }

            // 执行图连接
            Graph.Connect(state.Playable, 0, Mixer, port);
            state.ConnectToLayer(port);
            state.Weight = 0f;

            _states.Add(state);
        }

        /// <summary>
        /// 获取指定端口的当前权重。
        /// </summary>
        public float GetInputWeight(int portIndex)
        {
            if (portIndex >= 0 && portIndex < Mixer.GetInputCount())
                return Mixer.GetInputWeight(portIndex);
            return 0f;
        }

        /// <summary>
        /// 设置指定端口的权重。
        /// </summary>
        public void SetInputWeight(int portIndex, float weight)
        {
            if (portIndex >= 0 && portIndex < Mixer.GetInputCount())
            {
                Mixer.SetInputWeight(portIndex, Mathf.Clamp01(weight));
            }
        }

        // --- 状态缓存管理 ---

        /// <summary>
        /// 清除所有状态缓存。
        /// </summary>
        public void ClearCache()
        {
            foreach (var kvp in _clipStateCache)
            {
                if (IsStateConnected(kvp.Value))
                {
                    DisconnectState(kvp.Value);
                }
                kvp.Value.Destroy();
            }
            _clipStateCache.Clear();
        }

        /// <summary>
        /// 清理最久未使用的缓存状态。
        /// 简单实现：移除第一个非当前播放的状态。
        /// </summary>
        private void CleanupOldestCachedState()
        {
            AnimState toRemove = null;
            AnimationClip clipToRemove = null;

            foreach (var kvp in _clipStateCache)
            {
                if (kvp.Value != _targetState && kvp.Value.Weight < 0.001f)
                {
                    toRemove = kvp.Value;
                    clipToRemove = kvp.Key;
                    break;
                }
            }

            if (toRemove != null)
            {
                if (IsStateConnected(toRemove))
                {
                    DisconnectState(toRemove);
                }
                toRemove.Destroy();
                _clipStateCache.Remove(clipToRemove);
            }
        }

        // --- 状态清理管理 ---

        /// <summary>
        /// 标记状态为待清理。
        /// </summary>
        private void MarkForCleanup(AnimState state)
        {
            if (state == null) return;

            // 缓存的状态不清理
            if (state is ClipState clipState && _clipStateCache.ContainsValue(clipState))
            {
                return;
            }

            if (!_pendingCleanup.ContainsKey(state))
            {
                _pendingCleanup[state] = Time.time + CLEANUP_DELAY;
            }
        }

        /// <summary>
        /// 处理待清理状态队列。
        /// </summary>
        private void ProcessCleanupQueue()
        {
            if (_pendingCleanup.Count == 0) return;

            float currentTime = Time.time;
            var toRemove = new List<AnimState>();

            foreach (var kvp in _pendingCleanup)
            {
                if (currentTime >= kvp.Value)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var state in toRemove)
            {
                _pendingCleanup.Remove(state);
                DestroyState(state);
            }
        }

        /// <summary>
        /// 断开状态与 Mixer 的连接。
        /// </summary>
        private void DisconnectState(AnimState state)
        {
            if (state.PortIndex >= 0)
            {
                Graph.Disconnect(Mixer, state.PortIndex);
                _freePorts.Add(state.PortIndex);
                state.ConnectToLayer(-1);
            }
            _states.Remove(state);
        }

        /// <summary>
        /// 销毁状态。
        /// </summary>
        private void DestroyState(AnimState state)
        {
            if (state == null) return;

            DisconnectState(state);
            state.Destroy();
        }

        /// <summary>
        /// 销毁层及其管理的所有状态。
        /// </summary>
        public void Destroy()
        {
            ClearCache();

            if (Mixer.IsValid()) Mixer.Destroy();

            foreach (var s in _states) s.Destroy();
            _states.Clear();
            _fadingStates.Clear();
            _pendingCleanup.Clear();
            _freePorts.Clear();
        }
    }
}
