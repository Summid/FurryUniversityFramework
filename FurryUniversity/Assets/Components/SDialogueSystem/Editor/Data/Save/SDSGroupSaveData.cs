using System;
using UnityEngine;

namespace SDS.Data.Save
{
    /// <summary>
    /// editor使用的group持久化数据
    /// </summary>
    [Serializable]
    public class SDSGroupSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}