using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public class TestUIItem : UIItemBase
    {
        [UIFieldInit("Text")]
        public UnityEngine.UI.Text Text;
        [UIFieldInit("Button")]
        public UnityEngine.UI.Image Button_Image;
        [UIFieldInit("Button")]
        public UnityEngine.UI.Button Button_Button;

    }
}
