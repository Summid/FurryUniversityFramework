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

        protected override void OnEnable()
        {
            base.OnEnable();

            this.cacheSize = this.serializedObject.FindProperty("cacheSize");
            this.scrollRect = this.serializedObject.FindProperty("scrollRect");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.cacheSize, true);
            EditorGUILayout.PropertyField(this.scrollRect, true);

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}