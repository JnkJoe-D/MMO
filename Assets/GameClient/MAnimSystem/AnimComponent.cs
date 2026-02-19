using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections.Generic;
using System;

namespace Game.MAnimSystem
{
    /// <summary>
    /// MAnimSystem 的核心组件。
    /// 挂载在角色上，作为外部系统播放动画的主要入口。
    /// 负责管理 PlayableGraph 的生命周期以及多层动画层。
    /// 支持多层混合：AvatarMask、Additive 模式、层权重淡入淡出。
    /// 
    /// 设计说明：
    /// - 动画始终由 Unity Update 自动驱动。
    /// - Play: 播放动画（运行时和编辑器都需要）。
    /// - SetSpeed: 速度控制（用于帧同步场景）。
    /// - Evaluate: 编辑器预览专用，手动采样动画帧。
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimComponent : MonoBehaviour
    {
        /// <summary>
        /// 关联的 Animator 组件。
        /// </summary>
        public Animator Animator { get; private set; }

        /// <summary>
        /// 管理的 PlayableGraph 实例。
        /// </summary>
        public PlayableGraph Graph { get; private set; }

        /// <summary>
        /// 层混合器（用于多层混合）。
        /// </summary>
        private AnimationLayerMixerPlayable _layerMixer;

        /// <summary>
        /// 所有动画层列表。
        /// </summary>
        private List<AnimLayer> _layers = new List<AnimLayer>();
        /// <summary>
        /// 每层的速度倍率
        /// </summary>
        private Dictionary<int, double> _layerSpeeds = new Dictionary<int, double>();
        /// <summary>
        /// 获取指定索引的动画层（延迟创建）。
        /// </summary>
        /// <param name="index">层索引</param>
        /// <returns>动画层实例</returns>
        public AnimLayer this[int index] => GetLayer(index);

        /// <summary>
        /// 获取层总数。
        /// </summary>
        public int LayerCount => _layers.Count;

        /// <summary>
        /// 图是否已创建并初始化。
        /// </summary>
        private bool _isGraphCreated;

        /// <summary>
        /// 是否在 OnEnable 时自动初始化图。
        /// </summary>
        public bool PlayAutomatically = true;
        /// <summary>
        /// 组件是否已初始化
        /// </summary>
        private bool _isInitialized = false;
        public void Initialize()
        {
            if (_isInitialized) return;
            Animator = GetComponent<Animator>();
            _isInitialized = true;
        }
        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (PlayAutomatically)
                InitializeGraph();
        }

        private void OnDisable()
        {
            ClearPlayGraph();
        }

        private void Update() //更新权重，控制过渡
        {
            if (!_isGraphCreated) return;

            // 始终自动更新，由 Unity 驱动
            UpdateInternal(Time.deltaTime);
        }
        /// <summary>
        /// 内部更新逻辑,帮助在MonoUpdate中同步每层的播放速度（过渡）
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void UpdateInternal(float deltaTime)
        {
            for (int i = 0; i < _layers.Count; i++)
            {
                float layerDeltaTime = deltaTime;
                if (_layerSpeeds.TryGetValue(i, out double speed))
                {
                    _layers[i]?.SetSpeed((float)speed);
                    layerDeltaTime *= (float)speed; // 叠加速度控制
                }
                _layers[i]?.Update(layerDeltaTime);
            }
        }
        /// <summary>
        /// 外部手动更新逻辑（供技能编辑器预览使用）。
        /// 支持手动驱动权重计算和状态更新。
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public void ManualUpdate(float deltaTime)
        {
            if (!_isGraphCreated) return;
            ManualUpdateInternal(deltaTime);
        }
        /// <summary>
        /// 内部更新逻辑，deltatime已包含速度控制，并由顶层线性传递
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void ManualUpdateInternal(float deltaTime)
        {
            for (int i = 0; i < _layers.Count; i++)
            {
                _layers[i]?.Update(deltaTime);
            }
        }


        /// <summary>
        /// 设置动画播放速度。
        /// 用于帧同步场景下的速度控制。
        /// </summary>
        /// <param name="speedScale">速度缩放因子 (1.0 = 正常速度)</param>
        public void SetLayerSpeed(int layerIndex, float speedScale)
        {
            if (!_isGraphCreated) return;
            if (layerIndex < 0 || layerIndex >= _layers.Count) return;
            if(_layerSpeeds.ContainsKey(layerIndex))
            {
                _layerSpeeds[layerIndex] = speedScale;
            }
            else
            {
                _layerSpeeds.Add(layerIndex, speedScale);
            }
            _layers[layerIndex].SetSpeed(speedScale);
        }

        /// <summary>
        /// 初始化 PlayableGraph 和基础层。
        /// </summary>
        public void InitializeGraph()
        {
            if (_isGraphCreated) return;

            // 创建 Graph，名称方便调试器识别
            Graph = PlayableGraph.Create($"AnimComponent_{gameObject.name}");
            Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            // 创建层混合器
            _layerMixer = AnimationLayerMixerPlayable.Create(Graph, 1);

            // 创建基础层 (Layer 0)
            CreateLayer(0);

            // 将层混合器连接到 Animator 的 Output
            var output = AnimationPlayableOutput.Create(Graph, "Animation", Animator);
            output.SetSourcePlayable(_layerMixer);

            // 开始运行图
            Graph.Play();
            _isGraphCreated = true;
        }

        // --- 层管理 ---

        /// <summary>
        /// 获取或创建指定索引的动画层。
        /// 如果层不存在，会自动创建该层及之前的所有层。
        /// </summary>
        /// <param name="index">层索引</param>
        /// <returns>动画层实例</returns>
        public AnimLayer GetLayer(int index)
        {
            if (!_isGraphCreated) InitializeGraph();

            // 确保索引有效
            if (index < 0) return null;

            // 延迟创建层
            while (_layers.Count <= index)
            {
                CreateLayer(_layers.Count);
            }
            return _layers[index];
        }

        /// <summary>
        /// 创建指定索引的动画层。
        /// </summary>
        /// <param name="index">层索引</param>
        private void CreateLayer(int index)
        {
            if (index < 0) return;
            // 确保 LayerMixer 有足够的输入端口
            if (index >= _layerMixer.GetInputCount())
            {
                _layerMixer.SetInputCount(index + 1);
            }

            // 创建层
            var layer = new AnimLayer(Graph, index, _layerMixer);

            // 将层的 Mixer 连接到 LayerMixer
            Graph.Connect(layer.Mixer, 0, _layerMixer, index);

            // 添加到列表
            while (_layers.Count <= index)
            {
                _layers.Add(null);
            }
            _layers[index] = layer;
            _layerSpeeds.Add(index, 1.0);// 默认速度为 1.0
        }

        // --- 公共 API ---

        /// <summary>
        /// 播放一个动画片段（在基础层播放）。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <returns>创建并播放的 AnimState</returns>
        public AnimState Play(AnimationClip clip)
        {
            return Play(clip, 0.25f);
        }

        /// <summary>
        /// 播放一个动画片段，并指定过渡时间（在基础层播放）。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <param name="fadeDuration">过渡时长 (秒)</param>
        /// <returns>创建并播放的 AnimState</returns>
        public AnimState Play(AnimationClip clip, float fadeDuration)
        {
            return GetLayer(0).Play(clip, fadeDuration);
        }

        /// <summary>
        /// 在指定层播放一个动画片段。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <param name="layerIndex">层索引</param>
        /// <param name="fadeDuration">过渡时长 (秒)</param>
        /// <returns>创建并播放的 AnimState</returns>
        public AnimState Play(AnimationClip clip, int layerIndex, float fadeDuration = 0.25f)
        {
            return GetLayer(layerIndex).Play(clip, fadeDuration);
        }

        /// <summary>
        /// 播放任意 AnimState 节点 (如 MixerState)。
        /// 使用默认 0.25s 过渡。
        /// </summary>
        /// <param name="state">状态节点</param>
        /// <returns>播放的状态</returns>
        public AnimState Play(AnimState state)
        {
            return Play(state, 0.25f);
        }

        /// <summary>
        /// 播放任意 AnimState 节点，并指定过渡时间。
        /// </summary>
        /// <param name="state">状态节点</param>
        /// <param name="fadeDuration">过渡时长 (秒)</param>
        /// <returns>播放的状态</returns>
        public AnimState Play(AnimState state, float fadeDuration)
        {
            GetLayer(0).Play(state, fadeDuration);
            return state;
        }

        /// <summary>
        /// 在指定层播放任意 AnimState 节点。
        /// </summary>
        /// <param name="state">状态节点</param>
        /// <param name="layerIndex">层索引</param>
        /// <param name="fadeDuration">过渡时长 (秒)</param>
        /// <returns>播放的状态</returns>
        public AnimState Play(AnimState state, int layerIndex, float fadeDuration = 0.25f)
        {
            GetLayer(layerIndex).Play(state, fadeDuration);
            return state;
        }

        /// <summary>
        /// 交叉淡入淡出到指定状态 (Play 的别名)。
        /// </summary>
        /// <param name="state">目标状态</param>
        /// <param name="fadeDuration">淡入时长</param>
        public void CrossFade(AnimState state, float fadeDuration)
        {
            Play(state, fadeDuration);
        }

        // --- 采样功能 (用于编辑器预览) ---

        /// <summary>
        /// 采样当前动画到指定时间。
        /// 仅用于编辑器预览或时间轴拖拽。
        /// 运行时请勿调用此方法。
        /// </summary>
        /// <param name="time">目标时间（秒）</param>
        public void Evaluate(float time)
        {
            if (!_isGraphCreated) return;

            var state = GetLayer(0).GetCurrentState();
            if (state != null)
            {
                state.Time = time;
                Graph.Evaluate(0f);
            }
        }

        /// <summary>
        /// 采样指定动画片段到指定归一化时间。
        /// 会立即切换到该动画并采样。
        /// 仅用于编辑器预览。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <param name="normalizedTime">归一化时间 (0~1)</param>
        public void SampleClip(AnimationClip clip, float normalizedTime)
        {
            if (!_isGraphCreated || clip == null) return;

            var state = GetLayer(0).Play(clip, 0f);
            state.NormalizedTime = normalizedTime;
            Graph.Evaluate(0f);
        }

        // --- 状态查询 (便捷方法) ---

        /// <summary>
        /// 获取当前播放的状态（基础层）。
        /// </summary>
        /// <returns>当前状态</returns>
        public AnimState GetCurrentState()
        {
            return GetLayer(0).GetCurrentState();
        }

        /// <summary>
        /// 获取当前播放的动画片段（基础层）。
        /// </summary>
        /// <returns>当前动画片段</returns>
        public AnimationClip GetCurrentClip()
        {
            return GetLayer(0).GetCurrentClip();
        }

        /// <summary>
        /// 检查是否正在播放指定片段（基础层）。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <returns>是否正在播放</returns>
        public bool IsPlaying(AnimationClip clip)
        {
            return GetLayer(0).IsPlaying(clip);
        }

        /// <summary>
        /// 获取当前播放时间（基础层）。
        /// </summary>
        /// <returns>当前时间（秒）</returns>
        public float GetCurrentTime()
        {
            return GetLayer(0).GetCurrentTime();
        }

        /// <summary>
        /// 获取当前播放进度（基础层）。
        /// </summary>
        /// <returns>归一化进度 (0~1)</returns>
        public float GetCurrentProgress()
        {
            return GetLayer(0).GetCurrentProgress();
        }
        /// <summary>
        /// 
        /// </summary>
        public void ClearPlayGraph()
        {
            if (_isGraphCreated)
            {
                // 销毁所有层
                foreach (var layer in _layers)
                {
                    layer?.Destroy();
                }
                _layers.Clear();
                _layerSpeeds.Clear();
                // 销毁图，释放非托管内存
                Graph.Destroy();
                _isGraphCreated = false;
            }
            if(!Animator.isActiveAndEnabled)return;
            Animator.Rebind(); // 强制刷新 Animator 状态，避免残留影响
            Animator.Update(0f); // 立即应用状态重置
        }
         /// <summary>
        public void Log(string message)
        {
            Debug.Log($"[AnimComponent] {message}");
        }
        public AvatarMask GetLayerMask(int layer)
        {
            // if (layer < 0 || layer >= _layers.Count) return null;
            if (layer < 0) return null;
            return _layers[layer].Mask;
        }
        public void SetLayerMask(int layer, AvatarMask avatarMask)
        {
            // if (layer < 0 || layer >= _layers.Count) return;
            if (layer < 0) return;
            _layers[layer].Mask = avatarMask;
        }
    }
}
