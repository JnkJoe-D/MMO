using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace SkillEditor
{
    /// <summary>
    /// 技能序列化工具类
    /// 遍历树状结构：groups → tracks → clips
    /// </summary>
    public static class SerializationUtility
    {
        /// <summary>
        /// 导出技能到 JSON 文件
        /// </summary>
        public static void ExportToJson(SkillTimeline timeline, string path)
        {
            if (timeline == null) return;

            // 1. 导出前置处理：确保所有 Clip 的 GUID 都是最新的
            RefreshAllGuids(timeline);

            // 2. 序列化
            string json = JsonUtility.ToJson(timeline, true);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// 从 JSON 文件路径导入技能
        /// </summary>
        public static SkillTimeline ImportFromJsonPath(string path)
        {
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            SkillTimeline timeline = ScriptableObject.CreateInstance<SkillTimeline>();
            JsonUtility.FromJsonOverwrite(json, timeline);

            // 导入后置处理：根据 GUID 还原资源引用
            ResolveAllAssets(timeline);

            return timeline;
        }
        /// <summary>
        /// 从 JSON 文件获取技能
        /// </summary>
        public static SkillTimeline OpenFromJson(TextAsset textAsset)
        {
            if(textAsset==null)return null;
            string json = textAsset.text;
            SkillTimeline timeline = ScriptableObject.CreateInstance<SkillTimeline>();
            JsonUtility.FromJsonOverwrite(json, timeline);

            // 导入后置处理：根据 GUID 还原资源引用
            ResolveAllAssets(timeline);
            return timeline;
        }
        /// <summary>
        /// 刷新所有片段的 GUID（遍历 groups → tracks → clips）
        /// </summary>
        private static void RefreshAllGuids(SkillTimeline timeline)
        {
            foreach (var track in timeline.AllTracks)
            {
                foreach (var clip in track.clips)
                {
                    if (clip is SkillAnimationClip animClip)
                    {
                        if(animClip.animationClip != null)
                            animClip.clipGuid = GetAssetGuid(animClip.animationClip);
                        if(animClip.overrideMask != null)
                            animClip.maskGuid = GetAssetGuid(animClip.overrideMask);
                    }
                    else if (clip is VFXClip vfxClip && vfxClip.effectPrefab != null)
                    {
                        vfxClip.prefabGuid = GetAssetGuid(vfxClip.effectPrefab);
                    }
                    else if (clip is SkillAudioClip audioClip && audioClip.audioClip != null)
                    {
                        audioClip.clipGuid = GetAssetGuid(audioClip.audioClip);
                    }
                }
            }
        }

        /// <summary>
        /// 根据 GUID 还原所有资源（遍历 groups → tracks → clips）
        /// </summary>
        public static void ResolveAllAssets(SkillTimeline timeline)
        {
            if (timeline == null) return;

            foreach (var track in timeline.AllTracks)
            {
                foreach (var clip in track.clips)
                {
                    if (clip is SkillAnimationClip animClip)
                    {
                        if(!string.IsNullOrEmpty(animClip.clipGuid))
                        {
                            animClip.animationClip = ResolveAsset<AnimationClip>(animClip.clipGuid);
                        }
                        if(!string.IsNullOrEmpty(animClip.maskGuid))
                        {
                            animClip.overrideMask = ResolveAsset<AvatarMask>(animClip.maskGuid);
                        }
                    }
                    else if (clip is VFXClip vfxClip && !string.IsNullOrEmpty(vfxClip.prefabGuid))
                    {
                        vfxClip.effectPrefab = ResolveAsset<GameObject>(vfxClip.prefabGuid);
                    }
                    else if (clip is SkillAudioClip audioClip && !string.IsNullOrEmpty(audioClip.clipGuid))
                    {
                        audioClip.audioClip = ResolveAsset<AudioClip>(audioClip.clipGuid);
                    }
                }
            }
        }
        private static string GetAssetGuid(Object asset)
        {
#if UNITY_EDITOR
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            return guid;
#else
            return "";
#endif
        }
        private static T ResolveAsset<T>(string guid) where T:Object
        {
#if UNITY_EDITOR
            string assetPath =  AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            return asset;
#else
            return null;
#endif
        }
    }
}
