using SDS.ScriptableObjects;
using System;
using UnityEngine;

namespace SDS.Data
{
    /// <summary>
    /// runtime下使用的choice持久化数据
    /// </summary>
    [Serializable]
    public class SDSDialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public SDSDialogueSO NextDialogue { get; set; }
    }
}