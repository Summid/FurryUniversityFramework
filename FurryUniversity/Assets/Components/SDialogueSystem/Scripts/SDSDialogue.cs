using SDS.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDS
{
    public class SDSDialogue : MonoBehaviour
    {
        //Dialogue Scriptable Objects
        [SerializeField] private SDSDialogueContainerSO dialogueContainer;
        [SerializeField] private SDSDialogueGroupSO dialogueGroup;
        [SerializeField] private SDSDialogueSO dialogue;

        //Filters
        [SerializeField] private bool groupedDialogues;
        [SerializeField] private bool startingDialoguesOnly;

        //Indexes
        [SerializeField] private int selectedDialogueGroupIndex;
        [SerializeField] private int selectedDialogueIndex;

        public SDSDialogueContainerSO DialogueContainer { get => this.dialogueContainer; }

        public SDSDialogueSO CurrentDialogue => this.dialogue;

        public bool SwitchToNextDialogue()
        {
            if (this.dialogue == null)
                return false;
            if (this.dialogue.DialogueType != Enumerations.SDSDialogueType.SingleChoice)
            {
                Debug.LogWarning($"{this.dialogue.DialogueName} 不是单选对话，不能直接切换到下一句");
                return false;
            }

            List<Data.SDSDialogueChoiceData> choice = this.dialogue.Choices;
            //TODO 判断choice是否合法并切换到下一句

            return false;
        }
    }
}