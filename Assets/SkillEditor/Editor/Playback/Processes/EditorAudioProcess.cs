using UnityEngine;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 编辑器预览：音频片段 Process
    /// 通过 EditorAudioManager 获取/归还 AudioSource
    /// </summary>
    [ProcessBinding(typeof(AudioClip), PlayMode.EditorPreview)]
    public class EditorAudioProcess : ProcessBase<AudioClip>
    {
        private UnityEngine.AudioSource audioSource;

        public override void OnEnter()
        {
            audioSource = EditorAudioManager.Instance.Get();
            if (audioSource != null && clip.audioClip != null)
            {
                audioSource.clip = clip.audioClip;
                audioSource.volume = clip.volume;
                audioSource.Play();
            }
        }

        public override void OnUpdate(float currentTime, float deltaTime)
        {
            // 音频播放由 AudioSource 自驱动，这里可做时间同步
            if (audioSource != null && audioSource.isPlaying)
            {
                float localTime = currentTime - clip.startTime;
                // 可选：同步 AudioSource 播放位置防止漂移
                // audioSource.time = localTime;
            }
        }

        public override void OnExit()
        {
            // 级别 1：归还 AudioSource 到池
            if (audioSource != null)
            {
                EditorAudioManager.Instance.Return(audioSource);
                audioSource = null;
            }
        }

        public override void Reset()
        {
            base.Reset();
            audioSource = null;
        }
    }
}
