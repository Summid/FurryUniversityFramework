using SFramework.Core.GameManagers;
using SFramework.Core.UI.External.UnlimitedScroller;
using SFramework.Utilities.Archive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class ArchiveItem : UIItemBase, IUIScrollerCell<ArchiveObject>
    {
        [UIFieldInit("TinyImage")]
        public UnityEngine.UI.Image TinyImage;
        [UIFieldInit("ShowText")]
        public TMPro.TextMeshProUGUI ShowText;
        [UIFieldInit("FloatTipInvoker")]
        public SFramework.Core.UI.FloatTipInvoker FloatTipInvoker_FloatTipInvoker;
        [UIFieldInit("FloatTipInvoker")]
        public SFramework.Core.UI.External.SimpleRaycast FloatTipInvoker_SimpleRaycast;


        private ArchiveObject archiveObject;
        private List<object> buttonDatas = new List<object>();
        
        public void ScrollerSetData(ArchiveObject data)
        {
            this.archiveObject = data;
            this.ShowText.text = $"Archive {data.ArchiveIndex}";

            this.buttonDatas.Clear();
            for (int i = 0; i < 2; ++i)
            {
                var buttonData = new ButtonsFloatTipView.ButtonData();
                buttonData.Index = i;
                buttonData.ShowText = i == 0 ? "保存" : "读取";
                buttonData.OnClick = this.OnClickFloatTipItem;

                if (i == 0)
                {
                    buttonData.Interactable = GameManager.Instance.UIManager.GetShowingUI<DialogueView>() != null;
                }
                else
                {
                    var dialogueView = GameManager.Instance.UIManager.GetShowingUI<DialogueView>();
                    var mainView = GameManager.Instance.UIManager.GetShowingUI<MainView>();
                    buttonData.Interactable = dialogueView != null || mainView != null;
                }

                this.buttonDatas.Add(buttonData);
            }

            this.FloatTipInvoker_FloatTipInvoker.Parameters = this.buttonDatas;
        }

        private void OnClickFloatTipItem(int index)
        {
            switch (index)
            {
                case 0:
                    Debug.Log("Save");
                    break;
                case 1:
                    Debug.Log("Load");
                    break;
            }
        }
    }
}