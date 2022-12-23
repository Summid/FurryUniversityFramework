using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SFramework.EditorExtensions
{
    public class SearchablePopup : PopupWindowContent
    {
        public static void Show(Rect position, string[] options, int current, Action<int, string> onValueChange, int width = 400)
        {
            SearchablePopup win = new SearchablePopup(options, current, onValueChange, width);
            PopupWindow.Show(position, win);
        }
        private readonly string[] names;
        private readonly int width;
        private SearchField searchField;
        private SelectTree tree;

        private SearchablePopup(string[] names, int currentIndex, Action<int, string> onSelectionMade, int width = 400)
        {
            this.names = names;
            this.width = width;
            this.searchField = new SearchField("", null, 0);
            this.tree = new SelectTree(new TreeViewState(), this, currentIndex, names, onSelectionMade);
            this.searchField.OnValueChanged += (str) => { this.tree.searchString = str; };
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(this.width, Mathf.Min(600, (this.names.Length + 1) * EditorStyles.toolbar.fixedHeight + 10));
        }

        public override void OnGUI(Rect rect)
        {
            var rects = rect.HorizontalSplit(EditorStyles.toolbar.fixedHeight + 5);
            this.DrawSearch(rects[0].Zoom(-5, AnchorType.LowerCenter));
            this.tree.OnGUI(rects[1].Zoom(-5, AnchorType.UpperCenter));
        }

        private void DrawSearch(Rect rect)
        {
            this.searchField.OnGUI(rect.Zoom(-2, AnchorType.MiddleCenter));
        }

        private class SelectTree : TreeView
        {
            private static readonly GUIStyle Selection = "SelectionRect";

            private readonly SearchablePopup pop;
            private readonly int current;
            private readonly string[] names;
            private readonly Action<int, string> OnSelectioniMade;

            private struct Index
            {
                public int Id;
                public string Value;
            }

            private List<Index> show;

            public SelectTree(TreeViewState state, SearchablePopup pop, int current, string[] names, Action<int, string> onSelectionMade) : base(state)
            {
                this.pop = pop;
                this.current = current;
                this.names = names;
                this.show = new List<Index>();
                for (int i = 0; i < names.Length; ++i)
                {
                    this.show.Add(new Index()
                    {
                        Id = names.ToList().IndexOf(names[i]),
                        Value = names[i]
                    });
                }
                this.OnSelectioniMade = onSelectionMade;
                this.showAlternatingRowBackgrounds = true;
                this.Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
                return root;
            }

            protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
            {
                var list = new List<TreeViewItem>();
                for (int i = 0; i < this.show.Count; ++i)
                {
                    list.Add(new TreeViewItem() { id = this.show[i].Id, depth = 1, displayName = this.show[i].Value });
                }
                return list;
            }

            protected override void SingleClickedItem(int id)
            {
                base.SingleClickedItem(id);
                this.OnSelectioniMade?.Invoke(id, this.names[id]);
                this.pop.editorWindow.Close();
                GUIUtility.ExitGUI();
            }

            private void DrawBox(Rect rect, Color tint)
            {
                Color c = GUI.color;
                GUI.color = tint;
                GUI.Box(rect, "", Selection);
                GUI.color = c;
            }

            protected override void SearchChanged(string newSearch)
            {
                this.show.Clear();
                for (int i = 0; i < this.names.Length; ++i)
                {
                    if (this.names[i].ToLower().Contains(searchString.ToLower()))
                    {
                        this.show.Add(new Index()
                        {
                            Id = this.names.ToList().IndexOf(this.names[i]),
                            Value = this.names[i]
                        });
                    }
                }
                this.Reload();
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                base.RowGUI(args);
                if (args.item.id == this.current)
                {
                    this.DrawBox(args.rowRect, Color.white);
                }
            }

            protected override bool CanMultiSelect(TreeViewItem item)
            {
                return false;
            }

            protected override bool CanBeParent(TreeViewItem item)
            {
                return false;
            }
        }
    }
}