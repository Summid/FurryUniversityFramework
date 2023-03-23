using SFramework.Core.GameManagers;
using SFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class AudioSettings : UIItemBase
    {
        [UIFieldInit("BGMToggle")]
        public UnityEngine.UI.Toggle BGMToggle;
        [UIFieldInit("BGMVolumeSlider")]
        public UnityEngine.UI.Slider BGMVolumeSlider;
        [UIFieldInit("BGMVolumeValueText")]
        public TMPro.TextMeshProUGUI BGMVolumeValueText;
        [UIFieldInit("SFXToggle")]
        public UnityEngine.UI.Toggle SFXToggle;
        [UIFieldInit("SFXVolumeSlider")]
        public UnityEngine.UI.Slider SFXVolumeSlider;
        [UIFieldInit("SFXVolumeValueText")]
        public TMPro.TextMeshProUGUI SFXVolumeValueText;

        private AudioManager audioManager;
        protected override void OnAwake()
        {
            this.audioManager = GameManager.Instance.AudioManager;
            
            this.BGMToggle.onValueChanged.AddListener(isOn =>
            {
                this.audioManager.SetBGMOn(isOn);
            });
            
            this.BGMVolumeSlider.onValueChanged.AddListener(value =>
            {
                this.audioManager.SetBGMVolume(value);
                this.BGMVolumeValueText.text = $"{(int)(value * 100)}%";
            });
            
            this.SFXToggle.onValueChanged.AddListener(isOn =>
            {
                this.audioManager.SetSFXOn(isOn);
            });
            
            this.SFXVolumeSlider.onValueChanged.AddListener(value =>
            {
                this.audioManager.SetSFXVolume(value);
                this.SFXVolumeValueText.text = $"{(int)(value * 100)}%";
            });
        }

        protected override void OnShow()
        {
            this.BGMToggle.isOn = PlayerPrefsTool.Music_On.GetValue() == 1;
            float bgmVolume = PlayerPrefsTool.MusicVolume_Value.GetValue();
            this.BGMVolumeSlider.value = bgmVolume;
            this.BGMVolumeValueText.text = $"{(int)(bgmVolume * 100)}%";
            
            this.SFXToggle.isOn = PlayerPrefsTool.SFX_On.GetValue() == 1;
            float sfxVolume = PlayerPrefsTool.SFXVolume_Value.GetValue();
            this.SFXVolumeSlider.value = sfxVolume;
            this.SFXVolumeValueText.text = $"{(int)(sfxVolume * 100)}%";
        }
    }
}
