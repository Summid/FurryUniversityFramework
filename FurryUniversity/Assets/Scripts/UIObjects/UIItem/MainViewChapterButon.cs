using SFramework.Core.UI.External.UnlimitedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class MainViewChapterButon : UIItemBase, IUIPool<string>, IUIScrollerCell<string>
    {
        [UIFieldInit("MainViewChapterButton")]
        public TMPro.TextMeshProUGUI MainViewChapterButton_TextMeshProUGUI;
        [UIFieldInit("MainViewChapterButton")]
        public UnityEngine.UI.Button MainViewChapterButton_Button;

        private string data = string.Empty;

        protected override void OnAwake()
        {
            this.MainViewChapterButton_Button.onClick.AddListener(() =>
            {
                Debug.Log($"clicked {this.data} chapter button");
            });
        }

        public void PoolSetData(string data)
        {
            this.data = data;
            this.MainViewChapterButton_TextMeshProUGUI.text = data;
        }

        public void ScrollerSetData(string data)
        {
            Debug.Log($"scroller set data {data}");
            this.data = data;
            this.MainViewChapterButton_TextMeshProUGUI.text = data;
        }
    }
}
