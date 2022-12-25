using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    /// <summary>
    /// 标记在UIView目录中对应Prefab资源的名称，名称不区分大小写且不需要后缀名
    /// </summary>
    public class UIViewAttribute : Attribute
    {
        public string UIViewAssetName { get; private set; }

        public EnumUIType UIType { get; private set; }

        public bool IsDisnavigatePage { get; private set; }

        public UIViewAttribute(string uiViewAssetName, EnumUIType uiType, bool isDisnavigatePage = false)
        {
            this.UIViewAssetName = uiViewAssetName;
            this.UIType = uiType;
            this.IsDisnavigatePage = isDisnavigatePage;
        }
    }
}