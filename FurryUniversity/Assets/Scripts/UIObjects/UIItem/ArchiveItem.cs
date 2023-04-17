using SFramework.Core.UI.External.UnlimitedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class ArchiveItem : UIItemBase, IUIScrollerCell<int>
    {
        [UIFieldInit("TinyImage")] public UnityEngine.UI.Image TinyImage_Image;
        [UIFieldInit("TinyImage")] public UnityEngine.UI.Button TinyImage_Button;
        [UIFieldInit("ShowText")] public TMPro.TextMeshProUGUI ShowText;


        public void ScrollerSetData(int data)
        {
            throw new System.NotImplementedException();
        }
    }
}