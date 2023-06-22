using SDS.Data;
using SDS.Enumerations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SDS.ScriptableObjects
{
    /// <summary>
    /// runtime下使用的node持久化数据
    /// </summary>
    public class SDSDialogueSO : ScriptableObject
    {
        [field: SerializeField] public string DialogueName { get; set; }
        [field: SerializeField] public List<SDSDialogueContentData> Contents { get; set; }
        [field: SerializeField] public List<SDSDialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public SDSDialogueType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartDialogue { get; set; }
        [field: SerializeField] public List<SDSDialogueEventData> Events { get; set; }

        public void Initialize(string dialogueName, List<SDSDialogueContentData> contents, List<SDSDialogueChoiceData> choices, SDSDialogueType dialogueType, bool isStartDialogue, List<SDSDialogueEventData> events)
        {
            this.DialogueName = dialogueName;
            this.Contents = contents;
            this.Choices = choices;
            this.DialogueType = dialogueType;
            this.IsStartDialogue = isStartDialogue;
            this.Events = events;
        }

        #region Runtime

        /// <summary>
        /// 获取该节点第一句话
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool TryGetFirstDialogueContent(out SDSDialogueContentData content)
        {
            content = null;
            if (this.Contents.Count <= 0)
                return false;

            content = this.Contents[0];
            return true;
        }

        /// <summary>
        /// 获取给定索引的对话
        /// </summary>
        /// <param name="content"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool TryGetDialogueContentByIndex(out SDSDialogueContentData content, int index)
        {
            content = null;
            if (this.Contents.Count <= index)
                return false;

            content = this.Contents[index];
            return true;
        }

        /// <summary>
        /// 获取该节点中，给定句子的下一句；若没有下一句则返回false
        /// </summary>
        /// <param name="currentContentData"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool TryGetNextDialogueContent(SDSDialogueContentData currentContentData, out SDSDialogueContentData next)
        {
            next = currentContentData;
            if (currentContentData == null)
                return false;

            int currentIndex = this.Contents.IndexOf(currentContentData);
            if (currentIndex >= this.Contents.Count - 1)
                return false;

            next = this.Contents[currentIndex + 1];
            return true;
        }
        
        /// <summary>
        /// 给定当前句子索引，获取下一句句子数据
        /// </summary>
        /// <param name="currentContentIndex"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool TryGetNextDialogueContent(int currentContentIndex, out SDSDialogueContentData next)
        {
            next = null;
            if (currentContentIndex + 1 >= this.Contents.Count)
                return false;

            next = this.Contents[currentContentIndex + 1];
            return true;
        }

        /// <summary>
        /// 获取开始或结束事件
        /// </summary>
        /// <param name="getStartEvent"></param>
        /// <returns></returns>
        public IEnumerable<SDSDialogueEventData> GetEvents(bool getStartEvent)
        {
            if (this.Events != null)
            {
                return this.Events.Where(e => e.IsEventOnExit == !getStartEvent);
            }
            return null;
        }

        /// <summary>
        /// 获取该节点的选项数据；当没有后续节点时返回false；只会返回有后续节点的选项；单选节点也会返回一个选项
        /// </summary>
        /// <param name="choices"></param>
        /// <returns></returns>
        public bool TryGetChoices(out IEnumerable<SDSDialogueChoiceData> choices)
        {
            choices = null;
            if (this.Choices.Count == 1 && this.Choices[0].NextDialogue == null)
                return false;

            choices = this.Choices.Where(choice => choice.NextDialogue != null);
            return true;
        }

        #endregion
    }
}