using SDS.Data;
using SDS.ScriptableObjects;
using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using SFramework.Utilities.Archive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    [UIView("DialogueView", EnumUIType.Page)]
    public partial class DialogueView : UIViewBase, ISavable
    {
        private SDSDialogueContainerSO currentDialogueContainer;//章节
        private SDSDialogueSO currentDialogueNode;//对话节点
        private SDSDialogueContentData currentDialogueContent;//对话句子

        protected override void OnAwake()
        {
            this.SettingsButton_Button.onClick.AddListener((() => GameManager.Instance.UIManager.ShowUIAsync<SettingsView>().Forget()));
            this.BackgroundButton_Button.onClick.AddListener(this.Roll2NextDialogueContent);
        }

        public void SetNewDialogueContainer(SDSDialogueContainerSO dialogueContainer)
        {
            this.currentDialogueContainer = dialogueContainer;
            if (this.currentDialogueContainer.TryGetFirstDialogue(out this.currentDialogueNode))
            {
                this.Roll2NextDialogueNode();
            }
            else
            {
                Debug.LogError($"对话文件 {this.currentDialogueContainer.FileName} 未找到开始节点");
            }
        }

        private void Roll2NextDialogueNode()
        {
            if (this.currentDialogueNode.TryGetFirstDialogueContent(out this.currentDialogueContent))
            {
                this.SetDialogueContentInfos();
            }
        }

        private void Roll2NextDialogueContent()
        {
            if (this.currentDialogueNode.TryGetNextDialogueContent(this.currentDialogueContent,
                    out this.currentDialogueContent))
            {
                this.SetDialogueContentInfos();
            }
        }

        private void SetDialogueContentInfos()
        {
            if (this.currentDialogueContent == null)
            {
                return;
            }
            
            this.DialogueSpeakerText.text = this.currentDialogueContent.Spokesman;
            this.DialogueContentText.text = this.currentDialogueContent.Text;
        }

        public ArchiveObject OnSave()
        {
            ArchiveObject archiveObject = new ArchiveObject();
            archiveObject.ChapterName = "HelloChapterName";
            archiveObject.DialogueName = "HelloDialogueName";
            archiveObject.ContentIndex = 233;
            return archiveObject;
        }
    }
}
