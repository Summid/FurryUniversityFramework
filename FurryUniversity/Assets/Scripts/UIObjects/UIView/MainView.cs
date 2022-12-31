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
            this.ShowPageViewButton_Button.onClick.AddListener(() => { GameManager.Instance.UIManager.ShowUIAsync<TestUIPrefab2>().Forget(); });
        }
    }
}
