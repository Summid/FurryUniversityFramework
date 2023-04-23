using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    [UIView("ButtonsFloatTipView", EnumUIType.Page)]
    public partial class ButtonsFloatTipView : UIViewBase
    {
        public class ButtonsData
        {
            public int Index;
            public string ShowText;
            public bool IsLast;
            public Action<int> OnClick;
        }

        public static void Pop(Vector3 screenPos, TextAnchor alignment)
        {
            
        }
    }
}
