using TMPro;
using UnityEditor;
using UnityEngine;

namespace SFramework.Core.UI.External.Editor
{
    [CustomEditor(typeof(TextMeshProOutline), true)]
    [CanEditMultipleObjects]
    public class TextMeshProOutlineEditor : UnityEditor.Editor
    {
        private SerializedProperty tmp;
        private SerializedProperty outlineWidth;
        private SerializedProperty outlineColor;

        private float outlineWidthValue;
        private Color outlineColorValue;

        private TextMeshProUGUI tmpUGUI;

        private void OnEnable()
        {
            this.tmp = this.serializedObject.FindProperty("tmp");
            if ((this.target as TextMeshProOutline).TryGetComponent(out this.tmpUGUI))
            {
                if (this.tmp.objectReferenceValue == null)
                {
                    this.tmp.objectReferenceValue = this.tmpUGUI;
                }
            }
            else
            {
                Debug.LogError($"GameObject {this.target.name} doesn't find TMP script. (TextMeshProOutline)");
                return;
            }

            this.outlineWidth = this.serializedObject.FindProperty("outlineWidth");
            this.outlineColor = this.serializedObject.FindProperty("outlineColor");

            this.outlineWidthValue = this.tmpUGUI.outlineWidth;
            this.outlineColorValue = this.tmpUGUI.outlineColor;

        }

        public override void OnInspectorGUI()
        {
            if (this.tmpUGUI == null)
            {
                return;
            }
            
            this.outlineWidthValue = EditorGUILayout.Slider("Outline Width", this.outlineWidthValue, 0f, 1f);
            this.outlineColorValue = EditorGUILayout.ColorField("Outline Color", this.outlineColorValue);

            //保存数值
            this.outlineWidth.floatValue = this.outlineWidthValue;
            this.outlineColor.colorValue = this.outlineColorValue;

            //预览
            this.tmpUGUI.outlineWidth = this.outlineWidthValue;
            this.tmpUGUI.outlineColor = this.outlineColorValue;

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}