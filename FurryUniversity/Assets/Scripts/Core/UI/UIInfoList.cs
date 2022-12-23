using System;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class UIInfoList : ScriptableObject
    {
        public List<UIViewInfo> UIList = new List<UIViewInfo>();
        public List<UIItemInfo> UIItemInfo = new List<UIItemInfo>();

        [NonSerialized]
        private Dictionary<string, UIViewInfo> uiListWithTypeNameKey;
        public UIViewInfo GetUIViewInfoByTypeName(string typeName)
        {
            if (this.uiListWithTypeNameKey == null)
            {
                this.uiListWithTypeNameKey = new Dictionary<string, UIViewInfo>();
                foreach (var viewInfo in this.UIList)
                {
                    if (string.IsNullOrEmpty(viewInfo.ViewClassName))
                        continue;
                    this.uiListWithTypeNameKey.Add(viewInfo.ViewClassName, viewInfo);
                }
            }

            this.uiListWithTypeNameKey.TryGetValue(typeName, out UIViewInfo info);
            return info;
        }

        [NonSerialized]
        private Dictionary<string, UIItemInfo> uiItemWithTypeNameKey;
        public UIItemInfo GetUIItemInfoByTypeName(string typeName)
        {
            if (this.uiItemWithTypeNameKey == null)
            {
                this.uiItemWithTypeNameKey = new Dictionary<string, UIItemInfo>();
                foreach (var itemInfo in this.UIItemInfo)
                {
                    if (string.IsNullOrEmpty(itemInfo.UIItemClassName))
                        continue;
                    if (this.uiItemWithTypeNameKey.ContainsKey(itemInfo.UIItemClassName))//可能有重复
                        continue;
                    this.uiItemWithTypeNameKey.Add(itemInfo.UIItemClassName, itemInfo);
                }
            }

            this.uiItemWithTypeNameKey.TryGetValue(typeName, out UIItemInfo info);
            return info;
        }

        [NonSerialized]
        private Dictionary<string, UIItemInfo> uiItemWithAssetNameKey;
        public UIItemInfo GetUIItemInfoByAssetNameKey(string assetName)
        {
            if (this.uiItemWithAssetNameKey == null)
            {
                this.uiItemWithAssetNameKey = new Dictionary<string, UIItemInfo>();
                foreach (var itemInfo in this.UIItemInfo)
                {
                    if (string.IsNullOrEmpty(itemInfo.UIItemAssetName))
                        continue;
                    this.uiItemWithAssetNameKey.Add(itemInfo.UIItemAssetName, itemInfo);
                }
            }

            this.uiItemWithAssetNameKey.TryGetValue(assetName, out UIItemInfo info);
            return info;
        }
    }

    public enum EnumUIType
    {
        /// <summary>Page类型的UI互斥，同一时间只会存在一个Page</summary>
        Page,
        /// <summary>浮动与Page之上，可以同时存在多个</summary>
        Window
    }

    [Serializable]
    public class UIViewInfo
    {
        /// <summary> UI的Prefab资源Bundle名称 </summary>
        public string ViewAssetBundleName;
        /// <summary> UI的Prefab资源名称 </summary>
        public string ViewAssetName;
        /// <summary> UI的类型 </summary>
        public EnumUIType ViewType;
        /// <summary> UI对应的类名 </summary>
        public string ViewClassName;
        /// <summary> 是否是不可导航Page </summary>
        public bool IsDisableNavigatePage;
    }

    [Serializable]
    public class UIItemInfo
    {
        public string UIItemAssetBundleName;
        public string UIItemAssetName;
        public string UIItemClassName;
    }
}