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

        
    }
}
