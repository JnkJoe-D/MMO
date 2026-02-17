using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace SkillEditor.Editor
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
            
            Debug.Log($"[{Lan.EditorTitle}] {Lan.ExportToJson}: {path}");
        }

        /// <summary>
        /// 从 JSON 文件导入技能
        /// </summary>
        public static SkillTimeline ImportFromJson(string path)
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
                            animClip.clipGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(animClip.animationClip));
                        if(animClip.avatarMask != null)
                            animClip.maskGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(animClip.avatarMask));
                    }
                    else if (clip is VFXClip vfxClip && vfxClip.effectPrefab != null)
                    {
                        vfxClip.clipGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(vfxClip.effectPrefab));
                    }
                    else if (clip is AudioClip audioClip && audioClip.audioClip != null)
                    {
                        audioClip.clipGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(audioClip.audioClip));
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
                            string assetPath = AssetDatabase.GUIDToAssetPath(animClip.clipGuid);
                            animClip.animationClip = AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(assetPath);
                        }
                        if(!string.IsNullOrEmpty(animClip.maskGuid))
                        {
                            string assetPath = AssetDatabase.GUIDToAssetPath(animClip.maskGuid);
                            animClip.avatarMask = AssetDatabase.LoadAssetAtPath<UnityEngine.AvatarMask>(assetPath);
                        }
                    }
                    else if (clip is VFXClip vfxClip && !string.IsNullOrEmpty(vfxClip.clipGuid))
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(vfxClip.clipGuid);
                        vfxClip.effectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    }
                    else if (clip is AudioClip audioClip && !string.IsNullOrEmpty(audioClip.clipGuid))
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(audioClip.clipGuid);
                        audioClip.audioClip = AssetDatabase.LoadAssetAtPath<UnityEngine.AudioClip>(assetPath);
                    }
                }
            }
        }
    }
}
