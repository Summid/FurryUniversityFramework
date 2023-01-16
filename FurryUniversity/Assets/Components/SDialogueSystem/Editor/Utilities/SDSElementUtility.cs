using SDS.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDS.Utilities
{
    public static class SDSElementUtility
    {
        /// <summary>
        /// TextField
        /// </summary>
        /// <param name="value"></param>
        /// <param name="onValueChanged"></param>
        /// <returns></returns>
        public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = new TextField()
            {
                value = value,
                label = label,
            };

            if (onValueChanged != null)
            {
                textField.RegisterValueChangedCallback(onValueChanged);
            }

            return textField;
        }

        /// <summary>
        /// TextArea
        /// </summary>
        /// <param name="value"></param>
        /// <param name="onValueChanged"></param>
        /// <returns></returns>
        public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = CreateTextField(value, label, onValueChanged);

            textField.multiline = true;

            return textField;
        }

        /// <summary>
        /// Foldout
        /// </summary>
        /// <param name="title"></param>
        /// <param name="collapsed">是否默认折叠</param>
        /// <returns></returns>
        public static Foldout CreateFoldout(string title, bool collapsed = false)
        {
            Foldout foldout = new Foldout()
            {
                text = title,
                value = !collapsed
            };

            return foldout;
        }

        /// <summary>
        /// Button
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onClick"></param>
        /// <returns></returns>
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
                text = text
            };

            return button;
        }

        /// <summary>
        /// CreatePort For Node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="portName"></param>
        /// <param name="orientation"></param>
        /// <param name="direction"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static Port CreatePort(this SDSNode node, string portName = "", Orientation orientation = Orientation.Horizontal,
            Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));

            port.name = portName;

            return port;
        }

        /// <summary>
        /// 创建PupupField Element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="choices"></param>
        /// <param name="defaultValue"></param>
        /// <param name="onListItems">当展开列表时，每个待选元素都会触发一次</param>
        /// <param name="onSelectedItem">选择【不同的】元素后触发一次，包括第一次自动选择默认元素时也会触发</param>
        /// <param name="onValueChanged">选择【不同的】元素后触发一次，【不】包括第一次自动选择默认元素时，会在onSelectedItem之后触发</param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static PopupField<T> CreatePopupField<T>(List<T> choices, T defaultValue, Func<T, string> onListItems, Func<T, string> onSelectedItem, EventCallback<ChangeEvent<T>> onValueChanged = null, string label = "")
        {
            if (choices == null || choices.Count == 0)
            {
                Debug.LogWarning($"PopupField's choices is empty");
                return null;
            }

            if (!choices.Contains(defaultValue))
            {
                Debug.LogWarning($"PopupField's default value is not in choices");
                return null;
            }

            PopupField<T> popupField = new PopupField<T>(label, choices, defaultValue, onSelectedItem, onListItems);
            if (onValueChanged != null)
                popupField.RegisterValueChangedCallback(onValueChanged);
            return popupField;
        }

        /// <summary>
        /// ObjectField，用于扩展功能，播放音效、动效等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label"></param>
        /// <param name="onValueChanged"></param>
        /// <returns></returns>
        public static ObjectField CreateObjectField<T>(string label = "", EventCallback<ChangeEvent<UnityEngine.Object>> onValueChanged = null) where T : UnityEngine.Object
        {
            ObjectField objectField = new ObjectField()
            {
                objectType = typeof(T),
                label = label,
            };
            if (onValueChanged != null)
            {
                objectField.RegisterValueChangedCallback(onValueChanged);
            }
            return objectField;
        }

        public static Vector2Field CreateVector2Field(Vector2 defaultValue, string label = "", EventCallback<ChangeEvent<Vector2>> onValueChanged = null)
        {
            Vector2Field vector2Field = new Vector2Field()
            {
                label = label,
                value = defaultValue
            };
            if (onValueChanged != null)
                vector2Field.RegisterValueChangedCallback(onValueChanged);
            return vector2Field;
        }

        public static Slider CreateSlider(float startValue, float endValue, bool horizontalLayout, float defaultValue = 0, string label = "", EventCallback<ChangeEvent<float>> onValueChanged = null)
        {
            Slider slider = new Slider(label, startValue, endValue, horizontalLayout ? SliderDirection.Horizontal : SliderDirection.Vertical);
            slider.value = defaultValue;
            if (onValueChanged != null)
                slider.RegisterValueChangedCallback(onValueChanged);
            return slider;
        }

        public static Label CreateLabel(string labelValue)
        {
            Label label = new Label(labelValue);

            return label;
        }

        public static Toggle CreateToggle(string label = "", bool defaultValue = false, EventCallback<ChangeEvent<bool>> onValueChanged = null)
        {
            Toggle toggle = new Toggle()
            {
                //label = label,
                value = defaultValue,
                text = label
            };
            if (onValueChanged != null)
                toggle.RegisterValueChangedCallback(onValueChanged);
            return toggle;
        }
    }
}