using SFramework.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.GameManager
{
    public enum DisplayMode { Page, Window };

    [AttributeUsage(AttributeTargets.Class)]
    class UIViewInfoAttribute : Attribute
    {
        public DisplayMode UIDisplayMode;
        public string UIViewName;

        private string assetBundleName;
        public string AssetBundleName
        {
            get
            {
                if(string.IsNullOrEmpty(this.assetBundleName))
                {
                    this.assetBundleName = this.UIViewName + StaticVariables.UIViewBundleExtension;
                }
                return this.assetBundleName;
            }
        }
    }

    public class UIManager : GameManagerBase
    {
        protected override void OnInitialized()
        {
            Debug.Log("UIManager Initialized");
        }
    }
}