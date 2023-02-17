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
    public partial class LoginView : UIViewBase, IProgress<float>
    {
        private List<TweenerCore<Color, Color, ColorOptions>> tweenerCores = new List<TweenerCore<Color, Color, ColorOptions>>();
        private MainView mainView;
        private readonly float sliderSpeed = 0.5f;
        private TweenerCore<Color, Color, ColorOptions> infoTextTweener;
        private TweenerCore<float, float, FloatOptions> sliderTweener;

        protected override void OnAwake()
        {
            this.BG_Button.onClick.AddListener(() =>
            {
                if (this.mainView == null)
                    return;
                this.mainView.Show();
            });
        }

        protected override async void OnShow()
        {
            this.ShowLoadingEffect();

            this.mainView = await GameManager.Instance.UIManager.ShowUIAsync<MainView>(false, this);
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
            var sliderTween = this.ProgressSlider.DOValueByAdapter(1f, (1 - this.ProgressSlider.value) / this.sliderSpeed);
            sliderTween.onComplete = () =>
            {
                this.ProgressSlider.gameObject.SetActive(false);
                this.TipsText.text = "点击任意键继续";
                this.tweenerCores.ForEach(t => t.Kill());
                this.LoadImages.ForEach(img => img.gameObject.SetActive(false));
            };
            this.TipsText.DOFadeByAdapter(0f, 2f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
            this.BG_Button.enabled = true;
        }

        public void Report(float value)
        {
            this.sliderTweener?.Kill();
            this.ProgressSlider.DOValueByAdapter(value, (value - this.ProgressSlider.value) / this.sliderSpeed);
        }

        protected override void OnHide()
        {
            this.infoTextTweener?.Kill();
        }

    }
}
