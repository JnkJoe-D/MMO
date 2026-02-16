using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Game.MAnimSystem
{
    /// <summary>
    /// 线性混合器 (1D Blend Tree)。
    /// 根据一个 float 参数 Parameter，在多个子节点之间进行线性插值。
    /// 适用于 Walk (0.0) -> Run (1.0) 这种由单一变量控制的混合。
    /// 支持阈值自动排序，确保插值正确。
    /// </summary>
    public class LinearMixerState : MixerState
    {
        /// <summary>
        /// 存储每个子节点对应的阈值。
        /// 始终保持从小到大排序。
        /// </summary>
        private List<float> _thresholds = new List<float>();

        private float _parameter;

        /// <summary>
        /// 控制混合的参数。
        /// 修改此值会自动触发权重的重新计算。
        /// </summary>
        public float Parameter
        {
            get => _parameter;
            set
            {
                if (!Mathf.Approximately(_parameter, value))
                {
                    _parameter = value;
                    UpdateWeights();
                }
            }
        }

        /// <summary>
        /// 添加子节点并指定该节点关联的阈值。
        /// 自动按阈值排序，确保插值正确。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <param name="threshold">触发该动画的参数阈值</param>
        /// <returns>创建的 ClipState</returns>
        public ClipState Add(AnimationClip clip, float threshold)
        {
            var state = new ClipState(clip);
            Add(state);
            
            // 找到正确的插入位置（保持阈值有序）
            int insertIndex = _thresholds.Count;
            for (int i = 0; i < _thresholds.Count; i++)
            {
                if (threshold < _thresholds[i])
                {
                    insertIndex = i;
                    break;
                }
            }
            
            // 插入阈值
            _thresholds.Insert(insertIndex, threshold);
            
            // 如果需要调整子节点顺序
            if (insertIndex < _children.Count - 1)
            {
                // 移动子节点到正确位置
                _children.RemoveAt(_children.Count - 1);
                _children.Insert(insertIndex, state);
                
                // 重新连接端口以匹配新顺序
                ReorderMixerPorts();
            }
            
            return state;
        }

        /// <summary>
        /// 重新排序 Mixer 端口以匹配子节点顺序。
        /// </summary>
        private void ReorderMixerPorts()
        {
            // 断开所有连接
            int count = _children.Count;
            for (int i = 0; i < count; i++)
            {
                _graph.Disconnect(_mixerPlayable, i);
            }
            
            // 按新顺序重新连接
            for (int i = 0; i < count; i++)
            {
                _graph.Connect(_children[i].Playable, 0, _mixerPlayable, i);
            }
            
            // 更新权重
            UpdateWeights();
        }

        /// <summary>
        /// 获取指定索引的阈值。
        /// </summary>
        /// <param name="index">子节点索引</param>
        /// <returns>对应的阈值</returns>
        public float GetThreshold(int index)
        {
            if (index >= 0 && index < _thresholds.Count)
            {
                return _thresholds[index];
            }
            return 0f;
        }

        /// <summary>
        /// 根据当前 Parameter 计算并更新所有子节点的权重。
        /// </summary>
        private void UpdateWeights()
        {
            if (_children.Count == 0) return;
            
            // 只有一个子节点，始终权重为 1
            if (_children.Count == 1)
            {
                SetChildWeight(0, 1f);
                return;
            }

            int count = _children.Count;
            
            // 1. 初始化所有权重为 0
            for (int i = 0; i < count; i++)
            {
                SetChildWeight(i, 0f);
            }

            // 2. 边界情况处理
            // 如果参数小于等于最小阈值，则播放第一个
            if (_parameter <= _thresholds[0])
            {
                SetChildWeight(0, 1f);
                return;
            }
            // 如果参数大于等于最大阈值，则播放最后一个
            if (_parameter >= _thresholds[count - 1])
            {
                SetChildWeight(count - 1, 1f);
                return;
            }

            // 3. 中间插值处理
            // 寻找参数落在哪两个阈值之间（阈值已排序）
            for (int i = 0; i < count - 1; i++)
            {
                float t1 = _thresholds[i];
                float t2 = _thresholds[i + 1];

                if (_parameter >= t1 && _parameter <= t2)
                {
                    // 在区间 [t1, t2] 内，计算插值因子
                    float factor = (_parameter - t1) / (t2 - t1);
                    
                    // t1 对应的节点权重递减，t2 对应的节点权重递增
                    SetChildWeight(i, 1f - factor);
                    SetChildWeight(i + 1, factor);
                    return;
                }
            }
        }
        
        /// <summary>
        /// 重写初始化回调，确保组件刚启动时的初始权重是正确的。
        /// </summary>
        protected override void OnInitialized()
        {
            UpdateWeights();
        }
    }
}
