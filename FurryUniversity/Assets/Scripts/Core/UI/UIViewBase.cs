using SFramework.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public abstract class UIViewBase : UIObject
    {
        public EnumUIType UIType { get; set; }

        private RectTransform visualRoot;
        private CanvasGroup rootCanvas;

        protected ViewMask Mask { get; private set; }

        protected override RectTransform VisualRoot => this.visualRoot;

        public sealed override async void Awake(GameObject gameObjectHost)
        {
            this.CreateVisualRoot(gameObjectHost);
            await this.CreateMask(gameObjectHost);
            base.Awake(gameObjectHost);
            this.rootCanvas = this.gameObject.AddComponent<CanvasGroup>();
        }

        #region 内部方法
        private void CreateVisualRoot(GameObject gameObjectHost)
        {
            List<Transform> allChildrens = new List<Transform>();
            for (int i = 0; i < gameObjectHost.transform.childCount; i++)
            {
                allChildrens.Add(gameObjectHost.transform.GetChild(i));
            }

            GameObject visualRootGO = new GameObject("[VisualRoot]");
            this.visualRoot = visualRootGO.AddComponent<RectTransform>();
            this.visualRoot.SetParent(gameObjectHost.transform);
            this.visualRoot.localPosition = Vector3.zero;
            this.visualRoot.localScale = Vector3.one;
            this.visualRoot.anchorMin = Vector3.zero;//全屏拉伸
            this.visualRoot.anchorMax = Vector3.one;
            this.visualRoot.offsetMin = Vector3.zero;
            this.visualRoot.offsetMax = Vector3.zero;

            for (int i = 0, count = allChildrens.Count; i < count; i++)
            {
                Transform child = allChildrens[i];
                child.SetParent(this.VisualRoot);
            }
        }

        private async STask CreateMask(GameObject gameObjectHost)
        {
            this.Mask = await this.CreateChildItemAsync<ViewMask>(UIItemBase.AssetList.ViewMask, gameObjectHost.transform);
            RectTransform maskRect = this.Mask.gameObject.transform as RectTransform;
            //伸展运动
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.anchoredPosition3D = Vector2.zero;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.one;

            maskRect.localScale = Vector3.one;
            this.Mask.gameObject.transform.SetAsFirstSibling();
            this.Mask.OnMaskClicked += this.OnMaskClicked;
        }

        private void OnMaskClicked()
        {
            this.OnClickMask();
        }
        #endregion

        #region 外部接口
        protected virtual void OnClickMask()
        {
            if (this.UIType == EnumUIType.Window)
                this.Hide();
        }
        #endregion
    }
}