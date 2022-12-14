using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public abstract partial class UIItemBase : UIObject
    {
        private UIItemSelector selector;

        public override EnumViewState UIState 
        {
            get
            {
                if (this.gameObject == null || this.gameObject.Equals(null))
                    return EnumViewState.Disposed;
                else
                {
                    if (base.UIState == EnumViewState.Disposed)
                        return base.UIState;
                    else
                        return this.gameObject.activeInHierarchy ? EnumViewState.Shown : EnumViewState.Hidden;
                }
            }
            protected set => base.UIState = value; 
        }

        public string BundleName { get; set; }

        public sealed override void Awake(GameObject gameObjectHost)
        {
            base.Awake(gameObjectHost);

            this.selector = gameObjectHost.GetComponent<UIItemSelector>();
            if (this.selector == null)
            {
                this.selector = gameObjectHost.AddComponent<UIItemSelector>();
                this.selector.SelectClass = this.ClassType.FullName;
            }

            this.selector.OnGameObjectEnable = this.OnGameObjectEnable;
            this.selector.OnGameObjectDisable = this.OnGameObjectDisable;

            if (this.selector.gameObject.activeInHierarchy)
            {
                this.OnShow();
            }
        }

        public sealed override void Dispose()
        {
            try
            {
                if(this.UIState == EnumViewState.Shown)
                    this.OnDisable();

                base.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public sealed override void Show()
        {
            if (this.gameObject == null)
                return;
            this.gameObject.SetActive(true);
        }

        public sealed override void Hide()
        {
            if (this.gameObject == null)
                return;
            this.gameObject.SetActive(false);
        }

        private void OnGameObjectEnable()
        {
            this.OnShow();
        }

        private void OnGameObjectDisable()
        {
            this.OnDisable();
        }

        /// <summary> ??????Hide()???????????????Unity??????OnDisable????????????????????????????????????????????? </summary>
        protected sealed override void OnDisable()
        {
            base.OnDisable();
        }
    }
}