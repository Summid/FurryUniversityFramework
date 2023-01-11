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

        //TODO 事件延迟触发参数来一个秋梨膏

        public string GetParameterByIndex(int index)
        {
            if (this.Parameters == null || this.Parameters.Count - 1 < index)
                return null;
            return this.Parameters[index];
        }

        /// <summary>
        /// 若没有找到事件参数，则返回传入数据类型的默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 给定索引上是否有事件参数
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool HasParameterByIndex(int index)
        {
            return this.GetParameterByIndex(index) != null;
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
    ///0: <see cref="SDSSpritePresetPosition"/> 预设枚举
    ///1: 非预设图片位置时启用，图片的X坐标
    ///2: 图片的Y坐标

    ///<see cref="SDSDialogueEventType.PlayBGM"/> 播放BGM 事件参数：
    ///0: 音量，float类型，0到1闭区间

    ///<see cref="SDSDialogueEventType.PlayBGM"/> 播放SFX 事件参数：
    ///0: 音量，float类型，0到1闭区间
}