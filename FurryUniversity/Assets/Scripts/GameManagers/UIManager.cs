using SFramework.Core.UI;
using SFramework.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.GameManagers
{
    public class UIManager : GameManagerBase
    {
        private UIInfoList uiList;

        protected override async void OnInitialized()
        {
            this.uiList = await AssetBundleManager.LoadAssetInAssetBundleAsync<UIInfoList>(StaticVariables.UIListName, StaticVariables.UIListBundleName);
            Debug.Log($"uiList in UIManager {this.uiList}");
            Debug.Log("UIManager Initialized");
        }

        #region 外部接口
        public UIItemInfo GetUIItemInfo(string itemAsset)
        {
            return this.uiList.GetUIItemInfoByAssetNameKey(itemAsset);
        }
        #endregion
    }
}