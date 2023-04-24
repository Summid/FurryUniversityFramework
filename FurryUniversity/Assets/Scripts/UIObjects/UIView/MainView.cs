using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using SDS;
using SDS.ScriptableObjects;
using SFramework.Adapter;
using SFramework.Core.GameManagers;
using SFramework.Core.UI.External.UnlimitedScroller;
using SFramework.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private Vector2 mainButtonsAwakeAnchoredPosition;

        protected override void OnAwake()
        {
            Debug.Log("in MainView OnAwake");

            this.SelectChapterButton_Button.onClick.AddListener(this.OnClickSelectChapter);
            this.ChaptersBackButton_Button.onClick.AddListener(this.OnClickChapterBack);
            this.ArchiveButton_Button.onClick.AddListener(() =>
                GameManager.Instance.UIManager.ShowUIAsync<ArchiveView>().Forget());
            this.SettingsButton_Button.onClick.AddListener((() =>
                GameManager.Instance.UIManager.ShowUIAsync<SettingsView>().Forget()));

            this.dialogueSystem = GameManager.Instance.DialogueSystem;
            this.mainButtonsAwakeAnchoredPosition = this.MainButtons.GetComponent<RectTransform>().anchoredPosition;
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
            if(bgmSource != null)
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
            this.ChapterButtonsPool_UIItemPool.UpdateList<SDSDialogueContainerSO, MainViewChapterButton>(this.dialogueSystem.GetAllDialogues());
            this.ChapterButtonsPool_UIItemPool.gameObject.SetActive(true);
            this.MainButtons.gameObject.SetActive(false);
        }

        private void OnClickChapterBack()
        {
            this.ChapterButtonsPool_UIItemPool.gameObject.SetActive(false);
            this.MainButtons.gameObject.SetActive(true);
        }

        protected override void OnHide()
        {
            this.bgAlphaTweener?.Kill();
            this.bgScaleTweener?.Kill();
            this.bgPositionTweener?.Kill();
            this.bgmVolumeTweener?.Kill();
            this.buttonPanelPositionTweener?.Kill();
            GameManager.Instance.AudioManager.PauseBGM().Forget();
            this.MainButtons.GetComponent<RectTransform>().anchoredPosition = this.mainButtonsAwakeAnchoredPosition;
            this.OnClickChapterBack();
        }
    }
}
