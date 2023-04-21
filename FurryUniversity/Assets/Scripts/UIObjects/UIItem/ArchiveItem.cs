using SFramework.Core.UI.External.UnlimitedScroller;
using SFramework.Utilities.Archive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class ArchiveItem : UIItemBase, IUIScrollerCell<ArchiveObject>
    {
        [UIFieldInit("TinyImage")] public UnityEngine.UI.Image TinyImage_Image;
        [UIFieldInit("TinyImage")] public UnityEngine.UI.Button TinyImage_Button;
        [UIFieldInit("ShowText")] public TMPro.TextMeshProUGUI ShowText;
        

        public void ScrollerSetData(ArchiveObject data)
        {
            this.ShowText.text = $"Archive {data.ArchiveIndex}";
        }

        protected override void OnHide()
        {
            Debug.Log("Archive Item onhide");
        }

        protected override void OnDispose()
        {
            Debug.Log("Archive Item ondispose");
            
        }
    }
}