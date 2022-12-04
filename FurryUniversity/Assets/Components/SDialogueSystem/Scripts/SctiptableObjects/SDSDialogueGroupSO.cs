using UnityEngine;

namespace SDS.ScriptableObjects
{
    /// <summary>
    /// runtime下使用的group持久化数据
    /// </summary>
    public class SDSDialogueGroupSO : ScriptableObject
    {
        [field:SerializeField]public string GroupName { get; set; }

        public void Initialize(string groupName)
        {
            this.GroupName = groupName;
        }
    }
}