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

        //TODO 进入对话与离开对话触发事件标识：离开对话所触发的事件用空对话节点来实现，该节点触发所有事件后自动跳转到下一对话节点

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

    /// <summary>
    /// 对话事件的事件参数所占用的索引的约定
    /// </summary>
    public class SDSDialogueEventParameterEnum
    {
        /// <summary> <see cref="SDSDialogueEventType.ImageOperations"/> 事件的事件参数约定 </summary>
        public enum ImageOperations
        {
            ///<see cref="SDSDialogueEventType.ImageOperations"/> 图片事件 参数：
            ///0: <see cref="SDSDialogueImageEventOperations"/> 子事件类型枚举索引
            ///1~5 <see cref="SDSDialogueImageEventOperations.Show"/> 占用，1：<see cref="SDSSpritePresetPosition"/> 枚举字符串；2：自定义X坐标；3：自定义Y坐标
            ///6~10 <see cref="SDSDialogueImageEventOperations.Hide"/> 占用，6：隐藏背景时所花时间
            ///11~15 <see cref="SDSDialogueImageEventOperations.Move"/> 占用，11：<see cref="SDSSpritePresetPosition"/> 枚举字符串；12：目标X坐标；13：目标Y坐标；14：移动时间
            OperationType = 0,

            ShowImagePresetPositionType = 1,
            ShowImageXPosition = 2,
            ShowImageYPosition = 3,

            HideImageTransitionTime = 6,

            MoveImagePresetPositionType = 11,
            MoveImageXPosition = 12,
            MoveImageYPosition = 13,
            MoveImageConsumeTime = 14,
        }

        /// <summary>
        /// <see cref="SDSDialogueEventType.BackgroundImageOperations"/> 事件的事件参数约定
        /// </summary>
        public enum BGImageOperations 
        {
            /// <summary> <see cref="SDSDialogueBackgroundImageEventOperations"/> 索引 </summary>
            OperationType = 0,

            /// <summary> 透明度从0到1所花时间；Hide同理 </summary>
            ShowBGImageTransitionTime = 1,

            HideBGImageTransitionTime = 6,
        }

        /// <summary>
        /// <see cref="SDSDialogueEventType.BGMOperations"/>
        /// </summary>
        public enum BGMOperation
        {
            /// <summary> <see cref="SDSDialogueBGMEventOperations"/> 索引 </summary>
            OperationType = 0,

            Volume = 1,//播放音量，该参数共用
            TransitionTime = 2,//播放或停止播放时的音量渐变时间，该参数共用
        }

        /// <summary>
        /// <see cref="SDSDialogueEventType.SFXOperations"/>
        /// </summary>
        public enum SFXOperation
        {
            /// <summary> <see cref="SDSDialogueSFXEventOperations"/> 索引 </summary>
            OperationType = 0,

            Volume = 1,
            LoopTimes = 2,
        }
    }
}