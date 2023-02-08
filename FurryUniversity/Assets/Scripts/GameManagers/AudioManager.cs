using SFramework.Core.Audio;
using SFramework.Threading.Tasks;
using SFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.GameManagers
{
    public class AudioManager : GameManagerBase
    {
        private static AudioManager selfInstance;//当Mgr初始化方法执行完之后将其赋值，用于判断Mgr是否初始化完毕

        private AudioLoader audioLoader;
        private bool musicOn;
        private bool sfxOn;
        private float musicVolume;
        private float sfxVolume;
        private AudioSource musicAudio;

        protected override async void OnInitialized()
        {
            AudioAssetList.Init();//加载Audio清单
            this.audioLoader = new AudioLoader();
            AudioManagerCore.Init(this.audioLoader);

            //根据设置初始化音量
            this.SetBGMOn(PlayerPrefsTool.Music_On.GetValue() == 1);
            this.SetSFXOn(PlayerPrefsTool.SFX_On.GetValue() == 1);

            await this.audioLoader.Init();//加载常驻音效
            selfInstance = this;
        }

        public async STask<AudioSource> PlaySoundAsync(string audioName, bool loop = false)
        {
            if (!this.musicOn || string.IsNullOrEmpty(audioName) || selfInstance == null)
                return null;

            float volume = 1f;//暂时默认已最大音量播放
            string clip = audioName;//后期可能会考虑将audioName换成整型id，因此这里换一下
            AudioSource audioSource = await AudioManagerCore.PlaySoundAsync(clip, loop);
            if (audioSource == null)
            {
                Debug.LogError($"Audio {audioName} not found");
                return null;
            }
            audioSource.volume = this.sfxVolume * volume;
            return audioSource;
        }

        public async STask PlayBGMAsync(string audioName)
        {
            while (selfInstance == null)
                await STask.NextFrame();
            float volume = 1f;//同上
            string clip = audioName;//同上

            //当正在播放的背景音乐与将要播放的相同，则跳过
            if (clip != null && !(this.musicAudio != null && this.musicAudio.isPlaying && this.musicAudio.clip.name == clip))
            {
                this.musicAudio = await AudioManagerCore.PlayBGMAsync(clip);
                if (this.musicAudio == null)
                {
                    Debug.LogError($"Audio {audioName} not found");
                    return;
                }

                this.musicAudio.volume = this.musicVolume * volume;
            }
        }

        public async STaskVoid PauseBGM()
        {
            while(selfInstance == null)
                await STask.NextFrame();
            AudioManagerCore.PauseBGM();
        }

        public void SetBGMOn(bool isOn)
        {
            this.musicOn = isOn;
            int value = isOn? 1 : 0;
            this.SetBGMVolume(value);
            PlayerPrefsTool.Music_On.SetValue(value);//1代表开启，0代表关闭
        }

        public void SetSFXOn(bool isOn)
        {
            this.sfxOn = isOn;
            int value = isOn ? 1 : 0;
            this.SetSFXVolume(value);
            PlayerPrefsTool.SFX_On.SetValue(value);
        }

        public void SetBGMVolume(float volume)
        {
            this.musicVolume = volume;
            AudioManagerCore.SetBGMVolume(volume);
        }

        public void SetSFXVolume(float volume)
        {
            this.sfxVolume = volume;
            AudioManagerCore.SetSFXVolume(volume);
        }

        public void DisposeSFXGroupBunudles()
        {
            this.audioLoader?.DisposeGroupSFXGroupBundlesAsync().Forget();
        }
    }
}
