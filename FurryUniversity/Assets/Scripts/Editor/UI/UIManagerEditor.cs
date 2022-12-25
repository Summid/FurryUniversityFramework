using SFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
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


            uiListInfoPath = StaticVariables.UIViewPrefabsPath + "/" + StaticVariables.UIListName;
            AssetDatabase.CreateAsset(list, uiListInfoPath);

            var uiListImporter = AssetImporter.GetAtPath(uiListInfoPath);
            uiListImporter.assetBundleName = StaticVariables.UIListBundleName;
            uiListImporter.SaveAndReimport();
        }
    }
}