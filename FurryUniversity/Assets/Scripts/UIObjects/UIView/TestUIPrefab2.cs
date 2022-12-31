using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    [UIView("TestUIPrefab2", EnumUIType.Page)]
    public partial class TestUIPrefab2 : UIViewBase
    {
        protected override void OnAwake()
        {
            this.HideButton_Button.onClick.AddListener(this.Hide);
        }
    }
}
