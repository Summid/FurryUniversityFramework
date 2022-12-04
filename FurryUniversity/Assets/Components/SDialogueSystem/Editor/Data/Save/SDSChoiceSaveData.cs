using System;
using UnityEngine;

namespace SDS.Data.Save
{
    /// <summary>
    /// editor使用的choice持久化数据
    /// </summary>
    [Serializable]
    public class SDSChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }

        /// <summary> 下一对话的NoidID </summary>
        [field: SerializeField] public string NodeID { get; set; }
    }
}