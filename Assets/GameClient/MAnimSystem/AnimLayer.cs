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
        /// 层混合器引用（用于设置层的权重、Mask、Additive）。
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
            get
            {
                return _mask == null ? new AvatarMask() : _mask; // 创建默认遮罩，避免返回 null
            }
            set
            {
                _mask = value??new AvatarMask(); // 确保不为 null
                // 同步到 LayerMixer,设置本层的遮罩
                if (_layerMixer.IsValid() && _mask != null)
                {
                    _layerMixer.SetLayerMaskFromAvatarMask((uint)LayerIndex, _mask);
                }
            }
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
        public AnimBlendMode BlendMode { get; set; } = AnimBlendMode.Linear;
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
        /// AnimationClip 到 ClipState 的状态池。
        /// </summary>
        private Dictionary<AnimationClip, Stack<AnimState>> _statePool = new Dictionary<AnimationClip, Stack<AnimState>>();

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
            return Mask;
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
        public void Play(AnimState state, float fadeDuration, bool forceResetTime = false)
        {
            if (state == null) return;
            if (state == _targetState && forceResetTime)
            {
                // 彻底斩断该状态的内部 Playable 联系并重建，实现同实例自身的硬重置
                state.RebuildPlayable();
                state.Playable.SetDone(false); // 重置播放状态

                // 如果需要由 1.0 -> 1.0 的过程，这里不需要做任何淡入淡出，保持权重即可
                return;
            }

            // 1. 确保状态已连接到本层的 Mixer
            if (!IsStateConnected(state))
            {
                ConnectState(state);
            }

            // 打捞方案：检查新状态是否原本就在淡出队列中（比如频繁切回头）
            float salvagedWeight = 0f;
            bool wasFading = false;
            for (int i = _fadingStates.Count - 1; i >= 0; i--)
            {
                if (_fadingStates[i].State == state)
                {
                    salvagedWeight = _fadingStates[i].State.Weight;
                    wasFading = true;
                    _fadingStates.RemoveAt(i);
                }
            }
            
            // 安全机制：如果有残留同状态也一并扫清
            RemoveFromFadingStates(state);

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
            _targetState?.Clear(); //清空事件回调防止误触发
            _targetState = state;
            _fadeSpeed = 1.0f / Mathf.Max(fadeDuration, 0.001f);
            
            // 将进度设置到打捞的权重线上，避免瞬间降为 0 导致 T-Pose 或突变抽搐
            _targetFadeProgress = salvagedWeight;

            // 5. 核心防抖修复：如果是在短时间内被打崩又切回来，坚决重建时间轴！
            // 不然高频按键会不断看到第一帧的起步姿态！顺滑过渡过去即可。
            if (!wasFading || forceResetTime)
            {
                _targetState.RebuildPlayable();
            }
            
            if (_targetState.Playable.IsValid())
            {
                _targetState.Playable.SetDone(false);
            }
            // 继承打捞遗产，保证这 1 帧内的骨骼混合依然平稳输出
            _targetState.Weight = salvagedWeight;

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

                _targetState.OnFadeComplete?.Invoke(_targetState);
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
        /// 从淡出列表中移除指定状态（用于新目标排他）。
        /// </summary>
        /// <param name="state">要移除的状态</param>
        private void RemoveFromFadingStates(AnimState state)
        {
            if (state == null) return;
            for (int i = _fadingStates.Count - 1; i >= 0; i--)
            {
                if (_fadingStates[i].State == state)
                {
                    _fadingStates.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 快捷方法：直接播放一个 AnimationClip。
        /// 优先使用缓存中的状态，避免重复创建。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <param name="fadeDuration">过渡时长</param>
        /// <returns>ClipState 实例</returns>
        public AnimState Play(AnimationClip clip, float fadeDuration = 0.25f, bool forceResetTime = false)
        {
            if (clip == null) return null;

            AnimState state = null;
            if (_statePool.TryGetValue(clip, out var pool) && pool.Count > 0)
            {
                state = pool.Pop();
                // 重点：出池后执行重构，刷新底层 Playable 杜绝回溯
                state.RebuildPlayable();
            }

            if (state == null)
            {
                state = new AnimState(clip);
                state.Initialize(this, Graph);
                // 新建的第一拍也重构一下做基准保险
                state.RebuildPlayable();
            }

            Play(state, fadeDuration, forceResetTime);
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
            if (_targetState is AnimState clipState)
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

                float targetWeight;
                if(BlendMode == AnimBlendMode.SmoothStep)
                {
                    // 使用 SmoothStep 曲线调整权重
                    targetWeight = Mathf.SmoothStep(0f, 1f, _targetFadeProgress);
                }
                else
                {
                    // 线性过渡
                    targetWeight = _targetFadeProgress;
                }
                _targetState.Weight = targetWeight;

                if (_targetFadeProgress >= 1f)
                {
                    _targetState.OnFadeComplete?.Invoke(_targetState);
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
                
                // 目标状态排他：目标不能继续留在淡出列表里
                if (fs.State == _targetState)
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
        /// 权重归一化（强约束版）。
        /// 目标：尽量保证每帧 Mixer 输入总权重为 1，避免出现“总权重下陷”导致参考姿态闪现。
        /// 
        /// 设计原则：
        /// 1) 有目标状态时：目标权重优先，淡出状态按剩余空间（1-target）等比缩放。
        /// 2) 无目标状态时：淡出状态整体归一到 1（兜底，不让层完全失重）。
        /// 3) 淡出总权重过小（接近 0）时：直接将目标顶满并清理淡出残留，防止数值抖动。
        /// </summary>
        /// <param name="totalFadeOutWeight">当前帧所有淡出状态的权重和（淡出循环计算得到）</param>
        private void NormalizeWeights(float totalFadeOutWeight)
        {
            // 分支 A：无目标状态
            // 说明：理论上过渡过程应始终有 target，但在边界帧可能出现 target 为空且 fading 尚有残留。
            // 若不兜底，这一层会临时“失重”（总权重接近 0），从而暴露参考姿态。
            if (_targetState == null)
            {
                if (_fadingStates.Count == 0) return;

                // A1：有 fading 但总权重几乎为 0
                // 说明：这是浮点/时序边界帧。此时均分 1.0 到所有 fading，保证总权重回到 1。
                if (totalFadeOutWeight <= 0.001f)
                {
                    float evenWeight = 1f / _fadingStates.Count;
                    for (int i = 0; i < _fadingStates.Count; i++)
                    {
                        _fadingStates[i].State.Weight = evenWeight;
                    }
                    return;
                }

                // A2：有 fading 且总权重大于阈值
                // 说明：直接做标准归一化（每个 fading 权重乘同一缩放系数），使总和=1。
                float fillScale = 1f / totalFadeOutWeight;
                for (int i = 0; i < _fadingStates.Count; i++)
                {
                    _fadingStates[i].State.Weight *= fillScale;
                }
                return;
            }
            
            // 分支 B：有目标状态（常态）
            // 说明：目标权重由淡入逻辑给出，这里只负责让 fading 填满“剩余空间”。
            // 即强约束：sum(target + fading) == 1。
            float currentTargetWeight = Mathf.Clamp01(_targetState.Weight);

            // B1：没有 fading（说明过渡结束或被清理）
            // 说明：为了消除“目标尚未顶满但 fading 已无”的空窗，直接将目标拉满到 1。
            if (_fadingStates.Count == 0)
            {
                _targetState.Weight = 1f;
                return;
            }

            // B2：有 fading，但其总权重几乎为 0
            // 说明：此时 fading 对输出贡献可忽略，保留它们只会带来数值抖动与逻辑残留。
            // 处理：目标顶满，fading 清零并进入清理队列。
            if (totalFadeOutWeight <= 0.001f)
            {
                _targetState.Weight = 1f;
                for (int i = 0; i < _fadingStates.Count; i++)
                {
                    _fadingStates[i].State.Weight = 0f;
                    MarkForCleanup(_fadingStates[i].State);
                }
                _fadingStates.Clear();
                return;
            }

            // B3：常规过渡帧
            // 说明：remain 是目标以外可分配空间；按 fading 当前占比等比缩放。
            // 这样既保留 fading 间相对比例，又严格满足总和守恒。
            float remain = Mathf.Clamp01(1f - currentTargetWeight);
            float scale = remain / totalFadeOutWeight;
            for (int i = 0; i < _fadingStates.Count; i++)
            {
                _fadingStates[i].State.Weight *= scale;
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
        /// 清除所有状态池。
        /// </summary>
        public void ClearCache()
        {
            foreach (var kvp in _statePool)
            {
                foreach (var state in kvp.Value)
                {
                    if (IsStateConnected(state))
                    {
                        DisconnectState(state);
                    }
                    state.Destroy();
                }
            }
            _statePool.Clear();
        }

        // --- 状态清理管理 ---

        /// <summary>
        /// 标记状态为待清理或待回收。
        /// </summary>
        private void MarkForCleanup(AnimState state)
        {
            if (state == null) return;

            if (!_pendingCleanup.ContainsKey(state))
            {
                _pendingCleanup[state] = Time.time + CLEANUP_DELAY;
            }
        }

        /// <summary>
        /// 处理待清理状态队列 (将其推入对象池)。
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

                // 回收到池中
                if (state != null && state.Clip != null)
                {
                    ReturnToPool(state);
                }
                else
                {
                    DestroyState(state);
                }
            }
        }

        private void ReturnToPool(AnimState clipState)
        {
            clipState.Pause(); 
            clipState.Clear(); // 清理残留的回调事件
            
            if (!_statePool.TryGetValue(clipState.Clip, out var pool))
            {
                pool = new Stack<AnimState>();
                _statePool[clipState.Clip] = pool;
            }
            
            // 限制单片段的池容量上限，避免无限膨胀。超出则彻底拔根。
            if (pool.Count >= 5) 
            {
                DestroyState(clipState);
            }
            else
            {
                pool.Push(clipState);
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
