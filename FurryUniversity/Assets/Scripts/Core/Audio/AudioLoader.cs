using UnityEngine;

namespace SFramework.Core.Audio
{
    public class AudioLoader : IAudioLoader
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

        public AudioClip LoadAudioClip(string assetName)
        {
            throw new System.NotImplementedException();
        }
    }
}