using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using SDS;
using SFramework.Adapter;
using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    [UIView("MainView", EnumUIType.Page)]
    public partial class MainView : UIViewBase
    {
        private TweenerCore<Color, Color, ColorOptions> bgAlphaTweener;
        private TweenerCore<Vector3, Vector3, VectorOptions> bgScaleTweener;
        private TweenerCore<Vector2, Vector2, VectorOptions> bgPositionTweener;
        private TweenerCore<float, float, FloatOptions> bgmVolumeTweener;
        private TweenerCore<Vector2, Vector2, VectorOptions> buttonPanelPositionTweener;

        private SDSDialogue dialogueSystem;

        protected override void OnAwake()
        {
            Debug.Log("in MainView OnAwake");

            this.SelectChapterButton_Button.onClick.AddListener(this.OnClickSelectChapter);

            this.dialogueSystem = GameManager.Instance.DialogueSystem;
        }

        protected override void OnShow()
        {
            Debug.Log("in MainView OnShow");
            this.ShowEnterViewEffect().Forget();

        }

        private async STaskVoid ShowEnterViewEffect()
        {
            this.bgAlphaTweener?.Kill();
            this.bgScaleTweener?.Kill();
            this.bgPositionTweener?.Kill();
            this.bgmVolumeTweener?.Kill();

            this.bgAlphaTweener = this.BG_RawImage.DOColorByAdapter(new Color(1f, 1f, 1f, 0f), 2f).From();

            this.bgScaleTweener = this.BG_RawImage.rectTransform.DOScale(Vector3.one * 2, 2f).From().SetEase(Ease.InExpo);

            this.bgPositionTweener = this.BG_RawImage.rectTransform.DOAnchorPosByAdapter(new Vector2(600, -500), 2f).From().SetEase(Ease.InExpo);
            this.bgPositionTweener.onComplete += this.ShowMainButtonsPanel;

            var bgmSource = await GameManager.Instance.AudioManager.PlayBGMAsync("TheLastCity");
            this.bgmVolumeTweener = bgmSource.DOFadeByAdapter(0f,1.5f).From();
        }

        private void ShowMainButtonsPanel()
        {
            this.buttonPanelPositionTweener?.Kill();
            var rectTrans = this.MainButtons.GetComponent<RectTransform>();
            this.buttonPanelPositionTweener = rectTrans.DOAnchorPosXByAdapter(0f, 0.5f);
        }

        private void OnClickSelectChapter()
        {
            foreach (var dialogue in this.dialogueSystem.IncludedContainers)
            {
                Debug.Log($"current included dialogue: {dialogue.FileName}");
            }
        }

        protected override void OnHide()
        {
            this.bgAlphaTweener?.Kill();
            this.bgScaleTweener?.Kill();
            this.bgPositionTweener?.Kill();
            this.bgmVolumeTweener?.Kill();
            this.buttonPanelPositionTweener?.Kill();
        }
    }
}
