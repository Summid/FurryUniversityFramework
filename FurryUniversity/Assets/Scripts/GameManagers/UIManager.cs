using SFramework.Core.UI;
using SFramework.Threading.Tasks;
using SFramework.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private List<UIInstanceInfo> uiInstanceLoadOrderList = new List<UIInstanceInfo>();//根据View显示顺序排序
        private Transform uiRoot;
        private CanvasGroup canvasGroup;
        private RectTransform uiWindowRoot;
        private RectTransform uiTopWindowRoot;

        public Canvas UIRootCanvas { get; private set; }

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

            Debug.Log("UIManager Initialized");

            //TODO 刘海屏设备清单加载
        }

        #region 外部接口
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
            rect.offsetMax = Vector2.one;

            rect.localScale = Vector3.one;
        }
        #endregion

        #region 内部方法
        private UIViewBase ShowUI(Type viewType)
        {
            return default;
        }

        private async STask<UIViewBase> InitViewAsync(string viewBundleName, string viewPrefabName, Type viewType, EnumUIType uiType)
        {
            GameObject bundleGO = await AssetBundleManager.LoadAssetInAssetBundleAsync<GameObject>(viewPrefabName, viewBundleName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGO);
            UIViewBase view = Activator.CreateInstance(viewType) as UIViewBase;
            view.UIType = uiType;
            SetRectTransformParentWithStretchRootCanvas(gameObject.transform as RectTransform, view.Topmost ? this.uiTopWindowRoot : this.uiWindowRoot);

            UIInstanceInfo uiInstanceInfo = new UIInstanceInfo { ViewInstance = view, ViewName = viewType.Name };
            this.uiInstances.Add(viewType, uiInstanceInfo);
            this.uiInstanceLoadOrderList.Add(uiInstanceInfo);

            this.SetViewUtilityFunc(view);

            gameObject.SetActive(false);
            view.Awake(gameObject);

            return view;
        }

        private void SetViewUtilityFunc(UIViewBase view)
        {

        }
        #endregion
    }
}