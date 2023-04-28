using SFramework.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class UIViewBase : UIObject
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
                this.CalWindowShow();
            }
        }

        #region 内部方法
        private void CreateVisualRoot(GameObject gameObjectHost)
        {
            List<Transform> allChildren = new List<Transform>();
            for (int i = 0; i < gameObjectHost.transform.childCount; ++i)
            {
                allChildren.Add(gameObjectHost.transform.GetChild(i));
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

            for (int i = 0, count = allChildren.Count; i < count; ++i)
            {
                Transform child = allChildren[i];
                child.SetParent(this.VisualRoot);
            }
        }

        private async STask CreateMask(GameObject gameObjectHost)
        {
            this.Mask = await this.CreateChildItemAsync<ViewMask>(UIItemBase.AssetList.ViewMask,
                gameObjectHost.transform);
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
        /// <summary> 显示View </summary>
        public override async STask ShowAsync()
        {
            if(this.UIType == EnumUIType.Page)
                await this.UIManager.SetPageEnable(this.ClassType);

            if (this.UIState == EnumViewState.Shown)
                return;

            this.Mask.gameObject.SetActive(true);
            this.Mask.EnableRaycast = false;

            if (this is IUIPrepareShow uiPrepareShow)
            {
                await uiPrepareShow.OnPrepareShow();
                this.rootCanvas.alpha = 1;
                this.rootCanvas.blocksRaycasts = true;
                
                this.UIManager.UnblockUI();
            }
            
            this.UIState = EnumViewState.Shown;
            if (this.UIType == EnumUIType.Page)
                await this.UIManager.SetPageShow(this.ClassType);
            else
                this.UIManager.SetWindowActive(this.ClassType);

            this.Mask.EnableRaycast = true;
            this.OnEnable();
        }
        
        public override async STask HideAsync()
        {
            if (this.UIState == EnumViewState.Disposed)
            {
                Debug.LogWarning($"{this} Hide请求失败，物体已被销毁");
                return;
            }

            if (this.UIState == EnumViewState.Hidden)
                return;

            if (this.UIType == EnumUIType.Page)
            {
                if (!this.UIManager.IsLastPage(this))
                {
                    await this.UIManager.SetPageHide(this.ClassType);//这里会调用OnDisable
                }
                else
                {
                    Debug.LogWarning($"{this} Dispose请求失败，限制唯一的Page Hide");
                }
            }
            else
            {
                this.UIManager.SetWindowInactive(this.ClassType);
                this.UIState = EnumViewState.Hidden;
                this.OnDisable();
            }
            
            if(this.Mask.gameObject != null && this.UIState != EnumViewState.Shown)
                this.Mask.gameObject.SetActive(false);
            await this.UIManager.UpdateUIInstanceLimitAsync();
        }

        public sealed override async STask DisposeAsync()
        {
            try
            {
                if (this.UIType == EnumUIType.Page && this.UIState == EnumViewState.Shown)
                    await this.UIManager.SetPageHide(this.ClassType);//不要忘了显示上一个Page
                
                this.UIManager.RemoveUI(this.ClassType);
                
                if(this.UIState != EnumViewState.Hidden) // Window View
                    this.OnDisable();

                await base.DisposeAsync();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        protected virtual void OnClickMask()
        {
            if (this.UIType == EnumUIType.Window)
                this.HideAsync().Forget();
        }
        
        /// <summary>
        /// 若当前依附的Page没有显示时，自己也要隐藏
        /// </summary>
        public void CalWindowShow()
        {
            if (this.UIType == EnumUIType.Page)
                return;

            if (string.IsNullOrEmpty(this.BasedPage))
                return;

            if (this.BasedPage != this.UIManager.GetCurrentPageName())
            {
                this.rootCanvas.alpha = 0;
                this.rootCanvas.blocksRaycasts = false;
            }
            else
            {
                this.rootCanvas.alpha = 1;
                this.rootCanvas.blocksRaycasts = true;
            }
        }
        #endregion
        
        public sealed override async STask AwakeAsync(GameObject gameObjectHost)
        {
            await base.AwakeAsync(gameObjectHost);
            this.CreateVisualRoot(gameObjectHost);
            await this.CreateMask(gameObjectHost);
            this.rootCanvas = this.gameObject.AddComponent<CanvasGroup>();

            if (this is IUIPrepareShow)
            {
                this.rootCanvas.alpha = 0;
                this.rootCanvas.blocksRaycasts = false;
                
                this.UIManager.BlockUI();
            }
        }

        /// <summary>
        /// just set state shown without external logic，显示上一个Page时会调用
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