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

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public bool CheckCurrentDialogueHasNextNode()
        {
            foreach (Data.SDSDialogueChoiceData choice in this.dialogue.Choices)
            {
                if (choice.NextDialogue != null)
                    return true;
            }

            return false;
        }

        public bool SwitchToNextDialogue()
        {
            if (this.dialogue == null)
                return false;
            if (this.dialogue.DialogueType != Enumerations.SDSDialogueType.SingleChoice)
            {
                Debug.LogWarning($"{this.dialogue.DialogueName} 不是单选对话，不能直接切换到下一句");
                return false;
            }

            List<Data.SDSDialogueChoiceData> choices = this.dialogue.Choices;
            if (choices == null || choices.Count == 0 || choices[0].NextDialogue == null)
            {
                Debug.LogWarning($"{this.dialogue.DialogueName} 没有后续对话节点");
                return false;
            }

            this.dialogue = choices[0].NextDialogue;
            return true;
        }

        public bool ChooseDialogueBranch(int branchIndex)
        {
            if (this.dialogue == null)
                return false;
            if (this.dialogue.DialogueType != Enumerations.SDSDialogueType.MultipleChoice)
            {
                Debug.LogWarning($"{this.dialogue.DialogueName} 不是多选对话，不能直接切换到下一句");
                return false;
            }

            List<Data.SDSDialogueChoiceData> choices = this.dialogue.Choices;
            if (choices == null || choices.Count == 0 || choices[0].NextDialogue == null)
            {
                Debug.LogWarning($"{this.dialogue.DialogueName} 没有后续对话节点");
                return false;
            }

            if (branchIndex >= choices.Count)
            {
                Debug.LogWarning($"多选对话选项错误，chooes branch index {branchIndex}, choices count {choices.Count}");
                return false;
            }

            this.dialogue = choices[branchIndex].NextDialogue;
            return true;
        }
    }
}