using DG.Tweening;
using SFramework.Adapter;
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
            Debug.Log("in MainView OnAwake");
        }

        protected override async void OnShow()
        {
            Debug.Log("in MainView OnShow");
            this.BG_RawImage.DOColorByAdapter(new Color(1f, 1f, 1f, 0f), 2f).From().SetAutoKill();
            this.BG_RawImage.rectTransform.DOScale(Vector3.one * 2, 2f).From().SetEase(Ease.InExpo).SetAutoKill();
            this.BG_RawImage.rectTransform.DOAnchorPosByAdapter(new Vector2(600, -500), 2f).From().SetEase(Ease.InExpo).SetAutoKill();
            var bgmSource = await GameManager.Instance.AudioManager.PlayBGMAsync("TheLastCity");
            bgmSource.DOFadeByAdapter(0f,1.5f).From().SetAutoKill();
        }
    }
}
