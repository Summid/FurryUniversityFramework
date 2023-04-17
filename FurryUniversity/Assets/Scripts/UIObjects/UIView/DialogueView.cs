using DG.Tweening;
using SDS.Data;
using SDS.ScriptableObjects;
using SFramework.Adapter;
using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using SFramework.Utilities.Archive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    [UIView("DialogueView", EnumUIType.Page)]
    public partial class DialogueView : UIViewBase
    {
        private SDSDialogueContainerSO currentDialogueContainer;//章节
        private SDSDialogueSO currentDialogueNode;//对话节点
        private SDSDialogueContentData currentDialogueContent;//对话句子

        private float expandDuration = 0.5f;
        private RectTransform topButtonsNodeRectTrans;
        private RectTransform expandButtonRectTrans;

        protected override void OnAwake()
        {
            this.topButtonsNodeRectTrans = this.TopButtonsNode_ContentSizeFitter.GetComponent<RectTransform>();
            this.expandButtonRectTrans = this.ExpandButton_Button.GetComponent<RectTransform>();
            
            this.SettingsButton_Button.onClick.AddListener(() => GameManager.Instance.UIManager.ShowUIAsync<SettingsView>().Forget());
            this.ArchiveButtons_Button.onClick.AddListener(() => GameManager.Instance.UIManager.ShowUIAsync<ArchiveView>().Forget());
            this.ExpandButton_Button.onClick.AddListener(this.OnClickExpandButton);
            this.BackgroundButton_Button.onClick.AddListener(this.Roll2NextDialogueContent);
        }

        #region Dialogue
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
        #endregion

        private void OnClickExpandButton()
        {
            float rotateValue;
            if (this.topButtonsNodeRectTrans.anchoredPosition.y > 0)
            {
                this.topButtonsNodeRectTrans.DOAnchorPosByAdapter(
                    new Vector2(this.topButtonsNodeRectTrans.anchoredPosition.x, 0), this.expandDuration);
                rotateValue = 180f;
            }
            else
            {
                this.topButtonsNodeRectTrans.DOAnchorPosByAdapter(
                    new Vector2(
                        this.topButtonsNodeRectTrans.anchoredPosition.x,
                        this.topButtonsNodeRectTrans.rect.height), this.expandDuration);
                rotateValue = 0f;
            }

            this.expandButtonRectTrans.DOLocalRotate(new Vector3(0f, 0f, rotateValue), this.expandDuration).SetEase(Ease.Linear)
                .SetAutoKill();
        }
    }
}
