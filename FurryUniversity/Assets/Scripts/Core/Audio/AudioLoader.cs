using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using SFramework.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.Audio
{
    /// <summary>
    /// 主要用于处理音效资源加载与卸载逻辑，具体的音效播放等逻辑在<see cref="AudioManagerCore"/>中
    /// </summary>
    public class AudioLoader : IAudioLoaderAsync
    {
        public enum AudioAssetType
        {
            /// <summary>背景音乐资源，同时只保留一个Bundle在内存中</summary>
            BGM,
            /// <summary>音效组，（推荐）按功能模块来分组，在合适的时候会卸载Bundle（如场景切换时）</summary>
            SFXGroup,
            /// <summary>常驻音效，常驻内存</summary>
            CommonSFX
        }

        private readonly BGMAssetLoader bgmLoader = new BGMAssetLoader();
        private readonly CommonSFXLoader commonSFXLoader = new CommonSFXLoader();
        private readonly SFXGroupAssetLoader sfxGroupAssetLoader = new SFXGroupAssetLoader();

        public AudioLoader() { }

        public async STask Init()
        {
            //先把常驻音效加载进来
            await this.commonSFXLoader.LoadCommonSFXBundleAsync();
        }

        public async STask<AudioClip> LoadAudioClipAsync(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                return default;

            var type = this.GetAudioAssetType(assetName);
            switch (type)
            {
                case AudioAssetType.BGM:
                    return await this.bgmLoader.LoadAudioClipAsync(assetName);
                case AudioAssetType.SFXGroup:
                    return await this.sfxGroupAssetLoader.LoadAudioClipAsync(assetName);
                case AudioAssetType.CommonSFX:
                    return await this.commonSFXLoader.LoadAudioClipAsync(assetName);
                default:
                    throw new System.Exception("不可能出现的bug");
            }
        }

        private AudioAssetType GetAudioAssetType(string assetName)
        {
            if (AudioAssetList.audioAssetInfo.TryGetValue(assetName, out var type))
            {
                return type;
            }

            return AudioAssetType.SFXGroup;
        }

        public async STask DisposeGroupSFXGroupBundlesAsync()
        {
            await this.sfxGroupAssetLoader.DisposeLoadedGroups();
        }

        private class BGMAssetLoader
        {
            private string lastBGMBundleName;

            public async STask<AudioClip> LoadAudioClipAsync(string assetName)
            {
                if (string.IsNullOrEmpty(assetName))
                    return default;

                if (this.lastBGMBundleName != null)
                {
                    await AssetBundleManager.UnloadAssetBundleAsync(this.lastBGMBundleName);
                    this.lastBGMBundleName = null;
                }

                string bundleName = assetName + StaticVariables.AudioBGMBundleExtension;
                AudioClip result = await AssetBundleManager.LoadAssetInAssetBundleAsync<AudioClip>(assetName, bundleName);
                this.lastBGMBundleName = bundleName;
                return result;
            }
        }

        private class CommonSFXLoader
        {
            public async STask LoadCommonSFXBundleAsync()
            {
                await AssetBundleManager.LoadAssetBundleAsync(StaticVariables.AudioCommonSFXBundleName);
            }

            public async STask<AudioClip> LoadAudioClipAsync(string assetName)
            {
                if (string.IsNullOrEmpty(assetName))
                    return default;

                return await AssetBundleManager.LoadAssetInAssetBundleAsync<AudioClip>(assetName, StaticVariables.AudioCommonSFXBundleName);
            }
        }

        private class SFXGroupAssetLoader
        {
            private HashSet<string> loadedBundles = new HashSet<string>();

            public async STask<AudioClip> LoadAudioClipAsync(string assetName)
            {
                string bundleName = this.GetGroupBundleName(assetName);
                if (bundleName == null)
                    return default;

                if (!this.loadedBundles.Contains(bundleName))
                {
                    await AssetBundleManager.LoadAssetBundleAsync(bundleName);
                    this.loadedBundles.Add(bundleName);
                }

                return await AssetBundleManager.LoadAssetInAssetBundleAsync<AudioClip>(assetName, bundleName);
            }

            public async STask DisposeLoadedGroups()
            {
                foreach (string bundleName in this.loadedBundles)
                {
                    await AssetBundleManager.UnloadAssetBundleAsync(bundleName);
                }
                this.loadedBundles.Clear();
            }

            private string GetGroupBundleName(string assetName)
            {
                if (AudioAssetList.sfxGroupAssetInfo.TryGetValue(assetName, out string value))
                {
                    return value;
                }

                return null;
            }
        }
    }
}