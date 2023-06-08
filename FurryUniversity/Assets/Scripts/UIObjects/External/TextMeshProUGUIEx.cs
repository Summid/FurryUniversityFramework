using SFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace SFramework.Core.UI.External
{
    public class TextMeshProUGUIEx : TextMeshProUGUI
    {
        [MenuItem("GameObject/UI/Ex/TextMeshPro UGUI Ex")]
        public static void CreateTMPEX()//todo 迁移到editor程序集下
        {
            GameObject selection = Selection.activeGameObject;
            if (selection == null)
                return;

            GameObject tmpGO = new GameObject("TMP UGUI Ex");
            tmpGO.transform.SetParent(selection.transform, false);
            var tmp = tmpGO.AddComponent<TextMeshProUGUIEx>();
            
            //设置默认字体
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{StaticVariables.FontPath}/{StaticVariables.DefaultFontName}{StaticVariables.FontExtension}");
            tmp.font = fontAsset;

            Selection.activeGameObject = tmpGO;
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            //在编辑器加载安卓AB，TMP的shader不会正确加载，需要重新加载一次(ˉ▽ˉ；)...
            #if UNITY_EDITOR && UNITY_ANDROID
            this.fontMaterial.shader = Shader.Find(this.fontMaterial.shader.name);
            #endif
        }
    }
}