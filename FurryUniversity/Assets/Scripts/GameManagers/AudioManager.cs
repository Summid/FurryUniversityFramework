using SFramework.Core.Audio;
using SFramework.Threading.Tasks;
using SFramework.Utilities;
using SFramework.Utilities.Archive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.GameManagers
{
    public class AudioManager : GameManagerBase,ISavable
    {
        private static AudioManager selfInstance;//当Mgr初始化方法执行完之后将其赋值，用于判断Mgr是否初始化完毕

        private AudioLoader audioLoader;
        private bool musicOn;
        private bool sfxOn;
        private float musicVolume;
        private float sfxVolume;
        private AudioSource musicAudio;

        public float CurrentMusicVolume => (this.musicOn ? 1 : 0) * this.musicVolume;
        public float CurrentSFXVolume => (this.sfxOn ? 1 : 0) * this.sfxVolume;

        protected override async void OnInitialized()
        {
            AudioAssetList.Init();//加载Audio清单
            this.audioLoader = new AudioLoader();
            AudioManagerCore.Init(this.audioLoader);

            //根据设置初始化音量
            this.musicOn = PlayerPrefsTool.Music_On.GetValue() == 1;
            this.sfxOn = PlayerPrefsTool.SFX_On.GetValue() == 1;
            this.musicVolume = PlayerPrefsTool.MusicVolume_Value.GetValue();
            this.sfxVolume = PlayerPrefsTool.SFXVolume_Value.GetValue();
            this.ResetAllVolume();

            await this.audioLoader.Init();//加载常驻音效
            selfInstance = this;
        }

        public async STask<AudioSource> PlaySoundAsync(string audioName, bool loop = false)
        {
            if (!this.sfxOn || string.IsNullOrEmpty(audioName) || selfInstance == null)
                return null;

            string clip = audioName;//TODO 后期可能会考虑将audioName换成整型id，因此这里换一下
            AudioSource audioSource = await clip.PlaySoundAsync(loop);
            if (audioSource == null)
            {
                Debug.LogError($"Audio {audioName} not found");
                return null;
            }
            audioSource.volume = this.CurrentSFXVolume;
            return audioSource;
        }

        public async STask<AudioSource> PlayBGMAsync(string audioName)
        {
            while (selfInstance == null)
                await STask.NextFrame();
            string clip = audioName;//同上

            //当正在播放的背景音乐与将要播放的相同，则跳过
            if (clip != null && !(this.musicAudio != null && this.musicAudio.isPlaying && this.musicAudio.clip.name == clip))
            {
                this.musicAudio = await clip.PlayBGMAsync();
                if (this.musicAudio == null)
                {
                    Debug.LogError($"Audio {audioName} not found");
                    return null;
                }

                this.musicAudio.volume = this.CurrentMusicVolume;
                return this.musicAudio;
            }
            return null;
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
            AudioManagerCore.SetBGMVolume(this.CurrentMusicVolume);
            PlayerPrefsTool.Music_On.SetValue(value);//1代表开启，0代表关闭
        }

        public void SetSFXOn(bool isOn)
        {
            this.sfxOn = isOn;
            int value = isOn ? 1 : 0;
            AudioManagerCore.SetSFXVolume(this.CurrentSFXVolume);
            PlayerPrefsTool.SFX_On.SetValue(value);
        }

        public void SetBGMVolume(float volume)
        {
            this.musicVolume = volume;
            AudioManagerCore.SetBGMVolume(this.CurrentMusicVolume);
            PlayerPrefsTool.MusicVolume_Value.SetValue(Mathf.Clamp01(volume));
        }

        public void SetSFXVolume(float volume)
        {
            this.sfxVolume = volume;
            AudioManagerCore.SetSFXVolume(this.CurrentSFXVolume);
            PlayerPrefsTool.SFXVolume_Value.SetValue(Mathf.Clamp01(volume));
        }

        public void ResetAllVolume()
        {
            AudioManagerCore.SetBGMVolume(this.CurrentMusicVolume);
            AudioManagerCore.SetSFXVolume(this.CurrentSFXVolume);
        }

        public void DisposeSFXGroupBunudles()
        {
            this.audioLoader?.DisposeGroupSFXGroupBundlesAsync().Forget();
        }

        public ArchiveObject OnSave()
        {
            ArchiveObject archiveObject = new ArchiveObject();
            archiveObject.ChapterName = "HelloAudioName";
            archiveObject.DialogueName = "HelloAudioName";
            archiveObject.ContentIndex = 23333;
            return archiveObject;
        }
    }
}
