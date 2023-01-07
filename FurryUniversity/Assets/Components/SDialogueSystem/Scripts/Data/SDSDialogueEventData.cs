using SDS.Enumerations;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDS.Data
{
    /// <summary>
    /// runtime 下使用的对话数据数据
    /// </summary>
    [Serializable]
    public class SDSDialogueEventData
    {
        [field: SerializeField] public SDSDialogueEventType EventType { get; set; }
        [field: SerializeField] public string AssetName { get; set; }
        [field: SerializeField] public List<string> Parameters { get; set; }
    }
}