using UnityEditor;
using UnityEditor.UI;

namespace SFramework.Core.UI.External.UnlimitedScroller.Editor
{
    [CustomEditor(typeof(HorizontalUnlimitedScroller), true)]
    [CanEditMultipleObjects]
    public class HorizontalUnlimitedScrollerEditor : HorizontalOrVerticalLayoutGroupEditor
    {
        private SerializedProperty cacheSize;
        private SerializedProperty scrollRect;
        private SerializedProperty cellPrefab;

        protected override void OnEnable()
        {
            base.OnEnable();

            this.cacheSize = this.serializedObject.FindProperty("cacheSize");
            this.scrollRect = this.serializedObject.FindProperty("scrollRect");
            this.cellPrefab = this.serializedObject.FindProperty("cellPrefab");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.cacheSize, true);
            EditorGUILayout.PropertyField(this.scrollRect, true);
            EditorGUILayout.PropertyField(this.cellPrefab, true);

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}