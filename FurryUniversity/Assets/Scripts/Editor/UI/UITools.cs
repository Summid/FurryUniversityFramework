using SFramework.Core.UI.External;
using SFramework.Utilities;
using SFramework.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SFramework.Core.UI.Editor
{
    public class UITools
    {
        [MenuItem("Tools/UI/一键替换TMP UGUI Ex")]
        public static void ReplaceTMP2TMPEX()
        {
            if (!EditorUtility.DisplayDialog("确定？", "不建议使用一键替换功能，可能会导致未知的修改", "冲了！", "溜了"))
                return;
            
            string uiViewPath = StaticVariables.UIViewPrefabsPath;
            string uiItemPath = StaticVariables.UIItemPrefabsPath;

            CheckPrefabsInDirectory(uiViewPath);
            CheckPrefabsInDirectory(uiItemPath);
            
            AssetDatabase.SaveAssets();
        }

        public static void CheckPrefabsInDirectory(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach (var fileSystemInfo in directoryInfo.GetFileSystemInfos())
            {
                if ((fileSystemInfo.Attributes & FileAttributes.Directory) != 0)//是文件夹
                {
                    CheckPrefabsInDirectory(fileSystemInfo.FullName);
                }
                else
                {
                    if (Path.GetExtension(fileSystemInfo.Name) != StaticVariables.PrefabExtension)
                        continue;
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(fileSystemInfo.FullName.GetRelativePath());
                    Debug.LogWarning($"Check prefab => {go}");
                    ReplaceTMPEX(go);
                }
            }
        }

        public static void ReplaceTMPEX(GameObject go)
        {
            if (go == null)
                return;
            GameObject prefabInstance = PrefabUtility.InstantiatePrefab(go) as GameObject;
            TextMeshProUGUI[] tmps = prefabInstance.GetComponentsInChildren<TextMeshProUGUI>(true);
            int index = 1;
            foreach (TextMeshProUGUI tmp in tmps)
            {
                Debug.Log($"Handle:{prefabInstance.name}==>{tmp.gameObject.name}");
                if (tmp is TextMeshProUGUIEx)
                    continue;

                EditorUtility.DisplayProgressBar("Working......", $"Handle:{prefabInstance.name}==>{tmp.gameObject.name}", (float)index++ / tmps.Length);

                GameObject tempGO = tmp.gameObject;//用于后面添加新组件
                TextMeshProUGUI tempTMP = GameObject.Instantiate(tmp);//拷贝一下 tmp 组件
                
                GameObject.DestroyImmediate(tmp, true);//删除 tmp 组件

                TextMeshProUGUIEx tmpEX = tempGO.AddComponent<TextMeshProUGUIEx>();
                tmpEX.text = tempTMP.text;
                tmpEX.font = tempTMP.font;
                tmpEX.material = tempTMP.material;
                tmpEX.fontMaterial = tempTMP.fontMaterial;
                tmpEX.fontStyle = tempTMP.fontStyle;
                tmpEX.fontSize = tempTMP.fontSize;
                tmpEX.enableAutoSizing = tempTMP.enableAutoSizing;
                tmpEX.color = tempTMP.color;
                tmpEX.colorGradient = tempTMP.colorGradient;
                tmpEX.overrideColorTags = tempTMP.overrideColorTags;

                tmpEX.characterSpacing = tempTMP.characterSpacing;
                tmpEX.wordSpacing = tempTMP.wordSpacing;
                tmpEX.lineSpacing = tempTMP.lineSpacing;
                tmpEX.paragraphSpacing = tempTMP.paragraphSpacing;

                tmpEX.alignment = tempTMP.alignment;

                tmpEX.enableWordWrapping = tempTMP.enableWordWrapping;
                tmpEX.overflowMode = tempTMP.overflowMode;

                tmpEX.horizontalMapping = tempTMP.horizontalMapping;
                tmpEX.verticalMapping = tempTMP.verticalMapping;

                tmpEX.margin = tempTMP.margin;
                tmpEX.geometrySortingOrder = tempTMP.geometrySortingOrder;
                tmpEX.richText = tempTMP.richText;
                tmpEX.raycastTarget = tempTMP.raycastTarget;
                tmpEX.maskable = tempTMP.maskable;
                tmpEX.parseCtrlCharacters = tempTMP.parseCtrlCharacters;
                tmpEX.spriteAsset = tempTMP.spriteAsset;
                tmpEX.styleSheet = tempTMP.styleSheet;
                tmpEX.enableKerning = tempTMP.enableKerning;
                tmpEX.extraPadding = tempTMP.extraPadding;

                if (tempGO.TryGetComponent(typeof(Button), out var component))
                {
                    var button = component as Button;
                    button.targetGraphic = tmpEX;
                }

                GameObject.DestroyImmediate(tempTMP.gameObject);
            }
            PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.UserAction);//prefab apply all
            GameObject.DestroyImmediate(prefabInstance);
            
            EditorUtility.ClearProgressBar();
        }
    }
}