using SDS.Data.Save;
using SDS.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace SDS.Elements
{
    public partial class SDSNode
    {
        private Foldout dialogueContentsFoldout;
        private List<DialogueContentVO> dialogueContentVOs = new List<DialogueContentVO>();
        private class DialogueContentVO
        {
            public SDSDialogueContentSaveData saveData;
            public Label countLabel;
            public TextField textField;
            public TextField spokesmanField;
            public Button deleteButton;
            public VisualElement dialogueContentContainer;
        }

        public void DrawContentArea(VisualElement container)
        {
            this.dialogueContentsFoldout = SDSElementUtility.CreateFoldout("Dialogue Contents");

            this.RefreshDialogueContentDatas();

            container.Add(this.dialogueContentsFoldout);
        }

        private void RefreshDialogueContentDatas(bool addNewDialogue = false)
        {
            this.dialogueContentVOs.Clear();
            if (this.Contents.Count == 0)
            {
                this.Contents.Add(new SDSDialogueContentSaveData() { Text = "Dialogue Text", Spokesman = "Spokesman" });
            }
            else if (addNewDialogue)
            {
                this.Contents.Add(new SDSDialogueContentSaveData() { Spokesman = this.Contents.Last()?.Spokesman });//默认延续发言人
            }

            int count = 1;
            foreach(SDSDialogueContentSaveData content in this.Contents)
            {
                Label countLabel = SDSElementUtility.CreateLabel(count++ + ".");
                countLabel.AddClasses("sds-node__content-count-label");

                TextField spokesmanField = SDSElementUtility.CreateTextField(content.Spokesman, null, callback =>
                {
                    content.Spokesman = callback.newValue;
                });
                spokesmanField.AddClasses("sds-node__text-field", "sds-node__text-field__hidden", "sds-node__choice-text-field");

                TextField textTextField = SDSElementUtility.CreateTextArea(content.Text, null, callback =>
                {
                    content.Text = callback.newValue;
                });
                textTextField.AddClasses("sds-node__text-field", "sds-node__quote-text-field");

                VisualElement container = new VisualElement();
                container.userData = content;

                Button deleteButton = SDSElementUtility.CreateButton("X", () =>
                {
                    int index = this.Contents.FindIndex(c => c == container.userData);
                    this.Contents.RemoveAt(index);
                    this.dialogueContentVOs.RemoveAt(index);
                    this.RefreshDialogueContentArea();
                    this.RefreshDialogueContentLabelValue();
                });
                deleteButton.userData = content;
                deleteButton.AddClasses("sds-node__button");

                this.dialogueContentVOs.Add(new DialogueContentVO()
                {
                    countLabel = countLabel,
                    spokesmanField = spokesmanField,
                    textField = textTextField,
                    saveData = content,
                    deleteButton = deleteButton,
                    dialogueContentContainer = container
                });
            }

            this.RefreshDialogueContentArea();
        }

        private void RefreshDialogueContentArea()
        {
            this.dialogueContentsFoldout.Clear();

            foreach (DialogueContentVO vo in this.dialogueContentVOs)
            {
                VisualElement dialogueContentContainer = vo.dialogueContentContainer;
                dialogueContentContainer.Clear();

                dialogueContentContainer.Add(vo.countLabel);
                dialogueContentContainer.Add(vo.spokesmanField);
                dialogueContentContainer.Add(vo.textField);
                if (this.dialogueContentVOs.Count > 1)
                    dialogueContentContainer.Add(vo.deleteButton);

                this.dialogueContentsFoldout.Add(dialogueContentContainer);
            }

            Button addDialogueButton = SDSElementUtility.CreateButton("Add New Dialogue", () =>
            {
                this.RefreshDialogueContentDatas(true);
            });
            addDialogueButton.AddClasses("sds-node__button");
            this.dialogueContentsFoldout.Add(addDialogueButton);
        }

        private void RefreshDialogueContentLabelValue()
        {
            int count = 1;
            foreach (DialogueContentVO vo in this.dialogueContentVOs)
            {
                vo.countLabel.text = count++ + ".";
            }
        }
    }
}