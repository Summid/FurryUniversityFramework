using SFramework.Core.GameManagers;
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

        public delegate void PageHandleDelVoid(Type type);
        public delegate bool PageHandleDelBool(Type type);

        public PageHandleDelVoid SetPageEnable;
        public PageHandleDelVoid SetPageShow;
        public PageHandleDelVoid SetUIActive;
        public PageHandleDelBool SetPageHide;
        public PageHandleDelVoid SetUIDisactive;
        public PageHandleDelVoid RemoveUI;

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
                this.CalWindowShow();//判断下自己是否应该显示
            }
        }

        public sealed override async STask AwakeAsync(GameObject gameObjectHost)
        {
            base.Awake(gameObjectHost);
            this.CreateVisualRoot(gameObjectHost);
            await this.CreateMask(gameObjectHost);
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

        /// <summary>
        /// 显示UI
        /// </summary>
        public sealed override void Show()
        {
            if (this.UIType == EnumUIType.Page)
                this.SetPageEnable?.Invoke(this.ClassType);

            if (this.UIState == EnumViewState.Shown)
                return;
            if(this.UIState == EnumViewState.Hidding)
                this.OnDisable();

            this.Mask.gameObject.SetActive(true);
            this.Mask.EnableRaycast = false;
            this.UIState = EnumViewState.Shown;
            if (this.UIType == EnumUIType.Page)
                this.SetPageShow?.Invoke(this.ClassType);
            else
            {
                this.SetUIActive?.Invoke(this.ClassType);
            }
            this.OnWillShow();
            this.Mask.EnableRaycast = true;
            this.OnEnable();
        }

        /// <summary>
        /// 隐藏UI
        /// </summary>
        public override void Hide()
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
                if (!GameManager.Instance.UIManager.IsLastPage(this))
                {
                    this.UIState = EnumViewState.Hidding;

                    if (!GameManager.Instance.UIManager.InternalHanding)
                        this.OnWillHide();

                    //Hide流程被取消
                    if (this.UIState != EnumViewState.Hidding)
                        return;

                    this.SetPageHide?.Invoke(this.ClassType);
                }
                else
                {
                    Debug.LogWarning($"{this} Dispose请求失败，限制唯一的Page Hide");
                }
            }
            else
            {
                this.UIState = EnumViewState.Hidding;
                if (!GameManager.Instance.UIManager.InternalHanding)
                    this.OnWillHide();

                if (this.UIState != EnumViewState.Hidding)
                    return;

                this.SetUIDisactive?.Invoke(this.ClassType);
                this.UIState = EnumViewState.Hidden;
                this.OnDisable();
            }
            if (this.Mask.gameObject != null && this.UIState != EnumViewState.Shown)
                this.Mask.gameObject.SetActive(false);
            GameManager.Instance.UIManager.UpdateUIInstanceLimit();
        }

        public sealed override void Dispose()
        {
            try
            {
                if (this.UIType == EnumUIType.Page && this.UIState == EnumViewState.Shown)
                    this.SetPageHide?.Invoke(this.ClassType);

                this.RemoveUI?.Invoke(this.ClassType);

                if(this.UIState != EnumViewState.Hidden)
                    this.OnDisable();

                base.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        /// <summary>
        /// 调用 <see cref="Show"/> 时触发，可在此处理View弹出动画等逻辑
        /// </summary>
        protected virtual void OnWillShow()
        {

        }

        /// <summary>
        /// 调用 <see cref="Hide"/> 时触发，可再此处理View关闭动画等逻辑
        /// </summary>
        protected virtual void OnWillHide()
        {

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

            if (this.BasedPage != GameManager.Instance.UIManager.GetCurrentPageName())
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
        
        /// <summary>
        /// just set state show state without external logic
        /// </summary>
        internal void SetStateShow()
        {
            if(this.UIState != EnumViewState.Shown)
                this.UIState = EnumViewState.Shown;
            this.OnEnable();
        }

        /// <summary>
        /// just set state hide state without external logic
        /// </summary>
        internal void SetStateHide()
        {
            if (this.UIState == EnumViewState.Hidden)
                return;

            this.UIState = EnumViewState.Hidding;
            this.OnWillHide();

            if (this.UIState != EnumViewState.Hidding)
                return;
            this.UIState = EnumViewState.Hidden;
            this.OnDisable();
        }

        /// <summary>
        /// 调用Show后触发，底层使用不暴露给用户
        /// </summary>
        protected sealed override void OnEnable()
        {
            base.OnEnable();
        }

        /// <summary>
        /// 调用Hide后触发，底层使用不暴露给用户
        /// </summary>
        protected sealed override void OnDisable()
        {
            base.OnDisable();
        }
    }
}