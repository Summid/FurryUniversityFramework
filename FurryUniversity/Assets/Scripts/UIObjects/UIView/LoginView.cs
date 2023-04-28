using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using SFramework.Adapter;
using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SFramework.Core.UI
{
    [UIView("LoginView", EnumUIType.Page)]
    public partial class LoginView : UIViewBase
    {
        private List<TweenerCore<Color, Color, ColorOptions>> tweenerCores = new List<TweenerCore<Color, Color, ColorOptions>>();
        private readonly float sliderSpeed = 0.5f;
        private TweenerCore<Color, Color, ColorOptions> infoTextTweener;
        private TweenerCore<float, float, FloatOptions> sliderTweener;

        protected override void OnAwake()
        {
            this.BG_Button.onClick.AddListener(() =>
            {
                GameManager.Instance.UIManager.ShowUIAsync<MainView>().Forget();
            });
        }

        protected override async void OnShow()
        {
            this.ShowLoadingEffect();
            await STask.Delay(2000);
            this.ShowLoadedEffect();
        }

        private void ShowLoadingEffect()
        {
            this.BG_Button.enabled = false;
            for (int i = 0; i < this.LoadImages.Count; ++i)
            {
                if (this.tweenerCores.Count < this.LoadImages.Count)
                {
                    var newTweenCore = this.LoadImages[i].DOFadeByAdapter(0f, 1f);
                    this.tweenerCores.Add(newTweenCore);
                }

                this.tweenerCores[i].SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetDelay(0.3f * i);
            }
        }

        private void ShowLoadedEffect()
        {
            this.sliderTweener?.Kill();
            var sliderTween = this.ProgressSlider_Slider.DOValueByAdapter(1f, (1 - this.ProgressSlider_Slider.value) / this.sliderSpeed);
            sliderTween.onComplete = () =>
            {
                this.ProgressSlider_Slider.gameObject.SetActive(false);
                this.TipsText.text = "点击任意键继续";
                this.tweenerCores.ForEach(t => t.Kill());
                this.LoadImages.ForEach(img => img.gameObject.SetActive(false));
            };
            this.TipsText.DOFadeByAdapter(0f, 2f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
            this.BG_Button.enabled = true;
        }

        protected override void OnHide()
        {
            this.infoTextTweener?.Kill();
        }

    }
}
