using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace Game.MAnimSystem
{
    /// <summary>
    /// 混合器状态基类。
    /// 这是一个特殊的 AnimState，它内部管理了一组子状态 (Children)，并将它们混合输出。
    /// 类似于 AnimLayer 的功能，但 MixerState 本身也是一个节点，可以嵌套在其他 Layer 或 Mixer 中。
    /// </summary>
    public class MixerState : AnimState
    {
        /// <summary>
        /// 具体的 AnimationMixerPlayable 实例。
        /// 这是主要的数据存储，_playableCache 是其缓存副本。
        /// </summary>
        protected AnimationMixerPlayable _mixerPlayable;

        /// <summary>
        /// 该混合器管理的所有子状态列表。
        /// </summary>
        protected List<AnimState> _children = new List<AnimState>();

        /// <summary>
        /// PlayableGraph 的引用缓存，用于添加子节点时创建 Playable。
        /// </summary>
        protected PlayableGraph _graph; 

        /// <summary>
        /// 创建 Mixer Playable。
        /// </summary>
        protected override Playable CreatePlayable(PlayableGraph graph)
        {
            _graph = graph;
            _mixerPlayable = AnimationMixerPlayable.Create(graph, 0);
            return _mixerPlayable;
        }

        /// <summary>
        /// 混合器的长度。
        /// 通常定义为所有子节点中最长的那个长度。
        /// </summary>
        public override float Length 
        {
            get
            {
                float maxLen = 0f;
                foreach(var c in _children) maxLen = Mathf.Max(maxLen, c.Length);
                return maxLen;
            }
        }

        /// <summary>
        /// 添加一个 Clip 作为子节点。
        /// 内部会自动创建 ClipState。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <returns>创建的 ClipState</returns>
        public ClipState Add(AnimationClip clip)
        {
            var state = new ClipState(clip);
            Add(state);
            return state;
        }

        /// <summary>
        /// 添加任意 AnimState 作为子节点。
        /// </summary>
        /// <param name="state">状态实例</param>
        public void Add(AnimState state)
        {
            if (state == null) return;
            
            // 初始化子状态
            // 注意：这里我们传递 null 作为 Layer，因为子节点直接由 MixerState 管理，目前不需要 AnimLayer 的复杂功能
            state.Initialize(null, _graph); 
            
            // 扩展 Mixer 端口
            int port = _mixerPlayable.GetInputCount();
            _mixerPlayable.SetInputCount(port + 1);
            
            // 连接子节点到 Mixer
            _graph.Connect(state.Playable, 0, _mixerPlayable, port);
            
            // 设置初始权重为 0
            _mixerPlayable.SetInputWeight(port, 0f); 
            
            // 注意：原生 AnimationMixerPlayable 会自动驱动子节点的时间，
            // 所以无需手动同步 Time，除非有特殊需求。
            
            _children.Add(state);
        }

        /// <summary>
        /// 设置指定子节点索引的权重。
        /// </summary>
        /// <param name="index">子节点索引</param>
        /// <param name="weight">权重 (0~1)</param>
        public void SetChildWeight(int index, float weight)
        {
            if (index >= 0 && index < _children.Count)
            {
                _mixerPlayable.SetInputWeight(index, weight);
            }
        }

        /// <summary>
        /// 获取指定索引的子状态。
        /// </summary>
        public AnimState GetChild(int index)
        {
            if (index >= 0 && index < _children.Count) return _children[index];
            return null;
        }

        /// <summary>
        /// 递归销毁混合器及其所有子状态。
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();
            foreach (var child in _children)
            {
                child.Destroy();
            }
            _children.Clear();
        }

        /// <summary>
        /// 更新逻辑。递归调用所有子节点的 OnUpdate。
        /// </summary>
        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            foreach(var c in _children)
            {
                c.OnUpdate(deltaTime);
            }
        }
    }
}
