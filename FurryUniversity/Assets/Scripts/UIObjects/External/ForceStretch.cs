using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI.External
{
    /// <summary>
    /// 让rectTransform恢复至异形屏适配前的宽度；会根据父节点的宽度来反向增加宽度，推荐将UI节点放在预制件root节点下
    /// </summary>
    public class ForceStretch : MonoBehaviour
    {
        public static float CutOffRange;
        private RectTransform rectTrans;
        private RectTransform parentRectTrans;
        
        void Start()
        {
            this.rectTrans = this.transform as RectTransform;
            this.parentRectTrans = this.rectTrans.parent as RectTransform;
            float width = this.rectTrans.rect.width;
            
            //设置左右居中模式，sizeDelta.x 为宽度
            this.rectTrans.anchorMin = new Vector2(0.5f, this.rectTrans.anchorMin.y);
            this.rectTrans.anchorMax = new Vector2(0.5f, this.rectTrans.anchorMax.y);
            this.rectTrans.sizeDelta = new Vector2(width, this.rectTrans.sizeDelta.y);//宽高不变
            this.Update();
        }

        void Update()
        {
            float desireWidth = this.parentRectTrans.rect.width + CutOffRange * 2;//跟着父节点改变宽度
            this.rectTrans.sizeDelta = new Vector2(desireWidth, this.rectTrans.sizeDelta.y);
        }
    }
}