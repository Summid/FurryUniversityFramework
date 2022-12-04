using SDS.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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
    }
}