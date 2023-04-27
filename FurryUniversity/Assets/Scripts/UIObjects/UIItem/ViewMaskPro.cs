using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SFramework.Core.UI
{
    public class ViewMaskPro : UIItemBasePro
    {
        [UIFieldInit("bgMask")]
        public Image bgMask_Image;
        [UIFieldInit("bgMask")]
        public Button bgMask_Button;


        public delegate void OnMaskClickHandle();
        public event OnMaskClickHandle OnMaskClicked;

        //后期可添加更多mask，通过该属性获取当前激活的mask
        private Graphic CurrentMask
        {
            get
            {
                return this.bgMask_Image;
            }
        }

        /// <summary>
        /// 设置mask透明度
        /// </summary>
        public float Alpha
        {
            get => this.CurrentMask.color.a;
            set
            {
                Color color = this.CurrentMask.color;
                color.a = value;
                this.CurrentMask.color = color;
            }
        }

        /// <summary>
        /// 设置mask是否可被点击
        /// </summary>
        public bool EnableRaycast
        {
            get => this.CurrentMask.raycastTarget;
            set => this.CurrentMask.raycastTarget = value;
        }

        /// <summary>
        /// 设置mask是否可见
        /// </summary>
        public bool Visible
        {
            get => this.CurrentMask.gameObject.activeSelf;
            set => this.CurrentMask.gameObject.SetActive(value);
        }
        
        protected override void OnAwake()
        {
            this.bgMask_Button.onClick.AddListener(() =>
            {
                this.OnMaskClicked?.Invoke();
            });
        }
    }
}
