using SDS.Data.Save;
using SDS.Enumerations;
using SDS.Utilities;
using SDS.Utility;
using SDS.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDS.Elements
{
    public partial class SDSNode : Node
    {
        //事件相关成员
        public List<SDSEventSaveData> Events { get; set; }
        private Foldout eventsFoldout;
        public Action OnEventSelected;
        private PopupField<object> popupField;
        private List<EventVO> eventVOs = new List<EventVO>();

        /// <summary> 给SDSEventSaveData穿件衣服 </summary>
        private class EventVO
        {
            public ObjectField objectField;
            public List<VisualElement> parameterElements = new List<VisualElement>();
            public string defaultDescription = string.Empty;
            public SDSEventSaveData eventData;
        }

        public void DrawEventArea(VisualElement container)
        {
            this.eventsFoldout = SDSElementUtility.CreateFoldout("Event Info");
            this.eventsFoldout.AddClasses("sds-node__eventsFoldout-container");
            //this.DrawEvents();
            this.RefreshEventDatas();

            Type eventType = typeof(SDSDialogueEventType);
            List<object> eventValues = Enum.GetValues(eventType).Cast<object>().ToList();

            this.popupField = SDSElementUtility.CreatePopupField<object>(eventValues, SDSDialogueEventType.NullEvent,
                null,
                this.OnSelectedPopupFieldItem);
            this.popupField.AddClasses("sds-node__eventPopupField");

            container.Add(this.eventsFoldout);
            container.Add(this.popupField);
        }

        private void RefreshEventDatas()
        {
            this.eventVOs.Clear();

            this.CorrectErrorEvents();

            foreach (SDSEventSaveData eventData in this.Events)
            {
                EventVO eventVO = new EventVO();
                eventVO.eventData = eventData;
                switch (eventData.EventType)
                {
                    case SDSDialogueEventType.NullEvent:
                        continue;
                    case SDSDialogueEventType.ImageOperations:
                        eventVO.defaultDescription = "Image Operations";
                        eventVO.objectField = SDSElementUtility.CreateObjectField<Sprite>(null, callback =>
                        {
                            eventData.AssetObject = callback.newValue;
                        });

                        List<object> imageOperatioinObjs = Enum.GetValues(typeof(SDSDialogueImageEventOperations)).Cast<object>().ToList();
                        List<string> imageOperationNames = new List<string>();
                        imageOperatioinObjs.ForEach(obj => imageOperationNames.Add(obj.ToString()));
                        int imageOperationSubEventParamIndex = (int)SDSDialogueEventParameterEnum.ImageOperations.OperationType;
                        SDSDialogueImageEventOperations currentImageOperation = (SDSDialogueImageEventOperations)eventData.GetParsedParameterByIndex<int>(imageOperationSubEventParamIndex);
                        var imageOperationsPopupField = SDSElementUtility.CreatePopupField<string>(imageOperationNames, currentImageOperation.ToString(),
                            null, selectedItem =>
                            {
                                string selectedItemName = selectedItem.ToString();
                                if (selectedItemName == eventData.GetParameterByIndex(imageOperationSubEventParamIndex))
                                {
                                    return selectedItemName;
                                }
                                //保存的是子事件枚举的索引
                                eventData.SetParameterByIndex(imageOperationSubEventParamIndex, imageOperationNames.FindIndex(operation => operation == selectedItemName).ToString());
                                this.RefreshImageOperationSubEventElements(eventVO);
                                return selectedItemName;
                            });
                        eventVO.parameterElements.Insert(0, imageOperationsPopupField);
                        break;
                    case SDSDialogueEventType.BackgroundImageOperations:
                        eventVO.defaultDescription = "Background Image Operation";
                        eventVO.objectField = SDSElementUtility.CreateObjectField<Sprite>(null, callback =>
                        {
                            eventData.AssetObject = callback.newValue;
                        });

                        List<object> bgImageOperationObjs = Enum.GetValues(typeof(SDSDialogueBackgroundImageEventOperations)).Cast<object>().ToList();
                        List<string> bgImageOperationNames = new List<string>();
                        bgImageOperationObjs.ForEach(obj => bgImageOperationNames.Add(obj.ToString()));
                        int bgImageOperationSubEventParamIndex = (int)SDSDialogueEventParameterEnum.BGImageOperations.OperationType;
                        SDSDialogueBackgroundImageEventOperations currentBGImageOperation = (SDSDialogueBackgroundImageEventOperations)eventData.GetParsedParameterByIndex<int>(bgImageOperationSubEventParamIndex);
                        var bgImageOperationPopupField = SDSElementUtility.CreatePopupField<string>(bgImageOperationNames, currentBGImageOperation.ToString(),
                            null, selectedItem =>
                            {
                                string selectedItemName = selectedItem.ToString();
                                if (selectedItemName == eventData.GetParameterByIndex(bgImageOperationSubEventParamIndex))
                                {
                                    return selectedItemName;
                                }
                                eventData.SetParameterByIndex(bgImageOperationSubEventParamIndex, bgImageOperationNames.FindIndex(operation => operation == selectedItemName).ToString());
                                this.RefreshBGImageOperationSubEventElements(eventVO);
                                return selectedItemName;
                            });
                        eventVO.parameterElements.Insert(0, bgImageOperationPopupField);
                        break;
                    case SDSDialogueEventType.BGMOperations:
                        eventVO.defaultDescription = "BGM Operations";
                        eventVO.objectField = SDSElementUtility.CreateObjectField<AudioClip>(null, callback =>
                        {
                            eventData.AssetObject = callback.newValue;
                        });

                        //音量参数
                        bool hasBGMVolumeParam = eventData.HasParameterByIndex(0);
                        float curBGMVolume = (float)eventData.GetParsedParameterByIndex<float>(0);
                        if (!hasBGMVolumeParam)
                        {
                            eventData.SetParameterByIndex(0, 1f.ToString());
                            curBGMVolume = 1f;
                        }
                        string bgmLabelText = "音量：{0}";
                        var bgmVolumeLabel = SDSElementUtility.CreateLabel(string.Format(bgmLabelText, curBGMVolume));
                        var bgmVolumeSlider = SDSElementUtility.CreateSlider(0f, 1f, true, curBGMVolume, string.Empty, callback =>
                        {
                            bgmVolumeLabel.text = string.Format(bgmLabelText, callback.newValue);
                            eventData.SetParameterByIndex(0, callback.newValue.ToString());
                        });
                        eventVO.parameterElements.Add(bgmVolumeLabel);
                        eventVO.parameterElements.Add(bgmVolumeSlider);
                        break;
                    case SDSDialogueEventType.SFXOperations:
                        eventVO.defaultDescription = "Play SFX";
                        eventVO.objectField = SDSElementUtility.CreateObjectField<AudioClip>(null, callback =>
                        {
                            eventData.AssetObject = callback.newValue;
                        });

                        //音量参数
                        bool hasSFXVolumeParam = eventData.HasParameterByIndex(0);
                        float curSFXVolume = (float)eventData.GetParsedParameterByIndex<float>(0);
                        if (!hasSFXVolumeParam)
                        {
                            eventData.SetParameterByIndex(0, 1f.ToString());
                            curSFXVolume = 1f;
                        }
                        string sfxlabelText = "音量：{0}";
                        var sfxVolumeLabel = SDSElementUtility.CreateLabel(string.Format(sfxlabelText, curSFXVolume));
                        var sfxVolumeSlider = SDSElementUtility.CreateSlider(0f, 1f, true, curSFXVolume, string.Empty, callback =>
                        {
                            sfxVolumeLabel.text = string.Format(sfxlabelText, callback.newValue);
                            eventData.SetParameterByIndex(0, callback.newValue.ToString());
                        });
                        eventVO.parameterElements.Add(sfxVolumeLabel);
                        eventVO.parameterElements.Add(sfxVolumeSlider);
                        break;
                }
                this.eventVOs.Add(eventVO);
            }

            this.RefreshEventArea();
        }

        private void RefreshEventArea()
        {
            this.eventsFoldout.Clear();

            foreach (EventVO eventVO in this.eventVOs)
            {
                if (this.eventsFoldout == null || eventVO.objectField == null)
                    continue;

                eventVO.objectField.value = eventVO.eventData.AssetObject;

                VisualElement eventContainer = new VisualElement();
                eventContainer.AddClasses("sds-node__event-container");
                eventContainer.userData = eventVO.eventData;

                //事件标题
                TextField eventTitleTextField = SDSElementUtility.CreateTextField(eventVO.defaultDescription, null, callback =>
                {
                    eventVO.eventData.Description = callback.newValue;
                });
                if (!string.IsNullOrEmpty(eventVO.eventData.Description))
                {
                    eventTitleTextField.value = eventVO.eventData.Description;
                }
                else
                {
                    eventVO.eventData.Description = eventVO.defaultDescription;
                }
                eventTitleTextField.AddClasses("sds-node__text-field", "sds-node__text-field__hidden", "sds-node__choice-text-field");

                //删除事件按钮
                Button deleteButton = SDSElementUtility.CreateButton("X", () =>
                {
                    int index = this.Events.FindIndex(e => e == eventContainer.userData);
                    this.Events.RemoveAt(index);
                    this.eventVOs.RemoveAt(index);
                    this.RefreshEventArea();
                });
                deleteButton.AddClasses("sds-node__button");

                eventContainer.Add(eventTitleTextField);
                eventContainer.Add(eventVO.objectField);
                foreach (var parameterElement in eventVO.parameterElements)
                {
                    eventContainer.Add(parameterElement);
                }
                eventContainer.Add(deleteButton);

                this.eventsFoldout.Add(eventContainer);
            }
        }

        private void CorrectErrorEvents()
        {
            //一句话只能有一个BGM和一个背景图
            var bgm = this.Events.Where(e => e.EventType == SDSDialogueEventType.BGMOperations);
            if (bgm.Count() > 1)
            {
                Debug.LogWarning($"一句对话只能拥有一首bgm嗷");
                this.eventVOs = this.eventVOs.Distinct(new EventVOComparer()).ToList();
                this.Events = this.Events.Distinct(new EventSaveDataComparer()).ToList();
            }
            var bgImage = this.Events.Where(e => e.EventType == SDSDialogueEventType.BackgroundImageOperations);
            if (bgImage.Count() > 1)
            {
                Debug.LogWarning($"一句对话只能有一个背景图");
                this.eventVOs = this.eventVOs.Distinct(new EventVOComparer()).ToList();
                this.Events = this.Events.Distinct(new EventSaveDataComparer()).ToList();
            }
        }

        private string OnSelectedPopupFieldItem(object eventTypeObj)
        {
            SDSDialogueEventType eventType = (SDSDialogueEventType)eventTypeObj;
            bool added = false;
            switch (eventType)
            {
                case SDSDialogueEventType.NullEvent:
                    return SDSDialogueEventType.NullEvent.ToString();
                case SDSDialogueEventType.ImageOperations:
                case SDSDialogueEventType.BackgroundImageOperations:
                case SDSDialogueEventType.BGMOperations:
                case SDSDialogueEventType.SFXOperations:
                    this.Events.Add(new SDSEventSaveData() { EventType = eventType });
                    added = true;
                    break;
            }

            if (added)
            {
                //this.DrawEvents();
                this.RefreshEventDatas();
                this.OnEventSelected?.Invoke();
            }

            //选择后，PopupField所显示的string
            return SDSDialogueEventType.NullEvent.ToString();
        }

        #region SubEventElementsHandlers
        /// <summary>
        /// 绘制图片事件的子事件元素
        /// </summary>
        /// <param name="eventVO"></param>
        private void RefreshImageOperationSubEventElements(EventVO eventVO)
        {
            //清理旧Elements，但排除第一个子事件选项PopupField
            if (eventVO.parameterElements.Count > 1)
                eventVO.parameterElements.RemoveRange(1, eventVO.parameterElements.Count - 1);

            SDSDialogueImageEventOperations currentOperation = (SDSDialogueImageEventOperations)eventVO.eventData.GetParsedParameterByIndex<int>((int)SDSDialogueEventParameterEnum.ImageOperations.OperationType);
            switch (currentOperation)
            {
                case SDSDialogueImageEventOperations.Show:
                    //显示图片
                    int presetParamIndex = (int)SDSDialogueEventParameterEnum.ImageOperations.ShowImagePresetPositionType;
                    int xPosParamIndex = (int)SDSDialogueEventParameterEnum.ImageOperations.ShowImageXPosition;
                    int yPosParamIndex = (int)SDSDialogueEventParameterEnum.ImageOperations.ShowImageYPosition;
                    RefreshPresetOrCustomPosElements(eventVO, presetParamIndex, xPosParamIndex, yPosParamIndex);

                    break;
                case SDSDialogueImageEventOperations.Hide:
                    int hideParamIndex = (int)SDSDialogueEventParameterEnum.ImageOperations.HideImageTransitionTime;
                    var currentHideTime = eventVO.eventData.GetParameterByIndex(hideParamIndex);
                    this.RefreshTimeConsumeElements(eventVO, currentHideTime, "隐藏所花时间（秒）", hideParamIndex);
                    break;
                case SDSDialogueImageEventOperations.Move:
                    //移动图片
                    RefreshPresetOrCustomPosElements(eventVO, (int)SDSDialogueEventParameterEnum.ImageOperations.MoveImagePresetPositionType,
                        (int)SDSDialogueEventParameterEnum.ImageOperations.MoveImageXPosition, (int)SDSDialogueEventParameterEnum.ImageOperations.MoveImageYPosition);

                    int moveParamIndex = (int)SDSDialogueEventParameterEnum.ImageOperations.MoveImageConsumeTime;
                    var currentMoveTime = eventVO.eventData.GetParameterByIndex(moveParamIndex);
                    this.RefreshTimeConsumeElements(eventVO, currentMoveTime, "移动时间（秒）", moveParamIndex);
                    break;
            }
            this.RefreshEventArea();

            void RefreshPresetOrCustomPosElements(EventVO eventVO, int presetParamIndex, int xPosIndex, int yPosIndex)
            {
                //预设坐标选项
                List<object> presetPosNames = Enum.GetValues(typeof(SDSSpritePresetPosition)).Cast<object>().ToList();
                var currentPresetPos = presetPosNames.FirstOrDefault(preset => preset.ToString() == eventVO.eventData.GetParameterByIndex(presetParamIndex));
                if (currentPresetPos == null)//默认为自定义坐标
                {
                    eventVO.eventData.SetParameterByIndex(presetParamIndex, SDSSpritePresetPosition.CustomizedPosition.ToString());
                }

                Vector2Field showVector2Field = null;
                var presetPopupField = SDSElementUtility.CreatePopupField<object>(presetPosNames, currentPresetPos ?? SDSSpritePresetPosition.CustomizedPosition,
                    null,
                    selectedObj =>
                    {
                        //自定义坐标
                        if (eventVO.eventData.GetParameterByIndex(presetParamIndex) == SDSSpritePresetPosition.CustomizedPosition.ToString())
                        {
                            Vector2 currentCustomPos = new Vector2((float)eventVO.eventData.GetParsedParameterByIndex<float>(xPosIndex), (float)eventVO.eventData.GetParsedParameterByIndex<float>(yPosIndex));
                            eventVO.eventData.SetParameterByIndex(xPosIndex, currentCustomPos.x.ToString());
                            eventVO.eventData.SetParameterByIndex(yPosIndex, currentCustomPos.y.ToString());
                            showVector2Field = SDSElementUtility.CreateVector2Field(currentCustomPos, string.Empty, callback =>
                            {
                                eventVO.eventData.SetParameterByIndex(xPosIndex, callback.newValue.x.ToString());
                                eventVO.eventData.SetParameterByIndex(yPosIndex, callback.newValue.y.ToString());
                            });
                        }

                        string selectedName = selectedObj.ToString();
                        if (selectedName == eventVO.eventData.GetParameterByIndex(presetParamIndex))
                            return selectedName;
                        eventVO.eventData.SetParameterByIndex(presetParamIndex, selectedName);

                        this.RefreshImageOperationSubEventElements(eventVO);
                        return selectedName;
                    });
                eventVO.parameterElements.Add(presetPopupField);
                if (showVector2Field != null)
                    eventVO.parameterElements.Add(showVector2Field);
            }
        }

        /// <summary>
        /// 绘制背景图子事件元素
        /// </summary>
        /// <param name="eventVO"></param>
        private void RefreshBGImageOperationSubEventElements(EventVO eventVO)
        {
            if (eventVO.parameterElements.Count > 1)
                eventVO.parameterElements.RemoveRange(1, eventVO.parameterElements.Count - 1);

            SDSDialogueBackgroundImageEventOperations currentOperation = (SDSDialogueBackgroundImageEventOperations)eventVO.eventData.GetParsedParameterByIndex<int>((int)SDSDialogueEventParameterEnum.BGImageOperations.OperationType);
            switch (currentOperation)
            {
                case SDSDialogueBackgroundImageEventOperations.Show:
                    int showParamIndex = (int)SDSDialogueEventParameterEnum.BGImageOperations.ShowBGImageTransitionTime;
                    string currentShowConsumeTime = eventVO.eventData.GetParameterByIndex(showParamIndex);
                    this.RefreshTimeConsumeElements(eventVO, currentShowConsumeTime, "显示所花时间（秒）", showParamIndex);
                    break;
                case SDSDialogueBackgroundImageEventOperations.Hide:
                    int hideParamIndex = (int)SDSDialogueEventParameterEnum.BGImageOperations.HideBGImageTransitionTime;
                    string currentHideConsumeTime = eventVO.eventData.GetParameterByIndex(hideParamIndex);
                    this.RefreshTimeConsumeElements(eventVO, currentHideConsumeTime, "显示所花时间（秒）", hideParamIndex);
                    break;
            }
            this.RefreshEventArea();
        }

        private void RefreshTimeConsumeElements(EventVO eventVO, string currentMoveTime, string label, int paramIndex)
        {
            var timeConsumeTextField = SDSElementUtility.CreateTextField(currentMoveTime, label, callback =>
            {
                string newValue = string.IsNullOrEmpty(callback.newValue) ? "0" : callback.newValue;
                if (float.TryParse(newValue, out float parsedNewValue))
                {
                    eventVO.eventData.SetParameterByIndex(paramIndex, newValue);
                }
                else
                {
                    Debug.LogWarning($"非法浮点数值: {newValue}");
                }
            });
            eventVO.parameterElements.Add(timeConsumeTextField);
        }
        #endregion

        /// <summary>
        /// 用于检查一句对话中是否有多个背景图事件和bgm事件
        /// </summary>
        private class EventSaveDataComparer : IEqualityComparer<SDSEventSaveData>
        {
            public bool Equals(SDSEventSaveData x, SDSEventSaveData y)
            {
                if ((x.EventType == SDSDialogueEventType.BackgroundImageOperations && x.EventType == y.EventType) ||
                    (x.EventType == SDSDialogueEventType.BGMOperations && x.EventType == y.EventType))
                {
                    return true;
                }
                return false;
            }

            public int GetHashCode(SDSEventSaveData obj)
            {
                return 0;
            }
        }

        /// <summary>
        /// 用于检查一句对话中是否有多个背景图事件和bgm事件
        /// </summary>
        private class EventVOComparer : IEqualityComparer<EventVO>
        {
            public bool Equals(EventVO x, EventVO y)
            {
                if ((x.eventData.EventType == SDSDialogueEventType.BackgroundImageOperations && x.eventData.EventType == y.eventData.EventType) ||
                    (x.eventData.EventType == SDSDialogueEventType.BGMOperations && x.eventData.EventType == y.eventData.EventType))
                {
                    return true;
                }
                return false;
            }

            public int GetHashCode(EventVO obj)
            {
                return 0;
            }
        }
    }
}