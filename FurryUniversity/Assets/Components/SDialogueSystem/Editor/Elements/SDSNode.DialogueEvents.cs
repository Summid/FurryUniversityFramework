using SDS.CustomizedData;
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
                        eventVO.defaultDescription = "Image Operation";
                        eventVO.objectField = SDSElementUtility.CreateObjectField<Sprite>(null, callback =>
                        {
                            eventData.AssetObject = callback.newValue;
                        });

                        RefreshSubEventPopupField<SDSDialogueImageEventOperations>(eventVO, (int)SDSDialogueEventParameterEnum.ImageOperations.OperationType, this.RefreshImageOperationSubEventElements);
                        break;
                    case SDSDialogueEventType.BackgroundImageOperations:
                        eventVO.defaultDescription = "Background Image Operation";
                        eventVO.objectField = SDSElementUtility.CreateObjectField<Sprite>(null, callback =>
                        {
                            eventData.AssetObject = callback.newValue;
                        });

                        RefreshSubEventPopupField<SDSDialogueBackgroundImageEventOperations>(eventVO, (int)SDSDialogueEventParameterEnum.BGImageOperations.OperationType, this.RefreshBGImageOperationSubEventElements);
                        break;
                    case SDSDialogueEventType.BGMOperations:
                        eventVO.defaultDescription = "BGM Operation";
                        eventVO.objectField = SDSElementUtility.CreateObjectField<AudioClip>(null, callback =>
                        {
                            eventData.AssetObject = callback.newValue;
                        });

                        RefreshSubEventPopupField<SDSDialogueBGMEventOperations>(eventVO, (int)SDSDialogueEventParameterEnum.BGMOperation.OperationType, this.RefreshBGMOperationSubEventElements);
                        break;
                    case SDSDialogueEventType.SFXOperations:
                        eventVO.defaultDescription = "Play SFX";
                        eventVO.objectField = SDSElementUtility.CreateObjectField<AudioClip>(null, callback =>
                        {
                            eventData.AssetObject = callback.newValue;
                        });

                        RefreshSubEventPopupField<SDSDialogueSFXEventOperations>(eventVO, (int)SDSDialogueEventParameterEnum.SFXOperation.OperationType, this.RefreshSFXOperationSubEventElements);
                        break;
                    case SDSDialogueEventType.CharacterOperations:
                        eventVO.defaultDescription = "Character Operation";
                        eventVO.objectField = SDSElementUtility.CreateObjectField<Character>(null, callback =>
                        {
                            eventData.AssetObject = callback.newValue;
                        });

                        RefreshSubEventPopupField<SDSDialogueCharacterEventOperations>(eventVO, (int)SDSDialogueEventParameterEnum.CharacterOperation.OperationType,
                            this.RefreshCharacterOperationSubEventElements);
                        break;
                }
                this.eventVOs.Add(eventVO);
            }

            this.RefreshEventArea();

            void RefreshSubEventPopupField<T>(EventVO eventVO, int subEventParameterIndex, Action<EventVO> onSelectedItemCallback) where T : Enum
            {
                var operationObjs = Enum.GetValues(typeof(T)).Cast<object>().ToList();
                List<string> operationNames = new List<string>();
                operationObjs.ForEach(operation => operationNames.Add(operation.ToString()));
                T currentImageOperation = (T)eventVO.eventData.GetParsedParameterByIndex<int>(subEventParameterIndex);
                var operationsPopupField = SDSElementUtility.CreatePopupField<string>(operationNames, currentImageOperation.ToString(), null,
                    selectedItem =>
                    {
                        if (selectedItem == eventVO.eventData.GetParameterByIndex(subEventParameterIndex))
                        {
                            return selectedItem;
                        }
                        //保存的是子事件枚举的索引
                        eventVO.eventData.SetParameterByIndex(subEventParameterIndex, operationNames.FindIndex(operation => operation == selectedItem).ToString());
                        onSelectedItemCallback?.Invoke(eventVO);
                        return selectedItem;
                    });
                eventVO.parameterElements.Insert(0, operationsPopupField);
            }
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

                TextField delayTimeTextField = SDSElementUtility.CreateTextField(eventVO.eventData.DelayTime.ToString(), "延迟时间（秒）", callback =>
                {
                    string newValue = string.IsNullOrEmpty(callback.newValue) ? "0" : callback.newValue;
                    if (float.TryParse(newValue, out float parsedNewValue))
                    {
                        eventVO.eventData.DelayTime = parsedNewValue;
                    }
                    else
                    {
                        Debug.LogWarning($"非法浮点数值：{newValue}");
                    }
                });

                Toggle isOnExitToggle = SDSElementUtility.CreateToggle("退出事件", eventVO.eventData.IsEventOnExit, callback =>
                {
                    eventVO.eventData.IsEventOnExit = callback.newValue;
                });

                eventContainer.Add(eventTitleTextField);
                eventContainer.Add(eventVO.objectField);
                foreach (var parameterElement in eventVO.parameterElements)
                {
                    eventContainer.Add(parameterElement);
                }
                eventContainer.Add(delayTimeTextField);
                eventContainer.Add(isOnExitToggle);
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
                case SDSDialogueEventType.CharacterOperations:
                    this.Events.Add(new SDSEventSaveData() { EventType = eventType });
                    added = true;
                    break;
            }

            if (added)
            {
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

                    int showTransitionTimeParamIndex = (int)SDSDialogueEventParameterEnum.ImageOperations.ShowImageTransitionTime;
                    var currentTime = eventVO.eventData.GetParameterByIndex(showTransitionTimeParamIndex);
                    this.RefreshFloatValueElements(eventVO, currentTime, "显示所花费时间（秒）", showTransitionTimeParamIndex);
                    break;
                case SDSDialogueImageEventOperations.Hide:
                    int hideParamIndex = (int)SDSDialogueEventParameterEnum.ImageOperations.HideImageTransitionTime;
                    var currentHideTime = eventVO.eventData.GetParameterByIndex(hideParamIndex);
                    this.RefreshFloatValueElements(eventVO, currentHideTime, "隐藏所花时间（秒）", hideParamIndex);
                    break;
                case SDSDialogueImageEventOperations.Move:
                    //移动图片
                    RefreshPresetOrCustomPosElements(eventVO, (int)SDSDialogueEventParameterEnum.ImageOperations.MoveImagePresetPositionType,
                        (int)SDSDialogueEventParameterEnum.ImageOperations.MoveImageXPosition, (int)SDSDialogueEventParameterEnum.ImageOperations.MoveImageYPosition);

                    int moveParamIndex = (int)SDSDialogueEventParameterEnum.ImageOperations.MoveImageConsumeTime;
                    var currentMoveTime = eventVO.eventData.GetParameterByIndex(moveParamIndex);
                    this.RefreshFloatValueElements(eventVO, currentMoveTime, "移动时间（秒）", moveParamIndex);
                    break;
            }

            int useAliasEventIndex = (int)SDSDialogueEventParameterEnum.ImageOperations.UseAlias;
            bool useAliasValue = (bool)eventVO.eventData.GetParsedParameterByIndex<bool>(useAliasEventIndex);
            this.RefreshToggleElements(eventVO, useAliasValue.ToString(), "使用别名", useAliasEventIndex, newValue =>
            {
                this.RefreshImageOperationSubEventElements(eventVO);
            });
            if (useAliasValue)
            {
                int aliasIndex = (int)SDSDialogueEventParameterEnum.ImageOperations.Alias;
                string alias = eventVO.eventData.GetParsedParameterByIndex<string>(aliasIndex) as string;
                this.RefreshStringValueElements(eventVO, alias, "别名：", aliasIndex);
                this.RefreshEventArea();
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
                    this.RefreshFloatValueElements(eventVO, currentShowConsumeTime, "显示所花时间（秒）", showParamIndex);
                    break;
                case SDSDialogueBackgroundImageEventOperations.Hide:
                    int hideParamIndex = (int)SDSDialogueEventParameterEnum.BGImageOperations.HideBGImageTransitionTime;
                    string currentHideConsumeTime = eventVO.eventData.GetParameterByIndex(hideParamIndex);
                    this.RefreshFloatValueElements(eventVO, currentHideConsumeTime, "隐藏所花时间（秒）", hideParamIndex);
                    break;
            }
            this.RefreshEventArea();
        }

        /// <summary>
        /// 绘制 BGM Operations 子事件元素
        /// </summary>
        /// <param name="eventVO"></param>
        private void RefreshBGMOperationSubEventElements(EventVO eventVO)
        {
            if (eventVO.parameterElements.Count > 1)
                eventVO.parameterElements.RemoveRange(1, eventVO.parameterElements.Count - 1);

            SDSDialogueBGMEventOperations currentOperation = (SDSDialogueBGMEventOperations)eventVO.eventData.GetParsedParameterByIndex<int>((int)SDSDialogueEventParameterEnum.BGMOperation.OperationType);

            int transitionTimeParamIndex = (int)SDSDialogueEventParameterEnum.BGMOperation.TransitionTime;
            bool hasBGMTransitionTimeParam = eventVO.eventData.HasParameterByIndex(transitionTimeParamIndex);
            float curBGMTransitionTime = (float)eventVO.eventData.GetParsedParameterByIndex<float>(transitionTimeParamIndex);
            if (!hasBGMTransitionTimeParam)
            {
                eventVO.eventData.SetParameterByIndex(transitionTimeParamIndex, 0f.ToString());
                curBGMTransitionTime = 0f;
            }

            switch (currentOperation)
            {
                case SDSDialogueBGMEventOperations.Play:
                case SDSDialogueBGMEventOperations.Resume:
                    int volumeParamIndex = (int)SDSDialogueEventParameterEnum.BGMOperation.Volume;

                    bool hasBGMVolumeParam = eventVO.eventData.HasParameterByIndex(volumeParamIndex);
                    float curBGMVolume = (float)eventVO.eventData.GetParsedParameterByIndex<float>(volumeParamIndex);
                    if (!hasBGMVolumeParam)
                    {
                        eventVO.eventData.SetParameterByIndex(volumeParamIndex, 1f.ToString());
                        curBGMVolume = 1f;
                    }
                    string bgmLabelText = "音量：{0}";
                    this.RefreshSliderElements(eventVO, 0, 1, curBGMVolume, bgmLabelText, volumeParamIndex);

                    this.RefreshFloatValueElements(eventVO, curBGMTransitionTime.ToString(), "过渡时间（秒）", transitionTimeParamIndex);
                    break;
                case SDSDialogueBGMEventOperations.Pause:
                case SDSDialogueBGMEventOperations.Stop:
                    this.RefreshFloatValueElements(eventVO, curBGMTransitionTime.ToString(), "过渡时间（秒）", transitionTimeParamIndex);
                    break;
            }
            this.RefreshEventArea();
        }

        /// <summary>
        /// 绘制 SFX Operation 子事件元素
        /// </summary>
        /// <param name="eventVO"></param>
        private void RefreshSFXOperationSubEventElements(EventVO eventVO)
        {
            if (eventVO.parameterElements.Count > 1)
                eventVO.parameterElements.RemoveRange(1, eventVO.parameterElements.Count - 1);

            SDSDialogueSFXEventOperations currentOperation = (SDSDialogueSFXEventOperations)eventVO.eventData.GetParsedParameterByIndex<int>((int)SDSDialogueEventParameterEnum.SFXOperation.OperationType);

            int volumeParamIndex = (int)SDSDialogueEventParameterEnum.SFXOperation.Volume;
            int loopTimesParamIndex = (int)SDSDialogueEventParameterEnum.SFXOperation.LoopTimes;

            bool hasVolumeParam = eventVO.eventData.HasParameterByIndex(volumeParamIndex);
            float curVolumeValue = (float)eventVO.eventData.GetParsedParameterByIndex<float>(volumeParamIndex);
            if (!hasVolumeParam)
            {
                eventVO.eventData.SetParameterByIndex(volumeParamIndex, 1f.ToString());
                curVolumeValue = 1f;
            }

            bool hasTimesParam = eventVO.eventData.HasParameterByIndex(loopTimesParamIndex);
            int curLoopTimesValue = (int)eventVO.eventData.GetParsedParameterByIndex<int>(loopTimesParamIndex);
            if (!hasTimesParam)
            {
                eventVO.eventData.SetParameterByIndex(loopTimesParamIndex, 0f.ToString());
                curLoopTimesValue = 0;
            }

            switch (currentOperation)
            {
                case SDSDialogueSFXEventOperations.Play:
                    this.RefreshSliderElements(eventVO, 0, 1, curVolumeValue, "音量：{0}", volumeParamIndex);
                    this.RefreshIntValueElements(eventVO, curLoopTimesValue.ToString(), "循环次数", loopTimesParamIndex);
                    break;
            }
            this.RefreshEventArea();
        }

        /// <summary>
        /// 绘制 Character Operation 子事件元素
        /// </summary>
        /// <param name="eventVO"></param>
        private void RefreshCharacterOperationSubEventElements(EventVO eventVO)
        {
            if (eventVO.parameterElements.Count > 1)
                eventVO.parameterElements.RemoveRange(1, eventVO.parameterElements.Count - 1);

            SDSDialogueCharacterEventOperations currentOperation =
                (SDSDialogueCharacterEventOperations)eventVO.eventData.GetParsedParameterByIndex<int>((int)SDSDialogueEventParameterEnum.CharacterOperation
                    .OperationType);
            switch (currentOperation)
            {

                case SDSDialogueCharacterEventOperations.Show:
                    //显示角色
                    int presetParamIndex = (int)SDSDialogueEventParameterEnum.CharacterOperation.ShowCharacterPresetPositionType;
                    int xPosParamIndex = (int)SDSDialogueEventParameterEnum.CharacterOperation.ShowCharacterXPosition;
                    int yPosParamIndex = (int)SDSDialogueEventParameterEnum.CharacterOperation.ShowCharacterYPosition;

                    //立绘差分
                    int spriteDiffParamIndex = (int)SDSDialogueEventParameterEnum.CharacterOperation.ShowCharacterSpriteDifference;
                    List<object> allDiff = Enum.GetValues(typeof(SDSDialogueCharacterSpriteDifference)).Cast<object>().ToList();
                    var currentSpriteDiff = allDiff.FirstOrDefault(diff => diff.ToString() == eventVO.eventData.GetParameterByIndex(spriteDiffParamIndex));
                    if (currentSpriteDiff == null)
                    {
                        eventVO.eventData.SetParameterByIndex(spriteDiffParamIndex, SDSDialogueCharacterSpriteDifference.Idle.ToString());
                        currentSpriteDiff = SDSDialogueCharacterSpriteDifference.Idle;
                    }
                    var spriteDiffPopupField = SDSElementUtility.CreatePopupField(allDiff, currentSpriteDiff, null, selectedItem =>
                    {
                        eventVO.eventData.SetParameterByIndex(spriteDiffParamIndex, selectedItem.ToString());
                        return selectedItem.ToString();
                    });
                    eventVO.parameterElements.Add(spriteDiffPopupField);

                    RefreshPresetOrCustomPosElements(eventVO, presetParamIndex, xPosParamIndex, yPosParamIndex);
                    
                    int showTransitionTimeParamIndex = (int)SDSDialogueEventParameterEnum.CharacterOperation.ShowCharacterTransitionTime;
                    var currentTime = eventVO.eventData.GetParameterByIndex(showTransitionTimeParamIndex);
                    this.RefreshFloatValueElements(eventVO, currentTime, "显示所花费时间（秒）", showTransitionTimeParamIndex);
                    break;
                case SDSDialogueCharacterEventOperations.Hide:
                    int hideParamIndex = (int)SDSDialogueEventParameterEnum.CharacterOperation.HideCharacterTransitionTime;
                    var currentHideTime = eventVO.eventData.GetParameterByIndex(hideParamIndex);
                    this.RefreshFloatValueElements(eventVO, currentHideTime, "隐藏所花时间（秒）", hideParamIndex);
                    break;
                case SDSDialogueCharacterEventOperations.Move:
                    //移动角色
                    RefreshPresetOrCustomPosElements(eventVO, (int)SDSDialogueEventParameterEnum.CharacterOperation.MoveCharacterPresetPositionType,
                        (int)SDSDialogueEventParameterEnum.CharacterOperation.MoveCharacterXPosition, (int)SDSDialogueEventParameterEnum.CharacterOperation.MoveCharacterYPosition);

                    int moveParamIndex = (int)SDSDialogueEventParameterEnum.CharacterOperation.MoveCharacterConsumeTime;
                    var currentMoveTime = eventVO.eventData.GetParameterByIndex(moveParamIndex);
                    this.RefreshFloatValueElements(eventVO, currentMoveTime, "移动时间（秒）", moveParamIndex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            int useAliasEventIndex = (int)SDSDialogueEventParameterEnum.CharacterOperation.UseAlias;
            bool useAliasValue = (bool)eventVO.eventData.GetParsedParameterByIndex<bool>(useAliasEventIndex);
            this.RefreshToggleElements(eventVO, useAliasValue.ToString(), "使用别名", useAliasEventIndex, newValue =>
            {
                this.RefreshCharacterOperationSubEventElements(eventVO);
            });
            if (useAliasValue)
            {
                int aliasIndex = (int)SDSDialogueEventParameterEnum.CharacterOperation.Alias;
                string alias = eventVO.eventData.GetParsedParameterByIndex<string>(aliasIndex) as string;
                this.RefreshStringValueElements(eventVO, alias, "别名：", aliasIndex);
                this.RefreshEventArea();
            }

            this.RefreshEventArea();

            void RefreshPresetOrCustomPosElements(EventVO eventVO, int presetParamIndex, int xPosIndex, int yPosIndex)
            {
                //预设坐标选项
                List<object> presetPosNames = Enum.GetValues(typeof(SDSSpritePresetPosition)).Cast<object>().ToList();
                var currentPresetPos = presetPosNames.FirstOrDefault(preset => preset.ToString() == eventVO.eventData.GetParameterByIndex(presetParamIndex));
                if (currentPresetPos == null)//默认为自定义坐标
                {
                    eventVO.eventData.SetParameterByIndex(presetParamIndex,SDSSpritePresetPosition.CustomizedPosition.ToString());
                }

                Vector2Field showVector2Field = null;
                var presetPopupField = SDSElementUtility.CreatePopupField<object>(presetPosNames, currentPresetPos ?? SDSSpritePresetPosition.CustomizedPosition,
                    null,
                    selectedObj =>
                    {
                        //自定义坐标
                        if (eventVO.eventData.GetParameterByIndex(presetParamIndex) == SDSSpritePresetPosition.CustomizedPosition.ToString())
                        {
                            Vector2 currentCustomPos = new Vector2((float)eventVO.eventData.GetParsedParameterByIndex<float>(xPosIndex),
                                (float)eventVO.eventData.GetParsedParameterByIndex<float>(yPosIndex));
                            eventVO.eventData.SetParameterByIndex(xPosIndex,currentCustomPos.x.ToString());
                            eventVO.eventData.SetParameterByIndex(yPosIndex,currentCustomPos.y.ToString());
                            showVector2Field = SDSElementUtility.CreateVector2Field(currentCustomPos, string.Empty, callback =>
                            {
                                eventVO.eventData.SetParameterByIndex(xPosIndex,callback.newValue.x.ToString());
                                eventVO.eventData.SetParameterByIndex(yPosIndex,callback.newValue.y.ToString());
                            });
                        }

                        string selectedName = selectedObj.ToString();
                        if (selectedName == eventVO.eventData.GetParameterByIndex(presetParamIndex))
                            return selectedName;
                        eventVO.eventData.SetParameterByIndex(presetParamIndex, selectedName);
                        
                        this.RefreshCharacterOperationSubEventElements(eventVO);
                        return selectedName;
                    });
                eventVO.parameterElements.Add(presetPopupField);
                if (showVector2Field != null)
                    eventVO.parameterElements.Add(showVector2Field);
            }
        }
        
        private void RefreshFloatValueElements(EventVO eventVO, string currentValue, string label, int paramIndex)
        {
            var textField = SDSElementUtility.CreateTextField(currentValue, label, callback =>
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
            eventVO.parameterElements.Add(textField);
        }

        private void RefreshIntValueElements(EventVO eventVO, string currentValue, string label, int paramIndex)
        {
            var textField = SDSElementUtility.CreateTextField(currentValue, label, callback =>
            {
                string newValue = string.IsNullOrEmpty(callback.newValue) ? "0" : callback.newValue;
                if (int.TryParse(newValue, out int parsedNewValue))
                {
                    eventVO.eventData.SetParameterByIndex(paramIndex, newValue);
                }
                else
                {
                    Debug.LogWarning($"非法整形数值: {newValue}");
                }
            });
            eventVO.parameterElements.Add(textField);
        }

        private void RefreshStringValueElements(EventVO eventVO, string currentValue, string label, int paramIndex)
        {
            var textField = SDSElementUtility.CreateTextField(currentValue, label, callback =>
            {
                eventVO.eventData.SetParameterByIndex(paramIndex, callback.newValue);
            });
            eventVO.parameterElements.Add(textField);
        }

        private void RefreshToggleElements(EventVO eventVO, string currentValue, string label, int paramIndex, Action<bool> onValueChanged = null)
        {
            var toggle = SDSElementUtility.CreateToggle(label, bool.Parse(currentValue), callback =>
            {
                eventVO.eventData.SetParameterByIndex(paramIndex,callback.newValue.ToString());
                onValueChanged?.Invoke(callback.newValue);
            });
            eventVO.parameterElements.Add(toggle);
        }

        private void RefreshSliderElements(EventVO eventVO, float startValue, float endValue, float currentValue, string formatLabelValue, int paramIndex)
        {
            var labelField = SDSElementUtility.CreateLabel(string.Format(formatLabelValue, currentValue));
            var sliderField = SDSElementUtility.CreateSlider(startValue, endValue, true, currentValue, string.Empty, callback =>
            {
                labelField.text = string.Format(formatLabelValue, callback.newValue);
                eventVO.eventData.SetParameterByIndex(paramIndex, callback.newValue.ToString());
            });
            eventVO.parameterElements.Add(labelField);
            eventVO.parameterElements.Add(sliderField);
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