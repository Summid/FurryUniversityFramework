using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using SDS.Enumerations;
using SDS.Elements;

namespace SDS.Windows
{
    public class SDSSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private SDSGraphView graphView;
        private Texture2D indentationIcon;//在二级选项前增加一个透明间距

        public void Initialize(SDSGraphView graphView)
        {
            this.graphView = graphView;

            this.indentationIcon = new Texture2D(1, 1);
            this.indentationIcon.SetPixel(0, 0, Color.clear);
            this.indentationIcon.Apply();
        }

        /// <summary>
        /// 创建搜索框
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element"),0),

                new SearchTreeGroupEntry(new GUIContent("Dialogue Node"),1),
                new SearchTreeEntry(new GUIContent("Single Choice",this.indentationIcon))
                {
                    level = 2,
                    userData = SDSDialogueType.SingleChoice
                },
                new SearchTreeEntry(new GUIContent("Multiple Choice",this.indentationIcon))
                {
                    level = 2,
                    userData = SDSDialogueType.MultipleChoice
                },

                new SearchTreeGroupEntry(new GUIContent("Dialogue Group"),1),
                new SearchTreeEntry(new GUIContent("Single Group"))
                {
                    level = 2,
                    userData = new Group()
                }
            };

            return searchTreeEntries;
        }

        /// <summary>
        /// 选择搜索框的选项
        /// </summary>
        /// <param name="SearchTreeEntry"></param>
        /// <param name="context"></param>
        /// <returns>是否关闭搜索框</returns>
        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = this.graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData)
            {
                case SDSDialogueType.SingleChoice:
                    {
                        SDSSingleChoiceNode singleChoiceNode = this.graphView.CreateNode("DialogueName", SDSDialogueType.SingleChoice, localMousePosition) as SDSSingleChoiceNode;

                        this.graphView.AddElement(singleChoiceNode);

                        return true;
                    }

                case SDSDialogueType.MultipleChoice:
                    {
                        SDSMultipleChoiceNode multipleChoiceNode = this.graphView.CreateNode("DialogueName", SDSDialogueType.MultipleChoice, localMousePosition) as SDSMultipleChoiceNode;

                        this.graphView.AddElement(multipleChoiceNode);

                        return true;
                    }

                case Group _:
                    {
                        this.graphView.CreateGroup("DialogueGroup", localMousePosition);

                        return true;
                    }

                default:
                    return false;
            }
        }
    }
}