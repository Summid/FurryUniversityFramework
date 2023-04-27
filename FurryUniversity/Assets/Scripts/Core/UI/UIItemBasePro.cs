using SFramework.Threading.Tasks;
using System;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class UIItemBasePro : UIObjectPro
    {
        private UIItemSelector selector;

        public override EnumViewState UIState
        {
            get
            {
                if (this.gameObject == null || this.gameObject.Equals(null))
                    return EnumViewState.Disposed;
                if (base.UIState == EnumViewState.Disposed)
                    return base.UIState;
                return this.gameObject.activeInHierarchy ? EnumViewState.Shown : EnumViewState.Hidden;
            }
            protected set => base.UIState = value;
        }

        /// <summary>
        /// BundleName不为空时才会在Dispose时卸载AB，目前在 <see cref="UIObject.CreateChildItemAsync{UIItem}(string, Transform)"/> 方法中会设置该变量
        /// </summary>
        public string BundleName { get; set; }

        public sealed override async STask AwakeAsync(GameObject gameObjectHost)
        {
            await base.AwakeAsync(gameObjectHost);

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
                this.OnEnable();
            }
        }

        public sealed override async STask DisposeAsync()
        {
            try
            {
                if(this.UIState == EnumViewState.Shown)
                    this.OnDisable();
                await base.DisposeAsync();
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

#pragma warning disable 1998
        public sealed override async STask ShowAsync()
        {
            if (this.gameObject != null)
                this.gameObject.SetActive(true);
        }

        public sealed override async STask HideAsync()
        {
            if(this.gameObject != null)
                this.gameObject.SetActive(false);
        }
#pragma warning restore 1998
        

        private void OnGameObjectEnable()
        {
            this.OnEnable();
        }

        private void OnGameObjectDisable()
        {
            this.OnDisable();
        }
        
        /// <summary>
        /// 调用Show后触发，底层使用不暴露给用户
        /// </summary>
        protected sealed override void OnEnable()
        {
            base.OnEnable();
        }

        /// <summary> 调用Hide()后触发，或Unity触发OnDisable时调用，底层使用，不暴露给用户 </summary>
        protected sealed override void OnDisable()
        {
            base.OnDisable();
        }
    }
}