using SDS.Enumerations;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDS.Data.Save
{
    /// <summary>
    /// editor 使用的对话事件持久化数据
    /// </summary>
    [Serializable]
    public class SDSEventSaveData
    {
        [field: SerializeField] public SDSDialogueEventType EventType { get; set; }
        [field: SerializeField] public UnityEngine.Object AssetObject { get; set; }
        [field: SerializeField] public List<string> Parameters { get; set; }
        [field: SerializeField] public string Description { get; set; }

        public string GetParameterByIndex(int index)
        {
            if (this.Parameters == null || this.Parameters.Count - 1 < index)
                return null;
            return this.Parameters[index];
        }

        public object GetParsedParameterByIndex<T>(int index)
        {
            string parameter = this.GetParameterByIndex(index);
            if (parameter == null)
                return default(T);
            Type type = typeof(T);
            if (type == typeof(float))
            {
                if (float.TryParse(parameter, out float result))
                {
                    return result;
                }
            }
            else if (type == typeof(int))
            {
                if (int.TryParse(parameter, out int result))
                {
                    return result;
                }
            }
            else if (type == typeof(bool))
            {
                if (bool.TryParse(parameter, out bool result))
                {
                    return result;
                }
            }

            return default(T);
        }

        public void SetParameterByIndex(int index, string value)
        {
            if(this.Parameters == null)
                this.Parameters = new List<string>();
            if (this.Parameters.Count - 1 < index)
                for (int i = this.Parameters.Count - 1; i < index; ++i)
                    this.Parameters.Add(null);
            this.Parameters[index] = value;
        }

    }

    ///<see cref="SDSDialogueEventType.ShowImage"/> 显示图片 事件参数：
    ///0: <see cref="SDSDialogueSpritePresetPosition"/> 预设枚举
    ///1: 非预设图片位置时启用，图片的X坐标
    ///2: 图片的Y坐标
    
    ///<see cref="SDSDialogueEventType.PlayBGM"/> 播放BGM 事件参数：
    ///0: 音量，float类型，0到1闭区间
}