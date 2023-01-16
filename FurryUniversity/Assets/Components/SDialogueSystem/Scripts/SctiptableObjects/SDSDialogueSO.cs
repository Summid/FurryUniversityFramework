using SDS.Data;
using SDS.Enumerations;
using System.Collections.Generic;
using UnityEngine;

namespace SDS.ScriptableObjects
{
    /// <summary>
    /// runtime下使用的node持久化数据
    /// </summary>
    public class SDSDialogueSO : ScriptableObject
    {
        [field: SerializeField] public string DialogueName { get; set; }
        [field: SerializeField][field: TextArea()] public string Text { get; set; }
        [field: SerializeField] public List<SDSDialogueContentData> Contents { get; set; }
        [field: SerializeField] public List<SDSDialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public SDSDialogueType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartDialogue { get; set; }
        [field: SerializeField] public List<SDSDialogueEventData> Events { get; set; }

        public void Initialize(string dialogueName, List<SDSDialogueContentData> contents, string text, List<SDSDialogueChoiceData> choices, SDSDialogueType dialogueType, bool isStartDialogue, List<SDSDialogueEventData> events)
        {
            this.DialogueName = dialogueName;
            this.Contents = contents;
            this.Text = text;
            this.Choices = choices;
            this.DialogueType = dialogueType;
            this.IsStartDialogue = isStartDialogue;
            this.Events = events;
        }
    }
}