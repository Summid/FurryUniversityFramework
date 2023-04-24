using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class ButtonsFloatTipItem : UIItemBase, IUIPool<ButtonsFloatTipView.ButtonData>
    {
        [UIFieldInit("Button")] public UnityEngine.UI.Image Button_Image;
        [UIFieldInit("Button")] public UnityEngine.UI.Button Button_Button;
        [UIFieldInit("Button")] public UnityEngine.UI.LayoutElement Button_LayoutElement;
        [UIFieldInit("ShowText")] public TMPro.TextMeshProUGUI ShowText;
        [UIFieldInit("DivideImage")] public UnityEngine.UI.Image DivideImage;

        private ButtonsFloatTipView.ButtonData data;
        
        public void PoolSetData(ButtonsFloatTipView.ButtonData data)
        {
            this.data = data;

            this.ShowText.text = this.data.ShowText;
            this.DivideImage.gameObject.SetActive(!this.data.DisableDivideLine);
            this.Button_Button.interactable = this.data.Interactable;
            this.Button_Button.onClick.RemoveAllListeners();
            this.Button_Button.onClick.AddListener(() => this.data.OnClick?.Invoke(data.Index));
        }
    }
}