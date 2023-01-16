using System;
using UnityEngine;

namespace SDS.Data
{
    /// <summary>
    /// runtime 使用的 对话持久化数据
    /// </summary>
    [Serializable]
    public class SDSDialogueContentData
    {
        [field: SerializeField][field: TextArea()] public string Text { set; get; }
        [field: SerializeField] public string Spokesman { get; set; }//发言人名字
    }
}