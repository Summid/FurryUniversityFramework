using SDS.ScriptableObjects;
using SFramework.Core.GameManagers;
using SFramework.Core.UI.External.UnlimitedScroller;
using SFramework.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class ChapterItem : UIItemBase, IUIScrollerCell<SDSDialogueContainerSO>
    {
        [UIFieldInit("ChapterButton")]
        public UnityEngine.UI.Image ChapterButton_Image;
        [UIFieldInit("ChapterButton")]
        public UnityEngine.UI.Button ChapterButton_Button;
        [UIFieldInit("ChapterText")]
        public SFramework.Core.UI.External.TextMeshProUGUIEx ChapterText;

        private SDSDialogueContainerSO data;

        protected override void OnAwake()
        {
            this.ChapterButton_Button.onClick.AddListener(() =>
            {
                if (this.data == null) return;
                MessageBoxView.ShowAsync($"进入章节 {this.data.FileName}", MessageBoxView.MessageFlag.Both, (flag, view) =>
                {
                    this.OnClickMessageBoxButton(flag,view).Forget();
                }).Forget();
            });
        }

        public void ScrollerSetData(SDSDialogueContainerSO data)
        {
            this.data = data;
            
            this.ChapterText.text = data.FileName;
        }

        private async STaskVoid OnClickMessageBoxButton(MessageBoxView.MessageFlag flag, MessageBoxView messageBoxView) 
        {
            if (flag == MessageBoxView.MessageFlag.Confirm)
            {
                var view = await GameManager.Instance.UIManager.ShowUIAsync<DialogueView>();
                view.SetNewDialogueContainer(this.data);
            }
            messageBoxView.HideAsync().Forget();
        }
    }
}
