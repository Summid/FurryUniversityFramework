using SFramework.Utilities;
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace SFramework.Core.UI.External.Editor
{
    [CustomEditor(typeof(TextMeshProUGUIEx), true)]
    [CanEditMultipleObjects]
    public class TextMeshProUGUIExEditor : TMP_BaseEditorPanel
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
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        protected override void DrawExtraSettings()
        {
            
        }
        protected override bool IsMixSelectionTypes()
        {
            return false;
        }
        
        protected override void OnUndoRedo()
        {
            
        }
    }
}