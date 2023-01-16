using System;
using UnityEngine;

namespace SDS.Data.Save
{
    /// <summary>
    /// editor 使用的 对话持久化数据
    /// </summary>
    [Serializable]
    public class SDSDialogueContentSaveData
    {
        [field: SerializeField] public string Text { set; get; }
        [field: SerializeField] public string Spokesman { get; set; }//发言人名字
    }
}