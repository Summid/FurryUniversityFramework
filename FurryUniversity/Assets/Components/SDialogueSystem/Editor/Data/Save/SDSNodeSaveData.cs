using SDS.Enumerations;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDS.Data.Save
{
    /// <summary>
    /// editor使用的node持久化数据
    /// </summary>
    [Serializable]
    public class SDSNodeSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<SDSDialogueContentSaveData> Contents { get; set; }
        [field: SerializeField] public List<SDSChoiceSaveData> Choices { get; set; }
        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public SDSDialogueType DialogueType { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
        [field: SerializeField] public List<SDSEventSaveData> Events { get; set; }
    }
}