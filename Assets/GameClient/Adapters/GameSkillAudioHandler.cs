using System.Collections.Generic;
using SkillEditor;
using UnityEngine;

namespace Game.Adapters
{
    /// <summary>
    /// 运行时音频适配器
    /// 实现 ISkillAudioHandler 接口，管理运行时技能音效播放
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

        private List<AudioSourceInfo> _pool = new List<AudioSourceInfo>();
        private int _nextId = 1;

        private void Awake()
        {
            if (audioRoot == null) audioRoot = transform;
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                CreateSource();
            }
        }

        private AudioSourceInfo CreateSource()
        {
            var go = new GameObject($"SkillAudio_{_pool.Count}");
            go.transform.SetParent(audioRoot);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            
            var info = new AudioSourceInfo { source = source, isBorrowed = false };
            _pool.Add(info);
            return info;
        }

        public int PlaySound(UnityEngine.AudioClip clip, AudioArgs args)
        {
            if (clip == null) return -1;

            var info = GetAvailableSource();
            if (info == null) return -1;

            info.id = _nextId++;
            info.isBorrowed = true;
            
            var source = info.source;
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
            return info.id;
        }

        public void StopSound(int soundId)
        {
            var info = GetSourceById(soundId);
            if (info != null)
            {
                ReturnSource(info);
            }
        }

        public void UpdateSound(int soundId, float volume, float pitch, float time)
        {
            var info = GetSourceById(soundId);
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
            foreach (var info in _pool)
            {
                if (info.isBorrowed)
                {
                    ReturnSource(info);
                }
            }
        }

        private AudioSourceInfo GetAvailableSource()
        {
            foreach (var info in _pool)
            {
                if (!info.isBorrowed) return info;
            }
            // 扩容
            return CreateSource();
        }

        private AudioSourceInfo GetSourceById(int id)
        {
            foreach (var info in _pool)
            {
                if (info.isBorrowed && info.id == id) return info;
            }
            return null;
        }

        private void ReturnSource(AudioSourceInfo info)
        {
            if (info.source != null)
            {
                info.source.Stop();
                info.source.clip = null;
            }
            info.isBorrowed = false;
            info.id = 0;
        }
    }
}
