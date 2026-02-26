using UnityEngine;

namespace Game.Logic.Player
{
    /// <summary>
    /// 标准化动画控制解耦接口
    /// 将业务与当前项目现有的 MAnimSystem（或任何未来可能的动画系统）彻底隔绝。
    /// </summary>
    public interface IAnimController
    {
        /// <summary>
        /// 播放指定动画片段
        /// 直接基于运行时硬引用，避免由 String 哈希带来的性能损耗与低级打字错误
        /// </summary>
        /// <param name="clip">动画剪辑的引用</param>
        /// <param name="fadeDuration">融合渐变时间（秒）</param>
        void PlayAnim(AnimationClip clip, float fadeDuration = 0.2f);
    }
}
