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
    }
}