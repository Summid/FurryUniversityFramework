using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SFramework.Core.UI
{
    public enum FloatTipViewType
    {
        ButtonsFloatTipView
    }

    public class FloatTipInvoker : MonoBehaviour, IPointerClickHandler
    {
        public string title;

        public TextAnchor showAlignment;
        public FloatTipViewType floatTipViewType;
        

        private void OnClickInvoker()
        {
            switch (this.floatTipViewType)
            {
                case FloatTipViewType.ButtonsFloatTipView:
                    
                    break;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (this.floatTipViewType)
            {
                case FloatTipViewType.ButtonsFloatTipView:
                    ButtonsFloatTipView.Pop(eventData.position, this.showAlignment);
                    break;
                default:
                    Debug.LogError($"未实现 {this.floatTipViewType} 的逻辑");
                    break;
            }
        }
    }
}
