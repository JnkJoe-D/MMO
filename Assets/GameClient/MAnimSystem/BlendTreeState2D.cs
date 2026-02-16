using System.Collections.Generic;
using UnityEngine;

namespace Game.MAnimSystem
{
    /// <summary>
    /// 2D 混合器 (2D Blend Tree)。
    /// 根据一个 Vector2 参数 Parameter，在多个散乱分布的子节点间进行插值。
    /// 使用 Shepard's Method (反距离加权 Inverse Distance Weighting) 进行简化的权重计算。
    /// 适用于 2D 移动混合 (Idle, WalkForward, WalkLeft, WalkRight, WalkBack) 等场景。
    /// 使用预分配数组避免每帧 GC 分配。
    /// </summary>
    public class BlendTreeState2D : MixerState
    {
        /// <summary>
        /// 存储每个子节点对应的 2D 参考坐标。
        /// </summary>
        private List<Vector2> _positions = new List<Vector2>();

        private Vector2 _parameter;

        /// <summary>
        /// 预分配的权重缓冲区，避免每帧分配。
        /// </summary>
        private float[] _weightBuffer = new float[8];

        /// <summary>
        /// 控制混合的 2D 参数。
        /// 修改此值会自动触发权重的重新计算。
        /// </summary>
        public Vector2 Parameter
        {
            get => _parameter;
            set
            {
                if (_parameter != value)
                {
                    _parameter = value;
                    UpdateWeights();
                }
            }
        }

        /// <summary>
        /// 添加子节点并指定 2D 坐标 (例如 x=Horizontal, y=Vertical)。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <param name="position">混合空间中的坐标点</param>
        /// <returns>创建的 ClipState</returns>
        public ClipState Add(AnimationClip clip, Vector2 position)
        {
            var state = base.Add(clip);
            _positions.Add(position);
            return state;
        }

        /// <summary>
        /// 获取指定索引的 2D 坐标。
        /// </summary>
        /// <param name="index">子节点索引</param>
        /// <returns>对应的 2D 坐标</returns>
        public Vector2 GetPosition(int index)
        {
            if (index >= 0 && index < _positions.Count)
            {
                return _positions[index];
            }
            return Vector2.zero;
        }

        /// <summary>
        /// 计算并更新权重。
        /// 使用预分配缓冲区避免 GC。
        /// </summary>
        private void UpdateWeights()
        {
            int count = _children.Count;
            if (count == 0) return;
            
            if (count == 1)
            {
                SetChildWeight(0, 1f);
                return;
            }

            // 按需扩容权重缓冲区
            if (count > _weightBuffer.Length)
            {
                _weightBuffer = new float[count * 2];
            }

            // 1. 特殊检查：如果参数与某个点完全重合，则避免除以零，直接设该点为 1
            for (int i = 0; i < count; i++)
            {
                float dist = Vector2.Distance(_parameter, _positions[i]);
                if (dist < 0.001f)
                {
                    for (int j = 0; j < count; j++)
                    {
                        SetChildWeight(j, j == i ? 1f : 0f);
                    }
                    return;
                }
            }

            // 2. 反距离加权算法 (Inverse Distance Weighting)
            // 公式: Wi = 1 / (distance ^ p)
            // 这里使用 p=1 (简单反比)，也可以尝试 p=2 (平方反比) 让混合更集中
            
            float totalWeight = 0f;

            for (int i = 0; i < count; i++)
            {
                float dist = Vector2.Distance(_parameter, _positions[i]);
                // 计算原始权重 (避免极小值)
                float w = 1.0f / Mathf.Max(0.0001f, dist);
                
                _weightBuffer[i] = w;
                totalWeight += w;
            }

            // 3. 归一化并应用权重
            // 确保所有权重之和为 1
            if (totalWeight > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    SetChildWeight(i, _weightBuffer[i] / totalWeight);
                }
            }
        }

        /// <summary>
        /// 初始化完成时计算一次初始权重。
        /// </summary>
        protected override void OnInitialized()
        {
            UpdateWeights();
        }
    }
}
