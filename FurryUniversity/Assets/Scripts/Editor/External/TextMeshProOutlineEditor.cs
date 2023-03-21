using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace SFramework.Core.UI.External.Editor
{
    [CustomEditor(typeof(TextMeshProOutline),true)]
    [CanEditMultipleObjects]
    public class TextMeshProOutlineEditor : UnityEditor.Editor
    {
        private SerializedProperty tmp;
        private SerializedProperty outlineWidth;
        private SerializedProperty outlineColor;

        private float outlineWidthValue;
        private Color outlineColorValue;

        private TextMeshProUGUI tmpUGUI;
        private Material originMaterial;
        private Material tempMaterial;

        private void OnEnable()
        {
            this.tmp = this.serializedObject.FindProperty("tmp");
            if (this.tmp.objectReferenceValue == null)
            {
                this.tmp.objectReferenceValue = (this.target as TextMeshProOutline).GetComponent<TextMeshProUGUI>();
            }
            
            this.outlineWidth = this.serializedObject.FindProperty("outlineWidth");
            this.outlineColor = this.serializedObject.FindProperty("outlineColor");

            this.outlineWidthValue = this.outlineWidth.floatValue;
            this.outlineColorValue = this.outlineColor.colorValue;

            Shader shader = Shader.Find("TextMeshPro/Distance Field");
            this.tempMaterial = new Material(shader);
            this.tmpUGUI = this.tmp.objectReferenceValue as TextMeshProUGUI;
            if (tmpUGUI != null)
            {
                this.originMaterial = this.tmpUGUI.material;
                this.tmpUGUI.material = this.tempMaterial;
            }
        }

        private void OnDisable()
        {
            throw new NotImplementedException();
        }

        public override void OnInspectorGUI()
        {
            this.outlineWidthValue = EditorGUILayout.Slider("Outline Width", this.outlineWidthValue, 0f, 1f);
            this.outlineColorValue = EditorGUILayout.ColorField("Outline Color", this.outlineColorValue);

            if (this.tmpUGUI == null)
            {
                return;
            }
            this.tmpUGUI.outlineWidth = this.outlineWidthValue;
            this.tmpUGUI.outlineColor = this.outlineColorValue;

            this.outlineWidth.floatValue = this.outlineWidthValue;
            this.outlineColor.colorValue = this.outlineColorValue;

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}