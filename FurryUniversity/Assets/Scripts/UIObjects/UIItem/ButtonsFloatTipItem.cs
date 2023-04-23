using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class ButtonsFloatTipItem : UIItemBase, IUIPool<ButtonsFloatTipView.ButtonsData>
    {
        [UIFieldInit("Button")] public UnityEngine.UI.Image Button_Image;
        [UIFieldInit("Button")] public UnityEngine.UI.Button Button_Button;
        [UIFieldInit("Button")] public UnityEngine.UI.LayoutElement Button_LayoutElement;
        [UIFieldInit("ShowText")] public TMPro.TextMeshProUGUI ShowText;
        [UIFieldInit("DivideImage")] public UnityEngine.UI.Image DivideImage;

        public void PoolSetData(ButtonsFloatTipView.ButtonsData data)
        {
            throw new System.NotImplementedException();
        }
    }
}