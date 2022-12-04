using System.Collections.Generic;
using UnityEngine;

namespace SDS.Data.Save
{
    /// <summary>
    /// 持久化数据，用于记录editor中的数据
    /// </summary>
    public class SDSGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public List<SDSGroupSaveData> Groups { get; set; }
        [field: SerializeField] public List<SDSNodeSaveData> Nodes { get; set; }

        [field: SerializeField] public List<string> OldGroupNames { get; set; }
        [field: SerializeField] public List<string> OldUngroupedNodeNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }

        public void Initialize(string fileName)
        {
            this.FileName = fileName;

            this.Groups = new List<SDSGroupSaveData>();
            this.Nodes = new List<SDSNodeSaveData>();
        }
    }
}