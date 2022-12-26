using SFramework.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SFramework.Core.UI.Editor
{
    public class UIManagerEditor
    {
        [MenuItem("Tools/生成资源清单")]
        public static void UpdateUIList()
        {
            GenerateUIList(out UIInfoList list, out string uiListInfoPath);
        }

        private static void GenerateUIList(out UIInfoList list, out string uiListInfoPath)
        {
            list = ScriptableObject.CreateInstance<UIInfoList>();

            Type[] types = Assembly.GetAssembly(typeof(UIViewBase)).GetTypes();
            string[] viewPrefabPaths = Directory.GetFiles(StaticVariables.UIViewPrefabsPath, "*.prefab");//获取UIView中所有Prefab资源

            //检查被UIViewAttribute标记的类型
            foreach (Type type in types)
            {
                UIViewInfo info = new UIViewInfo()
                {
                    ViewClassName = type.FullName
                };
                var uiViewAtt = type.GetCustomAttribute<UIViewAttribute>();
                if (uiViewAtt != null)
                {
                    info.ViewType = uiViewAtt.UIType;
                    info.IsDisableNavigatePage = uiViewAtt.IsDisnavigatePage;

                    //查找对应Prefab
                    string targetView = viewPrefabPaths.FirstOrDefault(file => Path.GetFileNameWithoutExtension(file).ToLower() == uiViewAtt.UIViewAssetName.ToLower());
                    if (targetView != null)
                    {
                        var importer = AssetImporter.GetAtPath(targetView);
                        if (string.IsNullOrEmpty(importer.assetBundleName))
                        {
                            importer.assetBundleName = Path.GetFileNameWithoutExtension(targetView).ToLower() + StaticVariables.UIViewBundleExtension;
                            importer.SaveAndReimport();
                            Debug.LogWarningFormat("UIView[{0}]对应的 View Prefab[{1}]资源没有添加Bundle标签，已自动添加", type.Name, Path.GetFileName(targetView));
                        }

                        info.ViewAssetName = Path.GetFileNameWithoutExtension(targetView);
                        info.ViewAssetBundleName = importer.assetBundleName.ToLower();
                    }
                    list.UIList.Add(info);
                }
            }

            //检查UIView目录下的Prefab，若没在list中（没有对应UIView脚本），也要记录
            foreach (string prefab in viewPrefabPaths)
            {
                string assetName = Path.GetFileNameWithoutExtension(prefab);
                if(!list.UIList.Exists(ui=>ui.ViewAssetName == assetName))
                {
                    UIViewInfo info = new UIViewInfo()
                    {
                        ViewAssetName = assetName,
                        ViewAssetBundleName = AssetImporter.GetAtPath(prefab).assetBundleName,
                    };
                    list.UIList.Add(info);
                }
            }

            string[] itemPrefabPaths = Directory.GetFiles(StaticVariables.UIItemPrefabsPath, "*.prefab", SearchOption.AllDirectories);
            foreach(string prefab in itemPrefabPaths)
            {
                string assetName = Path.GetFileNameWithoutExtension(prefab);
                if (!list.UIItemInfo.Exists(ui => ui.UIItemAssetName == assetName))
                {
                    var selector = AssetDatabase.LoadAssetAtPath<GameObject>(prefab).GetComponent<UIItemSelector>();

                    if (selector == null)
                    {
                        Debug.LogError($"生成资源清单错误：{prefab}上没有selector脚本");
                        continue;
                    }

                    var importer = AssetImporter.GetAtPath(prefab);
                    if (string.IsNullOrEmpty(importer.assetBundleName))
                    {
                        importer.assetBundleName = Path.GetFileNameWithoutExtension(assetName).ToLower() + StaticVariables.UIItemBundleExtension;
                        importer.SaveAndReimport();
                        Debug.LogWarningFormat("UIItem[{0}]对应的 Item Prefab[{1}]资源没有添加Bundle标签，已自动添加", selector.SelectClass, Path.GetFileName(assetName));
                    }

                    UIItemInfo info = new UIItemInfo()
                    {
                        UIItemAssetName = assetName,
                        UIItemAssetBundleName = importer.assetBundleName,
                        UIItemClassName = selector.SelectClass
                    };

                    list.UIItemInfo.Add(info);
                }
            }

            uiListInfoPath = StaticVariables.UIViewPrefabsPath + "/" + StaticVariables.UIListName;
            AssetDatabase.CreateAsset(list, uiListInfoPath);

            var uiListImporter = AssetImporter.GetAtPath(uiListInfoPath);
            uiListImporter.assetBundleName = StaticVariables.UIListBundleName;
            uiListImporter.SaveAndReimport();
        }
    }
}