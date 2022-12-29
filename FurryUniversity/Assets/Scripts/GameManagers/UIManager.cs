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

        private UIInfoList uiList;
        private Dictionary<Type,UIInstanceInfo> uiInstances = new Dictionary<Type,UIInstanceInfo>();//加载后的View保存在这
        private List<UIInstanceInfo> uiInstanceLoadOrderList = new List<UIInstanceInfo>();//根据View显示顺序排序

        protected override async void OnInitialized()
        {
            this.uiList = await AssetBundleManager.LoadAssetInAssetBundleAsync<UIInfoList>(StaticVariables.UIListName, StaticVariables.UIListBundleName);
            Debug.Log($"uiList in UIManager {this.uiList}");
            Debug.Log("UIManager Initialized");
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
        #endregion
    }
}