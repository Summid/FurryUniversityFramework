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
            this.PlayBGMButton_Button.onClick.AddListener(() =>
            {
                GameManager.Instance.AudioManager.PlayBGMAsync("Idonotdeserve").Forget();
            });

            this.PauseBGMButton_Button.onClick.AddListener(() =>
            {
                GameManager.Instance.AudioManager.PauseBGM().Forget();
            });

            int index = 0;
            foreach (var button in this.PlaySFXButtons_Button)
            {

                string sfxName = string.Empty;
                if (index == 0)
                    sfxName = "ui_effect_5";
                else if (index == 1)
                    sfxName = "ui_effect_94";
                else if (index == 2)
                    sfxName = "ui_effect_45";

                button.onClick.AddListener(() => { GameManager.Instance.AudioManager.PlaySoundAsync(sfxName).Forget(); });

                index++;
            }

            this.DisposeSFXBundlesButton_Button.onClick.AddListener(() => { GameManager.Instance.AudioManager.DisposeSFXGroupBunudles(); });
        }

        protected override void OnShow()
        {
            Debug.Log("in MainView OnShow");
        }
    }
}
