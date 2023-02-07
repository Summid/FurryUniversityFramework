using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    [UIView("TestUIPrefab2", EnumUIType.Page)]
    public partial class TestUIPrefab2 : UIViewBase, IUIUpdator
    {
        public void OnUpdate()
        {
            //Debug.Log("OnUpdate");
        }

        protected override void OnAwake()
        {
            this.HideButton_Button.onClick.AddListener(this.Hide);
            this.PlayBGMButton_Button.onClick.AddListener(() =>
            {
                GameManager.Instance.AudioManager.PlayBGMAsync("Whenlovegoeswithflow").Forget();
            });
        }
    }
}
