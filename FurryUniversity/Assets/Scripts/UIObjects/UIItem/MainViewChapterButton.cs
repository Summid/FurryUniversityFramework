using SDS.ScriptableObjects;
using SFramework.Core.UI.External.UnlimitedScroller;
using SFramework.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class MainViewChapterButton : UIItemBase, IUIPool<SDSDialogueContainerSO>
    {
        [UIFieldInit("MainViewChapterButton")]
        public TMPro.TextMeshProUGUI MainViewChapterButton_TextMeshProUGUI;
        [UIFieldInit("MainViewChapterButton")]
        public UnityEngine.UI.Button MainViewChapterButton_Button;

        public SDSDialogueContainerSO dialogue;

        protected override void OnAwake()
        {
            this.MainViewChapterButton_Button.onClick.AddListener(() =>
            {
                MessageBoxView.ShowAsync($"进入章节 {this.dialogue.FileName}", MessageBoxView.MessageFlag.Both,
                    (flag) =>
                    {
                        if (flag == MessageBoxView.MessageFlag.Confirm)
                            Debug.Log("click message box view confirm button");
                        else if(flag == MessageBoxView.MessageFlag.Cancel)
                            Debug.Log("click message box view cancel button");
                    }).Forget();
            });
        }

        public void PoolSetData(SDSDialogueContainerSO data)
        {
            this.dialogue = data;

        }
    }
}
