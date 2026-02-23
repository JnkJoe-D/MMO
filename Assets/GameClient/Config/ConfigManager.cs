using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using cfg;
using SimpleJSON;
using Game.Framework;
using Game.Resource;

namespace Game.Config
{
    /// <summary>
    /// Luban 配置管理器
    /// </summary>
    public class ConfigManager : Singleton<ConfigManager>
    {
        private Tables _tables;
        
        /// <summary>
        /// 配置表访问入口
        /// </summary>
        public Tables Tables => _tables;

        /// <summary>
        /// 初始化配置表
        /// </summary>
        public async Task InitializeAsync()
        {
            // 通过 Luban 生成的入口初始化，注入资源加载委托
            _tables = new Tables(LoadConfigJson);
            
            // 如果需要预加载某些表，可以在这里处理
            // await _tables.TbItem.LoadAsync(); (如果导出了异步加载逻辑)
            
            Debug.Log("[ConfigManager] 初始化完成");
        }

        /// <summary>
        /// Luban 内部加载委托 (适配 SimpleJSON)
        /// 【生产环境补充】：
        /// 在 HostPlayMode 下，同步加载 (LoadAssetSync) 仅在资源已存在于本地缓存时有效。
        /// 本架构通过 GameRoot 保证了 ResourceManager 初始化（及热更新下载）先于 ConfigManager 运行，
        /// 因此此处同步加载是安全且符合生产环境规范的。
        /// </summary>
        /// <param name="file">JSON 文件名 (不带后缀)</param>
        /// <returns>解析后的 JSONNode</returns>
        private JSONNode LoadConfigJson(string file)
        {
            // 拼接寻址路径：Assets/Configs/{file}.json
            string assetPath = $"Assets/Configs/{file}.json";
            var asset = ResourceManager.Instance.LoadAsset<TextAsset>(assetPath);
            if (asset == null)
            {
                Debug.LogError($"[ConfigManager] 找不到配置文件: {file}");
                return null;
            }

            return JSONNode.Parse(asset.text);
        }
    }
}
