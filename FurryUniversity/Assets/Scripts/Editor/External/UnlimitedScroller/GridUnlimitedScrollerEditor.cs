using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

namespace SFramework.Core.UI.External.UnlimitedScroller.Editor
{
    [CustomEditor(typeof(GridUnlimitedScroller),true)]
    [CanEditMultipleObjects]
    public class GridUnlimitedScrollerEditor : GridLayoutGroupEditor
    {
        private SerializedProperty matchContentWidth;
        private SerializedProperty matchAverageCellsPerRow;
        private SerializedProperty cellPerRow;
        private SerializedProperty horizontalAlignment;
        private SerializedProperty cacheSize;
        private SerializedProperty scrollRect;
        private SerializedProperty cellPrefab;

        private GridLayoutGroup self;

        protected override void OnEnable()
        {
            base.OnEnable();

            this.matchContentWidth = this.serializedObject.FindProperty("matchContentWidth");
            this.matchAverageCellsPerRow = this.serializedObject.FindProperty("matchAverageCellsPerRow");
            this.cellPerRow = this.serializedObject.FindProperty("cellPerRow");
            this.horizontalAlignment = this.serializedObject.FindProperty("horizontalAlignment");
            this.cacheSize = this.serializedObject.FindProperty("cacheSize");
            this.scrollRect = this.serializedObject.FindProperty("scrollRect");
            this.cellPrefab = this.serializedObject.FindProperty("cellPrefab");
            
            this.self = this.target as GridLayoutGroup;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (this.self.constraint == GridLayoutGroup.Constraint.FixedRowCount && !this.matchContentWidth.boolValue)
            {
                EditorGUILayout.PropertyField(this.matchAverageCellsPerRow, true);
            }

            if (!this.matchAverageCellsPerRow.boolValue)
            {
                EditorGUILayout.PropertyField(this.matchContentWidth, true);
            }
            
            if (!this.matchContentWidth.boolValue && !this.matchAverageCellsPerRow.boolValue)
            {
                EditorGUILayout.PropertyField(this.cellPerRow, true);
            }

            EditorGUILayout.PropertyField(this.horizontalAlignment, true);
            EditorGUILayout.PropertyField(this.cacheSize, true);
            EditorGUILayout.PropertyField(this.scrollRect, true);
            EditorGUILayout.PropertyField(this.cellPrefab, true);

            if (this.matchContentWidth.boolValue && this.matchAverageCellsPerRow.boolValue)
            {
                this.matchContentWidth.boolValue = false;
                this.matchAverageCellsPerRow.boolValue = false;
            }
            
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}