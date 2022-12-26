using SFramework.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
            GenerateCode(list.UIList);
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
                if (!list.UIList.Exists(ui => ui.ViewAssetName == assetName))
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
            foreach (string prefab in itemPrefabPaths)
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

        private static void GenerateCode(List<UIViewInfo> uiInfoList)
        {
            DirectoryInfo directory = Directory.CreateDirectory(StaticVariables.UIViewGenerateCodePath);
            List<string> changedCodeFiles = new List<string>();

            //UI代码生成
            foreach (var info in uiInfoList)
            {
                if (string.IsNullOrEmpty(info.ViewClassName))
                    continue;

                string[] temp = info.ViewClassName.Split('.');
                string className = temp[temp.Length - 1];
                if (!string.IsNullOrEmpty(info.ViewAssetName) && !string.IsNullOrEmpty(info.ViewClassName))
                {
                    GameObject viewPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StaticVariables.UIViewPrefabsPath + $"/{info.ViewAssetName}" + StaticVariables.PrefabExtension);

                    ReferenceCollector rc = viewPrefab.GetComponent<ReferenceCollector>();
                    if (rc != null)
                    {
                        StringBuilder uiFieldBuilder = new StringBuilder();

                        foreach (var rcData in rc.data)
                        {
                            if ((rcData.IsList && rcData.gameObjectList.Count == 0) && rcData.gameObject == null)
                            {
                                Debug.LogError($"[{info.ViewAssetName}].{rcData.key}所指向的GameObject为空，跳过");
                                continue;
                            }

                            string name = rcData.key;
                            GameObject go = (rcData.IsList ? rcData.gameObjectList[0] : rcData.gameObject) as GameObject;
                            if (go == null)
                            {
                                Debug.LogError($"[{className}].{rcData.key} 指定的GameObject为空，跳过");
                                continue;
                            }

                            List<Behaviour> scripts = go.GetComponents<Behaviour>().Where(s => !(s is IIgnoreUIGenCode)).ToList();
                            if (scripts.Count == 0)
                            {
                                uiFieldBuilder.AppendLine(CreateUIField(typeof(GameObject), name, name, rcData.IsList));
                            }
                            else if (scripts.Count == 1)
                            {
                                Type fieldType = null;
                                if (scripts[0] is UIItemSelector selector)
                                {
                                    if (string.IsNullOrEmpty(selector.SelectClass))
                                    {
                                        continue;
                                    }
                                    var assembly = Assembly.GetAssembly(typeof(UIItemBase));
                                    fieldType = assembly.GetType(selector.SelectClass);
                                }
                                else
                                {
                                    fieldType = scripts[0].GetType();
                                }

                                if (fieldType != null)
                                {
                                    uiFieldBuilder.AppendLine(CreateUIField(fieldType, name, name, rcData.IsList));
                                }
                                else
                                {
                                    Debug.LogError($"[{info.ViewAssetName}].[{rcData.key}]有错误");
                                }
                            }
                            else
                            {
                                foreach (var script in scripts)
                                {
                                    Type fieldType = null;
                                    if (script is UIItemSelector selector)
                                    {
                                        if (string.IsNullOrEmpty(selector.SelectClass))
                                        {
                                            continue;
                                        }
                                        var assembly = Assembly.GetAssembly(typeof(UIItemBase));
                                        fieldType = assembly.GetType(selector.SelectClass);
                                    }
                                    else
                                    {
                                        fieldType = script.GetType();
                                    }

                                    string typeSuffix = fieldType.Name;
                                    uiFieldBuilder.AppendLine(CreateUIField(fieldType, $"{name}_{typeSuffix}", name, rcData.IsList));
                                }
                            }
                        }

                        string codeText = ViewCodeTemplate.Replace("{0}", className).Replace("{1}", uiFieldBuilder.ToString());
                        string filePath = $"{directory.FullName}/{className}.g.cs";
                        using (var fileStream = File.Open(filePath, FileMode.Create))
                        {
                            changedCodeFiles.Add(fileStream.Name);
                            var bytes = Encoding.UTF8.GetBytes(codeText);
                            fileStream.Write(bytes, 0, bytes.Length);
                        };
                    }
                }
            }

            //多余的清理掉
            foreach (var file in directory.GetFiles("cs"))
            {
                if (!changedCodeFiles.Contains(file.FullName))
                {
                    File.Delete(file.FullName);
                }
            }
        }

        private static string CreateUIField(Type type, string fieldName, string keyName, bool isList)
        {
            string result = $"        [UIFieldInit(\"{keyName}\")]\r\n";
            if (isList)
                return result + $"        public List<{type.FullName}> {fieldName};";
            else
                return result + $"        public {type.FullName} {fieldName};";
        }

        private const string ViewCodeTemplate =
@"//自动生成的代码,根据UIView对应的Prefab脚本ReferenceCollector指定的节点清单自动生成
using System.Collections.Generic;

namespace SFramework.Core.UI
{
    public partial class {0}
    {
{1}
    }
}
    ";
    }
}