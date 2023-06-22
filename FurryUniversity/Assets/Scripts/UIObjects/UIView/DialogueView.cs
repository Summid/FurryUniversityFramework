using DG.Tweening;
using SDS.Data;
using SDS.Enumerations;
using SDS.ScriptableObjects;
using SFramework.Adapter;
using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SFramework.Core.UI
{
    [UIView("DialogueView", EnumUIType.Page)]
    public partial class DialogueView : UIViewBase
    {
        private float expandDuration = 0.5f;
        private RectTransform topButtonsNodeRectTrans;
        private RectTransform expandButtonRectTrans;

        protected override void OnAwake()
        {
            this.topButtonsNodeRectTrans = this.TopButtonsNode_ContentSizeFitter.GetComponent<RectTransform>();
            this.expandButtonRectTrans = this.ExpandButton_Button.GetComponent<RectTransform>();
            
            this.SettingsButton_Button.onClick.AddListener(() => GameManager.Instance.UIManager.ShowUIAsync<SettingsView>().Forget());
            this.ArchiveButtons_Button.onClick.AddListener(() => GameManager.Instance.UIManager.ShowUIAsync<ArchiveView>().Forget());
            this.BackButton_Button.onClick.AddListener(() => this.HideAsync().Forget());
            this.ExpandButton_Button.onClick.AddListener(this.OnClickExpandButton);
            this.BackgroundButton_Button.onClick.AddListener(this.ExecuteNextStep);
        }

        #region Dialogue
        /// <summary>
        /// 对话状态，通过它判断下一步应该做什么
        /// </summary>
        public enum DialogueViewState
        {
            Null,//对话节点为空，未设置
            AwaitNextStartEvent,//未执行开始事件
            AwaitNextSentence,//执行完开始事件后，正在显示对话内容
            AwaitChoice,//播放完所有对话文本，等待玩家选择
            AwaitNextEndEvent,//玩家进行选择后，等待执行结束事件
            AwaitNextNode,//等待跳转到下一个对话节点
            End,//当前对话文件执行完毕，没有剩余对话节点
        }
        public bool Executing;
        public DialogueViewData ViewData;
        private bool needContinue;

        public void SetNewDialogueContainer(SDSDialogueContainerSO dialogueContainer)
        {
            if (this.ViewData == null)
                this.ViewData = new DialogueViewData();
            this.ViewData.SetNewDialogueContainer(dialogueContainer);
            this.ExecuteNextStep();
        }

        private async void ExecuteNextStep()
        {
            if (this.Executing)
                return;

            this.Executing = true;
            
            if(this.ViewData.DialogueViewState != DialogueViewState.AwaitChoice)
                this.ChoicesNode.gameObject.SetActive(false);
            
            switch (this.ViewData.DialogueViewState)
            {
                case DialogueViewState.Null:
                    Debug.LogError($"未设置对话文件");
                    break;
                case DialogueViewState.AwaitNextStartEvent:
                    await this.ExecuteEvents(true);
                    this.needContinue = true;
                    break;
                case DialogueViewState.AwaitNextSentence:
                    this.SetNextDialogueContent();
                    break;
                case DialogueViewState.AwaitChoice:
                    this.SetChoice();
                    break;
                case DialogueViewState.AwaitNextEndEvent:
                    await this.ExecuteEvents(false);
                    this.needContinue = true;
                    break;
                case DialogueViewState.AwaitNextNode:
                    this.ViewData.TryMove2NextDialogueNode();
                    this.needContinue = true;
                    break;
                case DialogueViewState.End:
                    //已经结束啦！
                    Debug.LogWarning($"this dialogue container has no more dialogue node");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.Executing = false;

            if (this.needContinue)
            {
                this.needContinue = false;
                this.ExecuteNextStep();
            }
        }

        private void SetNextDialogueContent()
        {
            var contentData = this.ViewData.GetNextDialogueContentData();
            if (contentData == null)
                return;
            this.DialogueSpeakerText.text = contentData.Spokesman;
            this.DialogueContentText.text = contentData.Text;
        }

        private async STask ExecuteEvents(bool start)
        {
            while (true)
            {
                var eventData = this.ViewData.GetNextEventData(start);
                if (eventData == null)
                    return;

                switch (eventData.EventType)
                {
                    case SDSDialogueEventType.NullEvent:
                        break;
                    case SDSDialogueEventType.ImageOperations:
                        break;
                    case SDSDialogueEventType.BackgroundImageOperations:
                        if (string.IsNullOrEmpty(eventData.AssetName)) break;
                        await this.BG_Image.SetSpriteAsync(eventData.AssetName, false);
                        break;
                    case SDSDialogueEventType.BGMOperations:
                        break;
                    case SDSDialogueEventType.SFXOperations:
                        break;
                    case SDSDialogueEventType.CharacterOperations:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                // await STask.Delay(500);//test
                Debug.LogWarning("test execute dialogue event");
            }
        }

        private void SetChoice()
        {
            switch (this.ViewData.DialogueType)
            {
                case SDSDialogueType.SingleChoice:
                    this.ViewData.TryChoose(0);
                    this.needContinue = true;
                    break;
                case SDSDialogueType.MultipleChoice:
                    this.ChoicesNode.gameObject.SetActive(true);
                    this.ChoicePool_UIItemPool.UpdateList<SDSDialogueChoiceData, DialogueChoiceItem>(this.ViewData.Choices, (item, index) =>
                    {
                        item.SetClickCallback(() =>
                        {
                            this.ViewData.TryChoose(index);
                            this.ExecuteNextStep();
                        });
                    });
                    break;
            }
        }

        public class DialogueViewData
        {
            public SDSDialogueContainerSO DialogueContainer { private set; get; }
            private SDSDialogueSO dialogueSO;
            
            public int DialogueContentIndex { private set; get; }
            public int StartEventIndex { private set; get; }
            public int EndEventIndex { private set; get; }
            public int ChoiceIndex { private set; get; }
            public List<SDSDialogueEventData> StartEvents { private set; get; }
            public List<SDSDialogueEventData> EndEvents { private set; get; }
            public SDSDialogueType DialogueType => this.dialogueSO.DialogueType;
            public List<SDSDialogueChoiceData> Choices { private set; get; }
            public bool IsLastDialogueNode { private set; get; }
            

            /// <summary>
            /// 设置新对话文件，重置相关数据
            /// </summary>
            /// <param name="container"></param>
            public void SetNewDialogueContainer(SDSDialogueContainerSO container)
            {
                if (container == null)
                {
                    Debug.LogError("DialogueView.DialogueViewData.SetNewDialogueContainer.container is NULL");
                    return;
                }

                this.DialogueContainer = container;

                if (this.DialogueContainer.TryGetFirstDialogue(out this.dialogueSO))
                {
                    this.ResetDialogueNodeData();
                }
                else
                {
                    Debug.LogError($"对话文件 {this.DialogueContainer.FileName} 未找到开始节点");
                }
            }

            /// <summary>
            /// 重置对话节点相关数据，跳转到下一对话节点前需调用
            /// </summary>
            private void ResetDialogueNodeData()
            {
                this.DialogueContentIndex = -1;
                this.StartEventIndex = -1;
                this.EndEventIndex = -1;
                this.ChoiceIndex = -1;
                this.StartEvents = this.dialogueSO.GetEvents(true).ToList();
                this.EndEvents = this.dialogueSO.GetEvents(false).ToList();
                
                if (this.dialogueSO.TryGetChoices(out var choices))
                {
                    this.Choices = choices.ToList();
                    this.IsLastDialogueNode = false;
                }
                else
                {
                    this.IsLastDialogueNode = true;
                }
            }

            /// <summary>
            /// 当前对话文件状态
            /// </summary>
            public DialogueViewState DialogueViewState
            {
                get
                {
                    if (this.dialogueSO == null)
                        return DialogueViewState.Null;
                    else if (this.StartEventIndex + 1 < this.StartEvents.Count)
                        return DialogueViewState.AwaitNextStartEvent;
                    else if (this.DialogueContentIndex + 1 < this.dialogueSO.Contents.Count)
                        return DialogueViewState.AwaitNextSentence;
                    else if (this.ChoiceIndex < 0)
                        return DialogueViewState.AwaitChoice;
                    else if (this.EndEventIndex + 1 < this.EndEvents.Count)
                        return DialogueViewState.AwaitNextEndEvent;
                    else if (this.IsLastDialogueNode)
                        return DialogueViewState.End;
                    return DialogueViewState.AwaitNextNode;
                }
            }

            /// <summary>
            /// 获取下一句对话数据
            /// </summary>
            /// <returns></returns>
            public SDSDialogueContentData GetNextDialogueContentData()
            {
                if (this.dialogueSO.TryGetNextDialogueContent(this.DialogueContentIndex, out var next))
                {
                    this.DialogueContentIndex++;
                    return next;
                }
                Debug.LogError($"GetNextDialogueContentData: content index out of range");
                return null;
            }

            /// <summary>
            /// 获取下一个事件数据
            /// </summary>
            /// <param name="getStartEvent">开始事件 or 结束事件</param>
            /// <returns></returns>
            public SDSDialogueEventData GetNextEventData(bool getStartEvent)
            {
                var events = getStartEvent ? this.StartEvents : this.EndEvents;
                int currentIndex = getStartEvent ? this.StartEventIndex : this.EndEventIndex;
                if (currentIndex + 1 >= events.Count)
                    return null;

                if (getStartEvent)
                    this.StartEventIndex++;
                else
                    this.EndEventIndex++;
                
                return events[currentIndex + 1];
            }

            /// <summary>
            /// 获取对话选项，单选节点也会返回一个选项
            /// </summary>
            /// <returns></returns>
            public List<SDSDialogueChoiceData> GetChoices()
            {
                if (this.dialogueSO == null)
                    return null;
                return this.dialogueSO.TryGetChoices(out var choices) ? choices.ToList() : null;
            }

            /// <summary>
            /// 选择对话选项
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public bool TryChoose(int index)
            {
                if (index < 0 || index >= this.Choices.Count)
                    return false;
                this.ChoiceIndex = index;
                return true;
            }

            /// <summary>
            /// 移动到下一个对话节点
            /// </summary>
            /// <returns></returns>
            public bool TryMove2NextDialogueNode()
            {
                if (this.IsLastDialogueNode)
                    return false;

                this.dialogueSO = this.Choices[this.ChoiceIndex].NextDialogue;
                this.ResetDialogueNodeData();
                
                return true;
            }
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
