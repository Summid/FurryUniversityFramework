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
        [field: SerializeField] public List<SDSDialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public SDSDialogueType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartDialogue { get; set; }

        public void Initialize(string dialogueName, string text, List<SDSDialogueChoiceData> choices, SDSDialogueType dialogueType, bool isStartDialogue)
        {
            this.DialogueName = dialogueName;
            this.Text = text;
            this.Choices = choices;
            this.DialogueType = dialogueType;
            this.IsStartDialogue = isStartDialogue;
        }
    }
}