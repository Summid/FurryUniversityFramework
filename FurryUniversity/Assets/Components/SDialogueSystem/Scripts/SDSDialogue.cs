using SDS.Data;
using SDS.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDS
{
    public class SDSDialogue : MonoBehaviour
    {
        //Dialogue Scriptable Objects
        [SerializeField] private SDSDialogueContainerSO dialogueContainer; //选定某个对话文件，可用于单章节调试
        [SerializeField] private SDSDialogueGroupSO dialogueGroup;
        [SerializeField] private SDSDialogueSO dialogue;

        //Filters
        [SerializeField] private bool groupedDialogues;
        [SerializeField] private bool startingDialoguesOnly;

        //Indexes
        [SerializeField] private int selectedDialogueGroupIndex;
        [SerializeField] private int selectedDialogueIndex;

        [SerializeField] private List<SDSDialogueContainerSO> includedContainers;//对话文件的引用，保存所有章节的对话文件

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public List<SDSDialogueContainerSO> GetAllDialogues()
        {
            return this.includedContainers;
        }


    }
}