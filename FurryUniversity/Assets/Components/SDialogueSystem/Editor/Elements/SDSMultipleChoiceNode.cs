using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDS.Enumerations;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using SDS.Utilities;
using SDS.Windows;
using SDS.Data.Save;

namespace SDS.Elements
{
    public class SDSMultipleChoiceNode : SDSNode
    {
        public override void Initialize(string nodeName, SDSGraphView graphView, Vector2 position)
        {
            base.Initialize(nodeName, graphView, position);

            this.DialogueType = SDSDialogueType.MultipleChoice;

            SDSChoiceSaveData choiceData = new SDSChoiceSaveData()
            {
                Text = "New Choice"
            };
            this.Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            //Main Container
            Button addChoiceButton = SDSElementUtility.CreateButton("Add Choice", () =>
            {
                SDSChoiceSaveData choiceData = new SDSChoiceSaveData()
                {
                    Text = "New Choice"
                };
                this.Choices.Add(choiceData);

                Port choicePort = this.CreateChoicePort(choiceData);

                this.outputContainer.Add(choicePort);
            });
            addChoiceButton.AddClasses("sds-node__button");
            this.mainContainer.Insert(1, addChoiceButton);//放在titleContainer后

            //Output Container
            //为每一个选项生成一个输出端口，此节点有多个选项
            foreach (SDSChoiceSaveData choiceData in this.Choices)
            {
                Port choicePort = this.CreateChoicePort(choiceData);

                this.outputContainer.Add(choicePort);
            }

            this.RefreshExpandedState();
        }

        #region Elements Creation
        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();
            choicePort.portName = string.Empty;

            choicePort.userData = userData;
            SDSChoiceSaveData choiceData = userData as SDSChoiceSaveData;

            Button deleteChoiceButton = SDSElementUtility.CreateButton("X", () =>
            {
                if (this.Choices.Count == 1) return;//至少得有一个选项
                if (choicePort.connected)
                {
                    this.graphView.DeleteElements(choicePort.connections);//移除连接
                }

                this.Choices.Remove(choiceData);//移除数据
                this.graphView.RemoveElement(choicePort);//移除port
            });
            deleteChoiceButton.AddClasses("sds-node__button");

            TextField choiceTextField = SDSElementUtility.CreateTextField(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });
            choiceTextField.AddClasses("sds-node__text-field", "sds-node__text-field__hidden", "sds-node__choice-text-field");

            //元素将从右往左绘制
            choicePort.Add(deleteChoiceButton);
            choicePort.Add(choiceTextField);
            return choicePort;
        }
        #endregion
    }
}