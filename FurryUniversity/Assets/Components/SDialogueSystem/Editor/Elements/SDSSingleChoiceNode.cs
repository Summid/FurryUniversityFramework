using UnityEngine;
using SDS.Enumerations;
using UnityEditor.Experimental.GraphView;
using SDS.Utilities;
using SDS.Windows;
using SDS.Data.Save;

namespace SDS.Elements
{
    public class SDSSingleChoiceNode : SDSNode
    {
        public override void Initialize(string nodeName, SDSGraphView graphView, Vector2 position)
        {
            base.Initialize(nodeName, graphView, position);

            this.DialogueType = SDSDialogueType.SingleChoice;

            SDSChoiceSaveData choiceData = new SDSChoiceSaveData()
            {
                Text = "Next Dialogue"
            };
            this.Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            //Output Container
            //为每一个选项生成一个输出端口，此节点只有一个选项
            foreach (SDSChoiceSaveData choice in this.Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);

                choicePort.userData = choice;
                choicePort.portName = choice.Text;

                this.outputContainer.Add(choicePort);
            }

            this.RefreshExpandedState();
        }
    }
}