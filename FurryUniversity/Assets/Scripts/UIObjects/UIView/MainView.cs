using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    [UIView("MainView", EnumUIType.Page)]
    public partial class MainView : UIViewBase
    {
        protected override void OnAwake()
        {
        }

        protected override void OnShow()
        {
            Debug.Log("in MainView OnShow");
        }
    }
}
