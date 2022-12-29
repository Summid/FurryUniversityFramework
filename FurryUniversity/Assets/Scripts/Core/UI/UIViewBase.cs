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


        protected override RectTransform VisualRoot => this.visualRoot;

        public sealed override void Awake(GameObject gameObjectHost)
        {
            this.CreateVisualRoot(gameObjectHost);

            base.Awake(gameObjectHost);
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

        private void CreateMask(GameObject gameObjectHost)
        {

        }
        #endregion
    }
}