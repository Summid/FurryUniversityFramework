using System.Collections.Generic;
using UnityEngine;

namespace SDS.ScriptableObjects
{
    /// <summary>
    /// 持久化数据，用于记录runtime下使用的数据
    /// </summary>
    public class SDSDialogueContainerSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SerializableDictionary<SDSDialogueGroupSO, List<SDSDialogueSO>> DialogueGroups { get; set; }
        [field: SerializeField] public List<SDSDialogueSO> UnGroupedDialogues { get; set; }

        public void Initialize(string fileName)
        {
            this.FileName = fileName;

            this.DialogueGroups = new SerializableDictionary<SDSDialogueGroupSO, List<SDSDialogueSO>>();
            this.UnGroupedDialogues = new List<SDSDialogueSO>();
        }

        public List<string> GetDialogueGroupNames()
        {
            List<string> dialogueGroupNames = new List<string>();
            foreach (SDSDialogueGroupSO dialogurGroup in this.DialogueGroups.Keys)
            {
                dialogueGroupNames.Add(dialogurGroup.GroupName);
            }
            return dialogueGroupNames;
        }

        public List<string> GetGroupedDialogueNames(SDSDialogueGroupSO dialogueGroup, bool startingDialogueOnly)
        {
            List<SDSDialogueSO> groupedDialogues = this.DialogueGroups[dialogueGroup];
            List<string> groupedDialogueNames = new List<string>();

            foreach (SDSDialogueSO groupedDialogue in groupedDialogues)
            {
                if (startingDialogueOnly && !groupedDialogue.IsStartDialogue)
                {
                    continue;
                }
                groupedDialogueNames.Add(groupedDialogue.DialogueName);
            }
            return groupedDialogueNames;
        }

        public List<string> GetUngroupedDialogueNames(bool startingDialogueOnly)
        {
            List<string> ungroupedDialogueNames = new List<string>();

            foreach (SDSDialogueSO ungroupedDialogue in this.UnGroupedDialogues)
            {
                if (startingDialogueOnly && !ungroupedDialogue.IsStartDialogue)
                {
                    continue;
                }
                ungroupedDialogueNames.Add(ungroupedDialogue.DialogueName);
            }
            return ungroupedDialogueNames;
        }
    }
}