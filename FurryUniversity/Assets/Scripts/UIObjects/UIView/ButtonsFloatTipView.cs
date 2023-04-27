using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using SFramework.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    [UIView("ButtonsFloatTipView", EnumUIType.Window)]
    public partial class ButtonsFloatTipView : UIViewBase
    {
        public class ButtonData
        {
            public int Index;
            public string ShowText;
            public bool DisableDivideLine;
            public bool Interactable;
            public Action<int> OnClick;
        }

        private RectTransform contentRectTrans;
        private List<ButtonData> datas = new List<ButtonData>();

        protected override void OnAwake()
        {
            this.contentRectTrans = this.Content_Image.GetComponent<RectTransform>();
        }

        public static async STaskVoid Pop(Vector3 screenPos, TextAnchor alignment, List<object> parameters, string title)
        {
            var view = await GameManager.Instance.UIManager.ShowUIAsync<ButtonsFloatTipView>();
            view.SetData(parameters, title);
            view.SetLayout(screenPos, alignment);
        }

        public void SetLayout(Vector3 screenPos, TextAnchor alignment)
        {
            Vector2 pivot = alignment.AnchorToPivot();
            Vector3 worldPos = screenPos.ScreenToWorldPosition(GameManager.Instance.UIManager.UIRootCanvas);

            //这里反转一下 pivot 坐标，用于限制浮窗的位置，感受一下
            pivot.x = 1 - pivot.x;
            pivot.y = 1 - pivot.y;
            this.contentRectTrans.pivot = pivot;
            this.contentRectTrans.position = worldPos;
            UIUtility.ClampToScreenRect(this.gameObject.transform as RectTransform, this.contentRectTrans);
        }

        public void SetData(List<object> parameters, string title)
        {
            if (parameters == null)
                return;

            this.datas.Clear();
            parameters.ForEach(param => this.datas.Add(param as ButtonData));
            
            this.TitleText.text = title;
            this.ButtonsNode_UIItemPool.UpdateList<ButtonData, ButtonsFloatTipItem>(this.datas).Forget();
        }
    }
}
