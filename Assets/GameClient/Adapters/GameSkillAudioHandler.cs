using System.Collections.Generic;
using SkillEditor;
using UnityEngine;
using Game.Pool;

namespace Game.Adapters
{
    /// <summary>
    /// 运行时音频适配器
    /// 实现 ISkillAudioHandler 接口，通过 ComponentPool 管理 AudioSource
    /// </summary>
    public class GameSkillAudioHandler : MonoBehaviour, ISkillAudioHandler
    {
        private class AudioSourceInfo
        {
            public int id;
            public AudioSource source;
            public bool isBorrowed;
        }

        [SerializeField]
        private int poolSize = 10;
        
        [SerializeField]
        private Transform audioRoot;

        private ComponentPool<AudioSource> _audioPool;
        private List<AudioSourceInfo> _activeInfos = new List<AudioSourceInfo>();
        private int _nextId = 1;

        private void Awake()
        {
            if (audioRoot == null) audioRoot = transform;
            InitializePool();
        }

        private void InitializePool()
        {
            var config = new ComponentPool<AudioSource>.Config
            {
                initialSize = poolSize,
                maxSize = poolSize * 2
            };

            _audioPool = new ComponentPool<AudioSource>(CreateAudioSource, config);
            _audioPool.OnGet = (source) =>
            {
                source.playOnAwake = false;
            };
            _audioPool.OnReturn = (source) =>
            {
                source.Stop();
                source.clip = null;
            };

            // 注册到全局管理器（可选，便于统一管理生命周期）
            GlobalPoolManager.RegisterComponentPool($"Audio_{GetInstanceID()}", _audioPool);
        }

        private AudioSource CreateAudioSource()
        {
            var go = new GameObject($"SkillAudio_{_activeInfos.Count}");
            go.transform.SetParent(audioRoot);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            go.SetActive(false);
            return source;
        }

        public int PlaySound(UnityEngine.AudioClip clip, AudioArgs args)
        {
            if (clip == null) return -1;

            var source = _audioPool.Get();
            if (source == null) return -1;

            int id = _nextId++;
            var info = new AudioSourceInfo { id = id, source = source, isBorrowed = true };
            _activeInfos.Add(info);

            source.clip = clip;
            source.volume = args.volume;
            source.pitch = args.pitch;
            source.loop = args.loop;
            source.spatialBlend = args.spatialBlend;
            source.time = args.startTime;
            
            if (args.spatialBlend > 0.01f)
            {
                source.transform.position = args.position;
            }

            source.Play();
            return id;
        }

        public void StopSound(int soundId)
        {
            var info = GetInfoById(soundId);
            if (info != null)
            {
                ReturnSource(info);
            }
        }

        public void UpdateSound(int soundId, float volume, float pitch, float time)
        {
            var info = GetInfoById(soundId);
            if (info != null && info.source != null)
            {
                info.source.volume = volume;
                info.source.pitch = pitch;
                // 注意：频繁设置 time 可能导致杂音，通常仅在 seek 时设置
                // 这里仅由上层逻辑保证调用频率
                // 如果 time < 0，表示不强制同步时间
                if (time >= 0f && Mathf.Abs(info.source.time - time) > 0.1f)
                {
                    info.source.time = time;
                }
            }
        }

        public void StopAll()
        {
            // 倒序移除，避免遍历修改问题
            for (int i = _activeInfos.Count - 1; i >= 0; i--)
            {
                if (_activeInfos[i].isBorrowed)
                {
                    ReturnSource(_activeInfos[i]);
                }
            }
        }

        private AudioSourceInfo GetInfoById(int id)
        {
            foreach (var info in _activeInfos)
            {
                if (info.isBorrowed && info.id == id) return info;
            }
            return null;
        }

        private void ReturnSource(AudioSourceInfo info)
        {
            if (info.source != null)
            {
                _audioPool.Return(info.source);
            }
            info.isBorrowed = false;
            info.id = 0;
            _activeInfos.Remove(info);
        }

        private void OnDestroy()
        {
            _audioPool?.Dispose();
        }
    }
}
