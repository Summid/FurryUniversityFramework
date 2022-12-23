using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SFramework.EditorExtensions
{
    public class SearchField : GUIBase
    {
        private string searchContent;
        private string[] searchableContents;
        private int contentIndex;
        private MethodInfo drawAPI;

        public Action<int> OnIndexChanged;
        public Action<string> OnValueChanged;
        public Action<string> OnEndEdit;
        public SearchField(string searchContent, string[] searchableContents, int contentIndex)
        {
            this.searchContent = searchContent;
            this.searchableContents = searchableContents;
            this.contentIndex = contentIndex;

            this.drawAPI = typeof(EditorGUI).GetMethod("ToolbarSearchField",
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new Type[]//获取该参数列表版本的方法
                {
                    typeof(int),
                    typeof(Rect),
                    typeof(string[]),
                    typeof(int).MakeByRefType(),
                    typeof(string)
                },
                null);
        }


        private int controlId;
        public override void OnGUI(Rect position)
        {
            base.OnGUI(position);

            if (this.drawAPI != null)
            {
                this.controlId = GUIUtility.GetControlID("EditorSearchField".GetHashCode(), FocusType.Keyboard, position);

                int index = this.contentIndex;
                object[] args = new object[] { this.controlId, position, this.searchableContents, index, this.searchContent };
                string newSearchContent = this.drawAPI.Invoke(null, args) as string;
                if ((int)args[3] != this.contentIndex)
                {
                    this.contentIndex = (int)args[3];
                    this.OnIndexChanged?.Invoke(this.contentIndex);
                }

                if (newSearchContent != this.searchContent)
                {
                    this.searchContent = newSearchContent;
                    this.OnValueChanged?.Invoke(this.searchContent);
                }

                Event e = Event.current;
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Escape || e.character == '\n')
                {
                    if (GUIUtility.keyboardControl == this.controlId)
                    {
                        GUIUtility.keyboardControl = -1;
                        if (e.type != EventType.Repaint && e.type != EventType.Layout)
                        {
                            e.Use();
                        }
                        this.OnEndEdit?.Invoke(this.searchContent);
                    }
                }
            }
        }

        protected override void OnDispose()
        {

        }
    }
}