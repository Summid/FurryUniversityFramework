using SFramework.Core.UI;
using SFramework.Core.UI.External;
using SFramework.Threading.Tasks;
using SFramework.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SFramework.Core.GameManagers
{
    public class UIManager : GameManagerBase
    {
        public class UIInstanceInfo
        {
            public UIViewBase ViewInstance;
            public string ViewName;
        }

        private bool isInited;
        private UIInfoList uiList;
        private Dictionary<Type,UIInstanceInfo> uiInstances = new Dictionary<Type,UIInstanceInfo>();//加载后的View保存在这
        private List<UIInstanceInfo> uiInstanceLoadOrderList = new List<UIInstanceInfo>();//根据View显示顺序排序，越后显示的排在越后
        private List<Type> navigateQueue = new List<Type>();//Page页面跳转顺序记录
        private Transform uiRoot;
        private CanvasGroup canvasGroup;
        private RectTransform uiWindowRoot;
        private RectTransform uiTopWindowRoot;
        private float screenCutOffRange;//异形屏适配像素

        public Canvas UIRootCanvas { get; private set; }
        /// <summary> 当统一处理UI时，若涉及到修改uiInstances等成员，通过该标志位判断处理状态 </summary>
        public bool InternalHanding { get; private set; }
        private int uiInstanceCacheLimitCount = int.MaxValue;
        public int UIInstanceCacheLimitCount
        {
            get => this.uiInstanceCacheLimitCount;
            private set
            {
                this.uiInstanceCacheLimitCount = value;
                this.UpdateUIInstanceLimit();
            }
        }

        protected override async void OnInitialized()
        {
            if (this.isInited)
                return;

            //加载UI清单
            this.uiList = await AssetBundleManager.LoadAssetInAssetBundleAsync<UIInfoList>(StaticVariables.UIListName, StaticVariables.UIListBundleName);
            Debug.Log($"uiList in UIManager {this.uiList}");
            if (this.uiList == null)
                return;

            //初始化UI节点
            this.uiRoot = GameObject.Find("UIRoot").transform;
            this.UIRootCanvas = this.uiRoot.GetComponent<Canvas>();
            this.canvasGroup = this.uiRoot.GetComponent<CanvasGroup>();
            this.canvasGroup.blocksRaycasts = true;
            this.uiWindowRoot = new GameObject("[Window]").AddComponent<RectTransform>();
            SetRectTransformParentWithStretchRootCanvas(this.uiWindowRoot, this.uiRoot);
            this.uiTopWindowRoot = new GameObject("[TOPWINDOWROOT]").AddComponent<RectTransform>();
            SetRectTransformParentWithStretchRootCanvas(this.uiTopWindowRoot, this.uiRoot);

            GameObject.DontDestroyOnLoad(this.uiRoot.gameObject);

            this.isInited = true;

            this.UIInstanceCacheLimitCount = 0;//暂定这么多个缓存

            Debug.Log("UIManager Initialized");

            //TODO 刘海屏设备清单加载
            //设置异形屏适配像素
            this.ScreenCutOffRange = PlayerPrefsTool.ScreenAdaptation_Value.GetValue();
            
            //默认显示界面
            this.ShowUIAsync<LoginView>().Forget();
        }

        #region 外部接口
        /// <summary>
        /// 显示UI View
        /// </summary>
        /// <typeparam name="ViewInstance"></typeparam>
        /// <param name="showImmediately">为false时，加载完毕资源后不会立即显示，会调用Awake但不会调用Show</param>
        /// <param name="progress">加载完成后会调用Report(0.8)</param>
        /// <returns></returns>
        public async STask<ViewInstance> ShowUIAsync<ViewInstance>(bool showImmediately = true, IProgress<float> progress = null) where ViewInstance : UIViewBase, new()
        {
            if (!this.isInited)
                return null;

            Type viewType = typeof(ViewInstance);
            return await this.ShowUIInternalAsync(viewType, showImmediately, progress) as ViewInstance;
        }

        public UIViewInfo GetUIViewInfo(Type uiObjectType)
        {
            string targetTypeName = uiObjectType.FullName;
            UIViewInfo targetUIInfo = this.uiList.GetUIViewInfoByTypeName(targetTypeName);
            return targetUIInfo;
        }

        public UIItemInfo GetUIItemInfo(string itemAsset)
        {
            return this.uiList.GetUIItemInfoByAssetNameKey(itemAsset);
        }

        public void DisposeUIBundle(UIObject uiObj)
        {
            Type uiType = uiObj.GetType();

            string bundleName = null;
            if (uiObj is UIViewBase)
            {
                bundleName = this.GetUIViewInfo(uiType)?.ViewAssetBundleName;
                if (this.uiInstances.TryGetValue(uiType, out UIInstanceInfo uiInfo))
                {
                    this.uiInstances.Remove(uiType);
                    this.uiInstanceLoadOrderList.Remove(uiInfo);
                }
            }
            else if (uiObj is UIItemBase itemBase)
            {
                bundleName = itemBase.BundleName;
            }

            if (bundleName != null)
            {
                AssetBundleManager.UnloadAssetBundleAsync(bundleName).Forget();
            }
        }

        /// <summary>
        /// 以拉伸填充的方式将rectTrans设置到一个父节点中
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="parent"></param>
        public static void SetRectTransformParentWithStretchRootCanvas(RectTransform rect, Transform parent)
        {
            rect.SetParent(parent);

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition3D = Vector3.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            rect.localScale = Vector3.one;
        }

        /// <summary>
        /// Page导航队列是否已经无法后退（队列中不超过一个Page）
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public bool IsLastPage(UIViewBase page)
        {
            int index = this.navigateQueue.IndexOf(page.ClassType);
            return index == 0;
        }

        public void UpdateUIInstanceLimit()
        {
            if (this.InternalHanding)
                return;

            try
            {
                //拷贝一份处于Hidden状态的View
                List<UIInstanceInfo> cacheList = this.uiInstanceLoadOrderList.Where(info => info.ViewInstance.UIState == UIObject.EnumViewState.Hidden).ToList();
                int needRemoveCount = cacheList.Count - this.UIInstanceCacheLimitCount;
                int index = 0;
                this.InternalHanding = true;
                while (needRemoveCount > 0)
                {
                    var uiInfoNeedRemove = cacheList[index];
                    Debug.Log($"<color=#00ff00>超出UI缓存最大限制:{this.UIInstanceCacheLimitCount}，移除UI实例:[{uiInfoNeedRemove.ViewName}]</color>");
                    uiInfoNeedRemove.ViewInstance.Dispose();
                    needRemoveCount--;
                    index++;
                }
            }
            finally
            {
                this.InternalHanding = false;
            }
        }

        /// <summary>
        /// 获取Page导航队列中最后一个Page对象名
        /// </summary>
        /// <returns></returns>
        public string GetCurrentPageName()
        {
            if (this.navigateQueue.Count == 0)
                return null;

            Type type = this.navigateQueue[navigateQueue.Count - 1];
            return type.Name;
        }

        /// <summary>
        /// 异形屏适配像素
        /// </summary>
        public float ScreenCutOffRange
        {
            get => this.screenCutOffRange;
            set
            {
                this.screenCutOffRange = value;
                PlayerPrefsTool.ScreenAdaptation_Value.SetValue(value);
                ForceStretch.CutOffRange = value;
                
                //left
                Vector2 temp = this.uiWindowRoot.offsetMin;//anchorMin到矩形左下角的距离
                temp.x = value;
                this.uiWindowRoot.offsetMin = temp;

                temp = this.uiTopWindowRoot.offsetMin;
                temp.x = value;
                this.uiTopWindowRoot.offsetMin = temp;
                
                //right
                temp = this.uiWindowRoot.offsetMax;//anchorMax到矩形右上角的距离
                temp.x = -value;
                this.uiWindowRoot.offsetMax = temp;

                temp = this.uiTopWindowRoot.offsetMax;
                temp.x = -value;
                this.uiTopWindowRoot.offsetMax = temp;
            }
        }
        
        public UIInstanceInfo GetShowingUI<ViewInstance>() where ViewInstance : UIViewBase
        {
            Type type = typeof(ViewInstance);
            return this.uiInstances.TryGetValue(type, out UIInstanceInfo info) ? info : null;
        }

        public UIInstanceInfo GetShowingUI(Type type)
        {
            return this.uiInstances.TryGetValue(type, out UIInstanceInfo info) ? info : null;
        }
        #endregion

        #region 内部方法
        private async STask<UIViewBase> ShowUIInternalAsync(Type viewType, bool showImmediately = true, IProgress<float> progress = null)
        {
            UIViewBase view = null;

            if (this.uiInstances.TryGetValue(viewType, out UIInstanceInfo uiInfo))
            {
                view = uiInfo.ViewInstance;
                //当一个界面Show的时候，将该界面在队列中的顺序提高到最后
                this.uiInstanceLoadOrderList.Remove(uiInfo);
                this.uiInstanceLoadOrderList.Add(uiInfo);
            }
            else
            {
                UIViewInfo targetUIInfo = this.GetUIViewInfo(viewType);

                if (targetUIInfo != null)
                {
                    if (targetUIInfo.ViewAssetBundleName != null && targetUIInfo.ViewAssetName != null)
                    {
                        view = await this.InitViewAsync(targetUIInfo.ViewAssetBundleName, targetUIInfo.ViewAssetName, viewType, targetUIInfo.ViewType, progress);
                    }
                }
            }
            if (view.UIType == EnumUIType.Window && this.navigateQueue.Count > 0)
            {
                view.BasedPage = this.navigateQueue[this.navigateQueue.Count - 1].Name;
            }
            this.uiInstances[viewType].ViewInstance.gameObject.transform.SetAsLastSibling();
            if (showImmediately)
                view.Show();

            return view;
        }

        private async STask<UIViewBase> InitViewAsync(string viewBundleName, string viewPrefabName, Type viewType, EnumUIType uiType, IProgress<float> progress = null)
        {
            progress?.Report(0.1f);
            GameObject bundleGO = await AssetBundleManager.LoadAssetInAssetBundleAsync<GameObject>(viewPrefabName, viewBundleName);
            progress?.Report(0.5f);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGO);
            UIViewBase view = Activator.CreateInstance(viewType) as UIViewBase;
            view.UIType = uiType;
            SetRectTransformParentWithStretchRootCanvas(gameObject.transform as RectTransform, view.Topmost ? this.uiTopWindowRoot : this.uiWindowRoot);

            UIInstanceInfo uiInstanceInfo = new UIInstanceInfo { ViewInstance = view, ViewName = viewType.Name };
            this.uiInstances.Add(viewType, uiInstanceInfo);
            this.uiInstanceLoadOrderList.Add(uiInstanceInfo);

            this.SetViewUtilityFunc(view);

            gameObject.SetActive(false);
            await view.AwakeAsync(gameObject);
            progress?.Report(0.8f);

            return view;
        }

        private void SetViewUtilityFunc(UIViewBase view)
        {
            view.SetPageEnable = this.SetPageEnable;
            view.SetPageShow = this.SetPageShow;
            view.SetPageHide = this.SetPageHide;
            view.SetUIActive = this.SetUIActive;
            view.SetUIDisactive = this.SetUIDisactive;
            view.RemoveUI = this.RemoveUI;
        }

        /// <summary>
        /// 释放UI资源，并销毁相关GameObject（销毁相关逻辑在UIObject.Dispose中）
        /// </summary>
        /// <param name="viewType"></param>
        private void RemoveUI(Type viewType)
        {
            if (this.uiInstances.TryGetValue(viewType, out UIInstanceInfo uiInfo))
            {
                this.uiInstances.Remove(viewType);
                this.uiInstanceLoadOrderList.Remove(uiInfo);
            }
        }

        /// <summary>
        /// set window view active false
        /// </summary>
        /// <param name="viewType"></param>
        private void SetUIDisactive(Type viewType)
        {
            if (this.uiInstances.TryGetValue(viewType, out UIInstanceInfo uiInfo))
            {
                uiInfo.ViewInstance.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// set window view active true
        /// </summary>
        /// <param name="viewType"></param>
        private void SetUIActive(Type viewType)
        {
            if (this.uiInstances.TryGetValue(viewType, out UIInstanceInfo uiInfo))
            {
                uiInfo.ViewInstance.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 设置Page View隐藏，上一个Page自动显示
        /// </summary>
        /// <param name="viewType"></param>
        /// <returns></returns>
        private bool SetPageHide(Type viewType)
        {
            if (this.InternalHanding)
            {
                var willHidePage = this.uiInstances[viewType];
                willHidePage.ViewInstance.gameObject.SetActive(false);
                return true;
            }

            //有上一个Page才设置
            if (this.navigateQueue.Count >= 2)
            {
                var willHidePage = this.uiInstances[viewType];
                willHidePage.ViewInstance.gameObject.SetActive(false);
                willHidePage.ViewInstance.SetStateHide();

                this.navigateQueue.RemoveAt(this.navigateQueue.Count - 1);

                Type preType = this.navigateQueue[this.navigateQueue.Count - 1];
                if (!this.uiInstances.ContainsKey(preType))
                {
                    //上一个Page被清理掉了，重新加载
                    this.ShowUIInternalAsync(preType).Forget();
                }
                else
                {
                    //缓存中有上一个Page，直接显示
                    var preUIInstance = this.uiInstances[this.navigateQueue[this.navigateQueue.Count - 1]];
                    preUIInstance.ViewInstance.gameObject.SetActive(true);
                    preUIInstance.ViewInstance.SetStateShow();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 设置Page View显示，上一个Page隐藏
        /// </summary>
        /// <param name="viewType"></param>
        private void SetPageShow(Type viewType)
        {
            var viewInstance = this.uiInstances[viewType].ViewInstance;
            if (this.InternalHanding)
            {
                viewInstance.gameObject.SetActive(true);
                return;
            }

            //隐藏上一个Page
            for (int i = 0, count = this.navigateQueue.Count; i < count; i++)
            {
                Type type = this.navigateQueue[i];
                if (type == viewType)
                    continue;
                if (this.uiInstances.TryGetValue(type, out UIInstanceInfo uIInstanceInfo))
                {
                    uIInstanceInfo.ViewInstance.gameObject.SetActive(false);
                    uIInstanceInfo.ViewInstance.SetStateHide();
                }
            }

            //整理导航队列
            if (!this.navigateQueue.Contains(viewType))
            {
                //显示的是新Page，将新Page加入导航队列
                if (this.navigateQueue.Count > 0)
                {
                    //如果前一个Page是IsDisableNavigatePage，则把该Page移除导航队列
                    Type prePageType = this.navigateQueue[this.navigateQueue.Count - 1];
                    if (this.GetUIViewInfo(prePageType).IsDisableNavigatePage)
                    {
                        this.navigateQueue.Remove(prePageType);
                    }
                }
                this.navigateQueue.Add(viewType);
            }
            else
            {
                //如果显示的Page是之前显示过的，则把该Page之后的Page都给移除掉
                int targetIndex = this.navigateQueue.IndexOf(viewType);
                if (this.navigateQueue.Count > targetIndex + 1)
                {
                    this.navigateQueue.RemoveRange(targetIndex + 1, this.navigateQueue.Count - (targetIndex + 1));
                }
            }

            viewInstance.gameObject.SetActive(true);
        }

        private void SetPageEnable(Type type)
        {
            this.HideDependencyWindow(type);
        }

        /// <summary>
        /// 将依附于当前Page的View都隐藏掉
        /// </summary>
        /// <param name="viewType"></param>
        private void HideDependencyWindow(Type viewType)
        {
            var page = this.uiInstances[viewType];

            List<UIInstanceInfo> hideInfo = new List<UIInstanceInfo>();
            for (int i = 0, count = this.uiInstanceLoadOrderList.Count; i < count; ++i)
            {
                var uiInfo = this.uiInstanceLoadOrderList[i];
                if (uiInfo.ViewInstance.BasedPage == page.ViewName)
                {
                    hideInfo.Add(uiInfo);
                }
            }

            for (int i = 0, count = hideInfo.Count; i < count; ++i)
            {
                hideInfo[i].ViewInstance.Hide();
            }
        }
        #endregion
    }
}