using SFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class EnvironmentSettings : UIItemBase
    {
        [UIFieldInit("AutoSaveToggle")]
        public UnityEngine.UI.Toggle AutoSaveToggle;
        [UIFieldInit("SpeedUpToggle")]
        public UnityEngine.UI.Toggle SpeedUpToggle;
        [UIFieldInit("ScreenAdaptationValueText")]
        public TMPro.TextMeshProUGUI ScreenAdaptationValueText;
        [UIFieldInit("ScreenAdaptationSlider")]
        public UnityEngine.UI.Slider ScreenAdaptationSlider;

        protected override void OnAwake()
        {
            this.AutoSaveToggle.onValueChanged.AddListener(isOn =>
            {
                PlayerPrefsTool.AutoSave_On.SetValue(isOn ? 1 : 0);
            });
            
            this.SpeedUpToggle.onValueChanged.AddListener(isOn =>
            {
                PlayerPrefsTool.SpeedUpDialogue_On.SetValue(isOn ? 1 : 0);
            });
            
            this.ScreenAdaptationSlider.onValueChanged.AddListener(value =>
            {
                PlayerPrefsTool.ScreenAdaptation_Value.SetValue(value);
                this.ScreenAdaptationValueText.text = $"{(int)(value * 100)}%";
                //TODO 异形屏UI适配
            });
        }

        protected override void OnShow()
        {
            this.AutoSaveToggle.isOn = PlayerPrefsTool.AutoSave_On.GetValue() == 1;
            this.SpeedUpToggle.isOn = PlayerPrefsTool.SpeedUpDialogue_On.GetValue() == 1;
            
            float screenAdaptationValue = PlayerPrefsTool.ScreenAdaptation_Value.GetValue();
            this.ScreenAdaptationSlider.value = screenAdaptationValue;
            this.ScreenAdaptationValueText.text = $"{(int)(screenAdaptationValue * 100)}%";
        }
    }
}
