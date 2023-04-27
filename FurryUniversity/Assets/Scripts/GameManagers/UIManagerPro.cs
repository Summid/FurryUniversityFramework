using SFramework.Core.UI;
using SFramework.Core.UI.External;
using SFramework.Threading.Tasks;
using SFramework.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SFramework.Core.GameManagers
{
    public partial class UIManagerPro : GameManagerBase
    {
        public class UIInstanceInfo
        {
            public UIViewBasePro ViewInstance;
            public string ViewName;
        }
        
        private bool isInited;
        private UIInfoList uiList;
        /// <summary> 加载后的View保存在这 </summary>
        private Dictionary<Type, UIInstanceInfo> uiInstances = new Dictionary<Type, UIInstanceInfo>();
        /// <summary> 根据View显示顺序排序，越后显示的排在越后 </summary>
        private List<UIInstanceInfo> uiInstanceLoadOrderList = new List<UIInstanceInfo>();
        /// <summary> Page页面跳转顺序记录，越后显示的排在越后，UI销毁不会将其移除出该数组 </summary>
        private List<Type> navigateQueue = new List<Type>();
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
                this.UpdateUIInstanceLimitAsync().Forget();
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
            UIUtility.SetRectTransformParentWithStretchRootCanvas(this.uiWindowRoot,this.uiRoot);
            this.uiTopWindowRoot = new GameObject("[TopWindowRoot]").AddComponent<RectTransform>();
            UIUtility.SetRectTransformParentWithStretchRootCanvas(this.uiTopWindowRoot,this.uiRoot);

            GameObject.DontDestroyOnLoad(this.uiRoot.gameObject);

            this.isInited = true;
            
            this.UIInstanceCacheLimitCount = 0;//暂定这么多个缓存

            Debug.Log("UIManager Initialized");
            
            //TODO 刘海屏设备清单加载
            //设置异形屏适配像素
            this.ScreenCutOffRange = PlayerPrefsTool.ScreenAdaptation_Value.GetValue();
            
            //默认显示界面
            // this.ShowUIAsync<LoginView>().Forget();
        }

        #region 外部接口

        public async STask<TViewInstance> ShowUIAsync<TViewInstance>(IProgress<float> progress = null)
            where TViewInstance : UIViewBasePro, new()
        {
            if (!this.isInited)
                return null;

            Type viewType = typeof(TViewInstance);
            return await this.ShowUIInternalAsync(viewType, progress) as TViewInstance;
        }

        /// <summary>
        /// 异步卸载AB包
        /// </summary>
        /// <param name="uiObj"></param>
        public async STask DisposeUIBundleAsync(UIObjectPro uiObj)
        {
            Type uiType = uiObj.GetType();

            string bundleName = null;
            if (uiObj is UIViewBasePro)
            {
                bundleName = this.GetUIViewInfo(uiType)?.ViewAssetBundleName;
                if (this.uiInstances.TryGetValue(uiType, out UIInstanceInfo uiInfo))
                {
                    this.uiInstances.Remove(uiType);
                    this.uiInstanceLoadOrderList.Remove(uiInfo);
                }
            }
            else if (uiObj is UIItemBasePro itemBasePro)
            {
                bundleName = itemBasePro.BundleName;
            }

            if (bundleName != null)
            {
                await AssetBundleManager.UnloadAssetBundleAsync(bundleName);
            }
        }
        
        /// <summary>
        /// 根据Type获取UIViewInfo
        /// </summary>
        /// <param name="uiObjectType"></param>
        /// <returns></returns>
        public UIViewInfo GetUIViewInfo(Type uiObjectType)
        {
            string targetTypeName = uiObjectType.FullName;
            UIViewInfo targetUIInfo = this.uiList.GetUIViewInfoByTypeName(targetTypeName);
            return targetUIInfo;
        }

        /// <summary>
        /// 根据资源名获取UIItemInfo
        /// </summary>
        /// <param name="itemAsset"></param>
        /// <returns></returns>
        public UIItemInfo GetUIItemInfo(string itemAsset)
        {
            return this.uiList.GetUIItemInfoByAssetNameKey(itemAsset);
        }

        /// <summary>
        /// 获取Page导航队列中最后一个Page对象名
        /// </summary>
        /// <returns></returns>
        public string GetCurrentPageName()
        {
            if (this.navigateQueue.Count == 0)
                return null;

            Type type = this.navigateQueue[this.navigateQueue.Count - 1];
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

        /// <summary>
        /// Page导航队列是否已经无法后退（队列中不超过一个Page）
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public bool IsLastPage(UIViewBasePro page)
        {
            int index = this.navigateQueue.IndexOf(page.ClassType);
            return index == 0;
        }

        /// <summary>
        /// 清理多余UI缓存
        /// </summary>
        public async STask UpdateUIInstanceLimitAsync()
        {
            if (this.InternalHanding)
                return;
            
            try
            {
                //拷贝一份处于Hidden状态的View
                List<UIInstanceInfo> cacheList = this.uiInstanceLoadOrderList
                    .Where(info => info.ViewInstance.UIState == UIObjectPro.EnumViewState.Hidden).ToList();
                int needRemoveCount = cacheList.Count - this.UIInstanceCacheLimitCount;
                int index = 0;
                this.InternalHanding = true;
                while (needRemoveCount > 0)
                {
                    var uiInfoNeedRemove = cacheList[index];
                    Debug.Log($"<color=#00ff00>超出UI缓存最大限制:{this.UIInstanceCacheLimitCount}，移除UI实例:[{uiInfoNeedRemove.ViewName}]</color>");
                    await uiInfoNeedRemove.ViewInstance.DisposeAsync();
                    needRemoveCount--;
                    index++;
                }
            }
            finally
            {
                this.InternalHanding = false;
            }
        }

        public UIInstanceInfo GetShowingUI<TViewInstance>() where TViewInstance : UIViewBasePro
        {
            Type type = typeof(TViewInstance);
            return this.uiInstances.TryGetValue(type, out var info) ? info : null;
        }

        public UIInstanceInfo GetShowingUI(Type type)
        {
            return this.uiInstances.TryGetValue(type, out UIInstanceInfo info) ? info : null;
        }
        #endregion

        #region 内部方法
        /// <summary>
        /// 从缓存或资源文件中获取View
        /// </summary>
        /// <param name="viewType"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        private async STask<UIViewBasePro> ShowUIInternalAsync(Type viewType, IProgress<float> progress = null)
        {
            UIViewBasePro view = null;

            if (this.uiInstances.TryGetValue(viewType, out UIInstanceInfo uiInfo))
            {
                //加载过，缓存中有

                view = uiInfo.ViewInstance;
                //当一个界面Show的时候，将该界面在队列中的顺序提高到最后
                this.uiInstanceLoadOrderList.Remove(uiInfo);
                this.uiInstanceLoadOrderList.Add(uiInfo);
            }
            else
            {
                //需要从文件中加载
                UIViewInfo targetUIInfo = this.GetUIViewInfo(viewType);
                if (targetUIInfo != null)
                {
                    if (targetUIInfo.ViewAssetBundleName != null && targetUIInfo.ViewAssetName != null)
                    {
                        view = await this.InitViewAsync(targetUIInfo.ViewAssetBundleName, targetUIInfo.ViewAssetName,
                            viewType, targetUIInfo.ViewType, progress);
                    }
                }
            }

            if (view.UIType == EnumUIType.Window && this.navigateQueue.Count > 0)
            {
                view.BasedPage = this.navigateQueue[this.navigateQueue.Count - 1].Name;
            }
            this.uiInstances[viewType].ViewInstance.gameObject.transform.SetAsLastSibling();

            await view.ShowAsync();
            return view;
        }

        /// <summary>
        /// 从文件中加载View资源
        /// </summary>
        /// <param name="viewBundleName"></param>
        /// <param name="viewPrefabName"></param>
        /// <param name="viewType"></param>
        /// <param name="uiType"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        private async STask<UIViewBasePro> InitViewAsync(string viewBundleName, string viewPrefabName, Type viewType,
            EnumUIType uiType, IProgress<float> progress = null)
        {
            progress?.Report(0.1f);

            GameObject bundleGO =
                await AssetBundleManager.LoadAssetInAssetBundleAsync<GameObject>(viewPrefabName, viewBundleName);
            
            progress?.Report(0.5f);

            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGO);
            UIViewBasePro view = Activator.CreateInstance(viewType) as UIViewBasePro;
            view.UIType = uiType;
            UIUtility.SetRectTransformParentWithStretchRootCanvas(gameObject.transform as RectTransform,
                view.Topmost ? this.uiTopWindowRoot : this.uiWindowRoot);

            UIInstanceInfo uiInstanceInfo = new UIInstanceInfo { ViewInstance = view, ViewName = viewType.Name };
            this.uiInstances.Add(viewType, uiInstanceInfo);
            this.uiInstanceLoadOrderList.Add(uiInstanceInfo);
            gameObject.SetActive(false);
            await view.AwakeAsync(gameObject);
            
            progress?.Report(0.8f);

            return view;
        }

        /// <summary>
        /// 释放UI资源时，需要Mgr做的事（销毁相关逻辑在UIObject.Dispose中）
        /// </summary>
        /// <param name="viewType"></param>
        internal void RemoveUI(Type viewType)
        {
            if (this.uiInstances.TryGetValue(viewType, out UIInstanceInfo uiInfo))
            {
                this.uiInstances.Remove(viewType);
                this.uiInstanceLoadOrderList.Remove(uiInfo);
            }
        }
        
        /// <summary>
        /// 隐藏Window，需要Mgr做的事；若当前Window已处于隐藏状态，则不会调用
        /// </summary>
        /// <param name="viewType"></param>
        internal void SetWindowInactive(Type viewType)
        {
            if (this.uiInstances.TryGetValue(viewType, out UIInstanceInfo uiInfo))
            {
                uiInfo.ViewInstance.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 显示Window，需要Mgr做的事；若当前Window已处于显示状态，则不会调用
        /// </summary>
        /// <param name="viewType"></param>
        internal void SetWindowActive(Type viewType)
        {
            if (this.uiInstances.TryGetValue(viewType, out UIInstanceInfo uiInfo))
            {
                uiInfo.ViewInstance.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 隐藏Page，需要Mgr做的事 —— 显示上一个Page（若有）；若当前Page已处于隐藏状态，则不会调用
        /// </summary>
        /// <param name="viewType"></param>
        internal async STask SetPageHide(Type viewType)
        {
            while (this.InternalHanding)
            {
                await STask.NextFrame();
            }

            //有上一个Page才设置
            if (this.navigateQueue.Count <= 1)
                return;

            UIInstanceInfo willHidePage = this.uiInstances[viewType];
            willHidePage.ViewInstance.gameObject.SetActive(false);
            willHidePage.ViewInstance.SetStateHide();

            this.navigateQueue.RemoveAt(this.navigateQueue.Count - 1);

            Type preType = this.navigateQueue[this.navigateQueue.Count - 1];
            if (!this.uiInstances.ContainsKey(preType))
            {
                //上一个Page被清理掉了，重新加载
                await this.ShowUIInternalAsync(preType);
            }
            else
            {
                //缓存中有上一个Page，直接显示
                UIInstanceInfo preUIInstance = this.uiInstances[this.navigateQueue[this.navigateQueue.Count - 1]];
                preUIInstance.ViewInstance.gameObject.SetActive(true);
                preUIInstance.ViewInstance.SetStateShow();
            }
        }
        
        /// <summary>
        /// 显示Page前，需要Mgr做的事 —— 隐藏上一个Page，显示当前gameObject；若当前Page已处于显示状态，则不会调用
        /// </summary>
        /// <param name="viewType"></param>
        internal async STask SetPageShow(Type viewType)
        {
            while (this.InternalHanding)
            {
                await STask.NextFrame();
            }

            //隐藏上一个Page
            for (int i = 0, count = this.navigateQueue.Count; i < count; ++i)
            {
                Type type = this.navigateQueue[i];
                if (type == viewType)
                    continue;
                if (this.uiInstances.TryGetValue(type, out UIInstanceInfo uiInstanceInfo))
                {
                    uiInstanceInfo.ViewInstance.gameObject.SetActive(false);
                    uiInstanceInfo.ViewInstance.SetStateHide();
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
            
            //显示gameObject
            UIViewBasePro viewInstance = this.uiInstances[viewType].ViewInstance;
            viewInstance.gameObject.SetActive(true);
        }

        /// <summary>
        /// 显示Page前，需要Mgr做的事情；即使当前Page已处于显示状态，也会调用
        /// </summary>
        /// <param name="type"></param>
        internal async STask SetPageEnable(Type type)
        {
            await this.HideDependencyWindowAsync(type);
        }

        /// <summary>
        /// 将依附于当前Page的View都隐藏掉
        /// </summary>
        /// <param name="viewType"></param>
        private async STask HideDependencyWindowAsync(Type viewType)
        {
            UIInstanceInfo page = this.uiInstances[viewType];

            List<UIInstanceInfo> hideInfo = new List<UIInstanceInfo>();
            for (int i = 0, count = this.uiInstanceLoadOrderList.Count; i < count; ++i)
            {
                UIInstanceInfo uiInfo = this.uiInstanceLoadOrderList[i];
                if (uiInfo.ViewInstance.BasedPage == page.ViewName)
                {
                    hideInfo.Add(uiInfo);
                }
            }

            for (int i = 0, count = hideInfo.Count; i < count; ++i)
            {
                await hideInfo[i].ViewInstance.HideAsync();
            }
        }
        #endregion
    }
}
