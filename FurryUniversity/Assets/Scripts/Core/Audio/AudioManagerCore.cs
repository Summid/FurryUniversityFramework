using SFramework.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.Audio
{
    public interface IAudioLoaderAsync
    {
        STask<AudioClip> LoadAudioClipAsync(string assetName);
    }

    public class AudioChannel
    {
        public enum Type
        {
            Single,
            Multiple
        }

        private float volume = 1f;
        private readonly List<AudioSource> source = new List<AudioSource>();

        private readonly GameObject parent;
        private readonly IAudioLoaderAsync loader;
        private readonly Type type;

        public float Volme
        {
            get => this.volume;
            set
            {
                this.volume = Mathf.Clamp01(value);
                this.source.ForEach(s => s.volume = this.volume);
            }
        }

        public AudioChannel(GameObject parent, IAudioLoaderAsync loader, Type type)
        {
            this.parent = parent;
            this.loader = loader;
            this.type = type;
        }

        public async STask<AudioSource> PlayAsync(string clipName,bool loop = false)
        {
            AudioClip clip = await this.loader.LoadAudioClipAsync(clipName);
            if (clip == null)
                return null;

            AudioSource pickedSource = this.PickAudioSource();

            pickedSource.loop = loop;
            pickedSource.clip = clip;
            pickedSource.volume = this.Volme;

            if (loop)
            {
                pickedSource.Play();
            }
            else
            {
                pickedSource.PlayOneShot(clip);//播放完之后自动关闭
            }

            return pickedSource;
        }

        public void Pause()
        {
            this.source.ForEach(s => s.Pause());
        }

        public void Resume()
        {
            this.source.ForEach(s => s.Play());
        }

        public void Stop()
        {
            this.source.ForEach(s => s.Stop());
        }

        public void Mute()
        {
            this.Volme = 0f;
        }

        private AudioSource PickAudioSource()
        {
            if (this.type == Type.Single)
            {
                if (this.source.Count == 0)
                    this.source.Add(this.parent.AddComponent<AudioSource>());

                return this.source[0];//Single类型的音效(bgm)只有一个AudioSource
            }
            else
            {
                //找一个空闲的AudioSource来播放AudioClip
                AudioSource idleSource = this.source.Find(s => !s.isPlaying);

                if (idleSource == null)
                {
                    idleSource = this.parent.AddComponent<AudioSource>();
                    this.source.Add(idleSource);
                }
                else
                {
                    idleSource.pitch = 1;
                }

                return idleSource;
            }
        }
    }

    /// <summary>
    /// 管理音效的核心逻辑，可根据业务需求包装；使用前需调用<see cref="Init(IAudioLoaderAsync)"/>方法进行初始化
    /// </summary>
    public static class AudioManagerCore
    {
        private static GameObject instance;
        private static AudioListener listener;
        private static AudioChannel bgm;
        private static AudioChannel sfx;

        public static void Init(IAudioLoaderAsync audioLoader)
        {
            if (instance != null)
                return;

            instance = new GameObject("[AudioManager]");
            listener = instance.AddComponent<AudioListener>();

            Object.DontDestroyOnLoad(instance);

            bgm = new AudioChannel(instance, audioLoader, AudioChannel.Type.Single);
            sfx = new AudioChannel(instance, audioLoader, AudioChannel.Type.Multiple);
        }

        public static async STask<AudioSource> PlaySoundAsync(this string assetName, bool loop = false)
        {
            return await sfx.PlayAsync(assetName, loop);
        }

        public static async STask<AudioSource> PlayBGMAsync(this string assetName)
        {
            return await bgm.PlayAsync(assetName, true);//bgm默认循环播放
        }

        public static void PauseBGM()
        {
            bgm.Pause();
        }

        public static void SetSFXVolume(float volume)
        {
            sfx.Volme = volume;
        }

        public static void SetBGMVolume(float volume)
        {
            bgm.Volme = volume;
        }
    }
}