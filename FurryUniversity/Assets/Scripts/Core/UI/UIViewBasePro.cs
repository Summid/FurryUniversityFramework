using SFramework.Core.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class UIViewBasePro : UIObjectPro
    {
        public EnumUIType UIType { get; set; }

        private RectTransform visualRoot;
        private CanvasGroup rootCanvas;
        
        protected ViewMask Mask { get; private set; }
        /// <summary> 是否是顶部windows </summary>
        public virtual bool Topmost => false;

        protected override RectTransform VisualRoot => this.visualRoot;
        
        private string basedPage;
        /// <summary> 依附的Page的名称 </summary>
        public virtual string BasedPage
        {
            get => this.basedPage;
            set
            {
                if (this.basedPage == value)
                    return;
                this.basedPage = value;
                // this.CalWindowShow();//TODO 判断下自己是否应该显示
            }
        }



        /// <summary>
        /// just set state shown without external logic
        /// </summary>
        internal void SetStateShow()
        {
            if (this.UIState != EnumViewState.Shown)
                this.UIState = EnumViewState.Shown;
            this.OnEnable();
        }

        /// <summary>
        /// just set state hidden without external logic
        /// </summary>
        internal void SetStateHide()
        {
            if (this.UIState == EnumViewState.Hidden)
                return;

            this.UIState = EnumViewState.Hidden;
            this.OnDisable();
        }

        protected sealed override void OnEnable()
        {
            base.OnEnable();
        }

        protected sealed override void OnDisable()
        {
            base.OnDisable();
        }
    }
}