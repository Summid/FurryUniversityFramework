using SDS.Utility;
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

        #region Editor

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

        #endregion

        #region Runtime

        /// <summary>
        /// 获取第一个对话节点，即无前置节点的节点；优先返回组内节点
        /// </summary>
        /// <param name="firstDialogue"></param>
        /// <returns></returns>
        public bool TryGetFirstDialogue(out SDSDialogueSO firstDialogue)
        {
            foreach (var dialogues in this.DialogueGroups.Values)
            {
                foreach (var dialogue in dialogues)
                {
                    if (dialogue.IsStartDialogue)
                    {
                        firstDialogue = dialogue;
                        return true;
                    }
                }
            }

            foreach (var ungroupedDialogue in this.UnGroupedDialogues)
            {
                if (ungroupedDialogue.IsStartDialogue)
                {
                    firstDialogue = ungroupedDialogue;
                    return true;
                }
            }

            firstDialogue = null;
            return false;
        }

        #endregion
    }
}