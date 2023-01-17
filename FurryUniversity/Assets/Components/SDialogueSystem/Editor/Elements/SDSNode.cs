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
        public string ID { get; set; }
        public string DialogueName { get; set; }
        public List<SDSChoiceSaveData> Choices { get; set; }
        public List<SDSDialogueContentSaveData> Contents { get; set; }
        public SDSDialogueType DialogueType { get; set; }

        protected SDSGraphView graphView;
        private Color defaultBackgroundColor;
        public SDSGroup Group { get; set; }

        public virtual void Initialize(string nodeName, SDSGraphView graphView, Vector2 position)
        {
            this.ID = Guid.NewGuid().ToString();
            this.DialogueName = nodeName;
            this.Contents = new List<SDSDialogueContentSaveData>();
            this.Choices = new List<SDSChoiceSaveData>();
            this.Events = new List<SDSEventSaveData>();
            this.OnEventSelected += () => { this.popupField.index = 0; };//选择事件后，重置选择索引

            this.SetPosition(new Rect(position, Vector2.zero));

            this.mainContainer.AddClasses("sds-node__main-container");
            this.extensionContainer.AddClasses("sds-node__extension-container");

            this.graphView = graphView;
            this.defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);
        }

        public virtual void Draw()
        {
            //Title Container
            TextField dialogueNameTextField = SDSElementUtility.CreateTextField(this.DialogueName, null, callback =>
            {
                TextField target = callback.target as TextField;
                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();//排除空格和特殊字符

                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(this.DialogueName))//当dialogue name 从非空字符串改为空字符串，禁用保存按钮
                    {
                        ++this.graphView.NameErrorsAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(this.DialogueName))//dialogue name 从空字符串改为非空字符串，错误减一
                    {
                        --this.graphView.NameErrorsAmount;
                    }
                }

                if (this.Group == null)//在未分组情况下修改名字
                {
                    this.graphView.RemoveUngroupedNode(this);
                    this.DialogueName = target.value;
                    this.graphView.AddUngroupedNode(this);
                    return;
                }

                //在组内修改名字
                SDSGroup currentGroup = this.Group;
                this.graphView.RemoveGroupedNode(this, this.Group);
                this.DialogueName = target.value;
                this.graphView.AddGroupNode(this, currentGroup);
            });
            dialogueNameTextField.AddClasses(
                "sds-node__text-field",
                "sds-node__text-field__hidden",
                "sds-node__filename-text-field"
                );
            this.titleButtonContainer.Insert(0, dialogueNameTextField);

            //Input Container
            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Dialogue Connection";
            this.inputContainer.Add(inputPort);

            //Extensions Container

            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddClasses("sds-node__custom-data-container");

            this.DrawContentArea(customDataContainer);
            this.DrawEventArea(customDataContainer);
            this.extensionContainer.Add(customDataContainer);
        }

        #region Overrided Methods
        /// <summary>
        /// 右键菜单
        /// </summary>
        /// <param name="evt"></param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => this.DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => this.DisconnectOutputPorts());

            base.BuildContextualMenu(evt);
        }
        #endregion


        #region Utility Methods
        public void DisconnectAllPorts()
        {
            this.DisconnectInputPorts();
            this.DisconnectOutputPorts();
        }

        private void DisconnectInputPorts()
        {
            this.DisconnectPorts(this.inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            this.DisconnectPorts(this.outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }

                graphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = this.inputContainer.Children().First() as Port;
            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            this.mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            this.mainContainer.style.backgroundColor = this.defaultBackgroundColor;
        }

        #endregion
    }
}