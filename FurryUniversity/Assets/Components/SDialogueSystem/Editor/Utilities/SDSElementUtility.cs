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

        public static PopupField<T> CreatePopupField<T>(List<T> choices, T defaultValue, Func<T, string> onListItems, Func<T, string> onSelectedItem, string label = "")
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
            vector2Field.RegisterValueChangedCallback(onValueChanged);
            return vector2Field;
        }
    }
}