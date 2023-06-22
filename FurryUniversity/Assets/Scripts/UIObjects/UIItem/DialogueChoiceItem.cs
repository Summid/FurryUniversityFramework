using SDS.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class DialogueChoiceItem : UIItemBase,IUIPool<SDSDialogueChoiceData>
    {
        [UIFieldInit("ChoiceItem")]
        public UnityEngine.UI.Image ChoiceItem_Image;
        [UIFieldInit("ChoiceItem")]
        public UnityEngine.UI.Button ChoiceItem_Button;
        [UIFieldInit("ChoiceText")]
        public SFramework.Core.UI.External.TextMeshProUGUIEx ChoiceText;


        private Action OnClick;

        protected override void OnAwake()
        {
            this.ChoiceItem_Button.onClick.AddListener(() => this.OnClick?.Invoke());
        }

        public void PoolSetData(SDSDialogueChoiceData data)
        {
            this.ChoiceText.text = data.Text;
        }

        public void SetClickCallback(Action callback)
        {
            this.OnClick = callback;
        }
    }
}
