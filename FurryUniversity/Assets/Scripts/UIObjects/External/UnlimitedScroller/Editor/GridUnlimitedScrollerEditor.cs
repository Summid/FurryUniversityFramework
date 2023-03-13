using UnityEditor;
using UnityEditor.UI;

namespace SFramework.Core.UI.External.UnlimitedScroller.Editor
{
    [CustomEditor(typeof(GridUnlimitedScroller),true)]
    [CanEditMultipleObjects]
    public class GridUnlimitedScrollerEditor : GridLayoutGroupEditor
    {
        private SerializedProperty matchContentWidth;
        private SerializedProperty cellPerRow;
        private SerializedProperty horizontalAlignment;
        private SerializedProperty cacheSize;
        private SerializedProperty scrollRect;

        protected override void OnEnable()
        {
            base.OnEnable();

            this.matchContentWidth = this.serializedObject.FindProperty("matchContentWidth");
            this.cellPerRow = this.serializedObject.FindProperty("cellPerRow");
            this.horizontalAlignment = this.serializedObject.FindProperty("horizontalAlignment");
            this.cacheSize = this.serializedObject.FindProperty("cacheSize");
            this.scrollRect = this.serializedObject.FindProperty("scrollRect");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.matchContentWidth, true);
            if (!this.matchContentWidth.boolValue)
            {
                EditorGUILayout.PropertyField(this.cellPerRow, true);
            }

            EditorGUILayout.PropertyField(this.horizontalAlignment, true);
            EditorGUILayout.PropertyField(this.cacheSize, true);
            EditorGUILayout.PropertyField(this.scrollRect, true);

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}