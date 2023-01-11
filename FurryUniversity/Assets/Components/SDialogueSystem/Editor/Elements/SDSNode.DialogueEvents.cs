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
            this.DrawEvents();

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
                //TODO
            }

            this.RefreshEventArea();
        }

        private void RefreshEventArea()
        {

        }

        private void DrawEvents()
        {
            this.eventsFoldout.Clear();

            this.CorrectErrorEvents();

            foreach (SDSEventSaveData eventData in this.Events)
            {
                ObjectField objectField = null;
                string defaultDescription = string.Empty;
                List<VisualElement> parameterElements = new List<VisualElement>();
                switch (eventData.EventType)
                {
                    case SDSDialogueEventType.NullEvent:
                        continue;
                    case SDSDialogueEventType.ImageOperations:
                        defaultDescription = "Image Operations";
                        objectField = SDSElementUtility.CreateObjectField<Sprite>(null, callback =>
                        {
                            eventData.AssetObject = callback.newValue;
                        });
                        List<object> imageOperationObjs = Enum.GetValues(typeof(SDSDialogueImageEventOperations)).Cast<object>().ToList();//不能直接用Cast转换为List<string>，这里手动搞一下
                        List<string> imageOperationsNames = new List<string>();
                        imageOperationObjs.ForEach(obj => imageOperationsNames.Add(obj.ToString()));
                        SDSDialogueImageEventOperations currentOperation = (SDSDialogueImageEventOperations)eventData.GetParsedParameterByIndex<int>(0);
                        var imageOperationsPopupField = SDSElementUtility.CreatePopupField<string>(imageOperationsNames, currentOperation.ToString(),
                            null, selectedItem =>
                            {
                                //TODO 调用提取的Refresh方法，而不是在下面的callback中调用DrawEvents，取消注册下面的callback
                                Debug.Log("onSelectedItem" + " " + selectedItem.ToString());
                                string selectedItemName = selectedItem.ToString();
                                if (selectedItemName == eventData.GetParameterByIndex(0))
                                {
                                    return selectedItemName;
                                }
                                eventData.SetParameterByIndex(0, imageOperationsNames.FindIndex(operation => operation == selectedItem.ToString()).ToString());
                                return selectedItemName;
                            }, callback =>
                            {
                                Debug.Log("onValueChanged new value" + " " + callback.newValue);
                                //eventData.SetParameterByIndex(0, callback.newValue.ToString());
                                this.DrawEvents();
                            });
                        parameterElements.Add(imageOperationsPopupField);

                        //处理子事件
                        currentOperation = (SDSDialogueImageEventOperations)(int)eventData.GetParsedParameterByIndex<int>(0);
                        switch (currentOperation)
                        {
                            case SDSDialogueImageEventOperations.Show:
                                Debug.Log("Show");
                                break;
                            case SDSDialogueImageEventOperations.Hide:
                                Debug.Log("Hide");
                                break;
                            case SDSDialogueImageEventOperations.Move:
                                Debug.Log("Move");
                                break;
                        }
                        break;
                    case SDSDialogueEventType.ShowImage:
                        defaultDescription = "Image";
                        objectField = SDSElementUtility.CreateObjectField<Sprite>(null, callback =>
                        {
                            eventData.AssetObject = callback.newValue;
                        });

                        //预设坐标选项
                        List<object> presetPosNames = Enum.GetValues(typeof(SDSSpritePresetPosition)).Cast<object>().ToList();
                        var currentPresetPos = presetPosNames.FirstOrDefault(preset => preset.ToString() == eventData.GetParameterByIndex(0));
                        if (currentPresetPos == null)//默认为自定义坐标
                        {
                            eventData.SetParameterByIndex(0, SDSSpritePresetPosition.CustomizedPosition.ToString());
                        }
                        var presetPopupField = SDSElementUtility.CreatePopupField<object>(presetPosNames, currentPresetPos ?? SDSSpritePresetPosition.CustomizedPosition,
                            null,
                            (selectedObj) =>
                            {
                                string selectedName = selectedObj.ToString();
                                if (selectedName == eventData.GetParameterByIndex(0))
                                    return selectedName;
                                eventData.SetParameterByIndex(0, selectedName);
                                this.DrawEvents();
                                return selectedName;
                            });
                        parameterElements.Add(presetPopupField);

                        //自定义坐标
                        if (eventData.GetParameterByIndex(0) == SDSSpritePresetPosition.CustomizedPosition.ToString())
                        {
                            Vector2 currentCustomPos = new Vector2((float)eventData.GetParsedParameterByIndex<float>(1), (float)eventData.GetParsedParameterByIndex<float>(2));
                            var vector2Field = SDSElementUtility.CreateVector2Field(currentCustomPos, string.Empty, callback =>
                            {
                                eventData.SetParameterByIndex(1, callback.newValue.x.ToString());
                                eventData.SetParameterByIndex(2, callback.newValue.y.ToString());
                            });
                            parameterElements.Add(vector2Field);
                        }
                        break;
                    case SDSDialogueEventType.ShowBackgroundImage:
                        defaultDescription = "BGImage";
                        objectField = SDSElementUtility.CreateObjectField<Sprite>(null, callback =>
                        {
                            eventData.AssetObject = callback.newValue;
                        });
                        break;
                    case SDSDialogueEventType.PlayBGM:
                        defaultDescription = "BGM";
                        objectField = SDSElementUtility.CreateObjectField<AudioClip>(null, callback =>
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
                        parameterElements.Add(bgmVolumeLabel);
                        parameterElements.Add(bgmVolumeSlider);
                        break;
                    case SDSDialogueEventType.PlaySFX:
                        defaultDescription = "SFX";
                        objectField = SDSElementUtility.CreateObjectField<AudioClip>(null, callback =>
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
                        parameterElements.Add(sfxVolumeLabel);
                        parameterElements.Add(sfxVolumeSlider);
                        break;
                }

                //TODO 提取为refresh方法
                //绘制
                if (objectField != null && this.eventsFoldout != null)
                {
                    objectField.value = eventData.AssetObject;

                    VisualElement eventContainer = new VisualElement();
                    eventContainer.AddClasses("sds-node__event-container");
                    eventContainer.userData = eventData;

                    TextField textField = SDSElementUtility.CreateTextField(defaultDescription, null, callback =>
                    {
                        eventData.Description = callback.newValue;
                    });
                    if (!string.IsNullOrEmpty(eventData.Description))
                    {
                        textField.value = eventData.Description;
                    }
                    else
                    {
                        eventData.Description = defaultDescription;
                    }
                    textField.AddClasses("sds-node__text-field", "sds-node__text-field__hidden", "sds-node__choice-text-field");

                    Button deleteButton = SDSElementUtility.CreateButton("X", () =>
                    {
                        this.Events.Remove((SDSEventSaveData)eventContainer.userData);
                        this.DrawEvents();
                    });
                    deleteButton.AddClasses("sds-node__button");

                    eventContainer.Add(textField);
                    eventContainer.Add(objectField);
                    foreach (var parameterElement in parameterElements)
                    {
                        eventContainer.Add(parameterElement);
                    }
                    eventContainer.Add(deleteButton);

                    this.eventsFoldout.Add(eventContainer);
                }

            }
        }

        private void CorrectErrorEvents()
        {
            //一句话只能有一个BGM和一个背景图
            var bgm = this.Events.Where(e => e.EventType == SDSDialogueEventType.PlayBGM);
            if (bgm.Count() > 1)
            {
                Debug.LogWarning($"一句对话只能拥有一首bgm嗷");
                this.eventVOs = this.eventVOs.Distinct(new EventVOComparer()).ToList();
                this.Events = this.Events.Distinct(new EventComparer()).ToList();
            }
            var bgImage = this.Events.Where(e => e.EventType == SDSDialogueEventType.ShowBackgroundImage);
            if (bgImage.Count() > 1)
            {
                Debug.LogWarning($"一句对话只能有一个背景图");
                this.eventVOs = this.eventVOs.Distinct(new EventVOComparer()).ToList();
                this.Events = this.Events.Distinct(new EventComparer()).ToList();
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
                case SDSDialogueEventType.ShowImage:
                case SDSDialogueEventType.ShowBackgroundImage:
                case SDSDialogueEventType.ImageOperations:
                case SDSDialogueEventType.BackgroundImageOperations:
                case SDSDialogueEventType.PlayBGM:
                case SDSDialogueEventType.PlaySFX:
                    this.Events.Add(new SDSEventSaveData() { EventType = eventType });
                    added = true;
                    break;
            }

            if (added)
            {
                this.DrawEvents();
                this.OnEventSelected?.Invoke();
            }

            //选择后，PopupField所显示的string
            return SDSDialogueEventType.NullEvent.ToString();
        }

        /// <summary>
        /// 用于检查一句对话中是否有多个背景图事件和bgm事件
        /// </summary>
        private class EventComparer : IEqualityComparer<SDSEventSaveData>
        {
            public bool Equals(SDSEventSaveData x, SDSEventSaveData y)
            {
                if ((x.EventType == SDSDialogueEventType.BackgroundImageOperations && x.EventType == y.EventType) ||
                    (x.EventType == SDSDialogueEventType.PlayBGM && x.EventType == y.EventType))
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
                    (x.eventData.EventType == SDSDialogueEventType.PlayBGM && x.eventData.EventType == y.eventData.EventType))
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