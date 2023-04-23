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

        [Tooltip("The alignment of the Float Tip to the pointer position.")]
        public TextAnchor showAlignment;
        public FloatTipViewType floatTipViewType;

        public List<object> Parameters;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            switch (this.floatTipViewType)
            {
                case FloatTipViewType.ButtonsFloatTipView:
                    ButtonsFloatTipView.Pop(eventData.position, this.showAlignment, this.Parameters, this.title).Forget();
                    break;
                default:
                    Debug.LogError($"未实现 {this.floatTipViewType} 的逻辑");
                    break;
            }
        }
    }
}
