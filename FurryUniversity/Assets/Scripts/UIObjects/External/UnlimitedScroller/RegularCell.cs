using System;
using UnityEngine;

namespace SFramework.Core.UI.External.UnlimitedScroller
{
    public class RegularCell : MonoBehaviour, ICell
    {
        public event Action<ScrollerPanelSide> OnVisible;
        public event Action<ScrollerPanelSide> OnInvisible;

        public void OnBecomeInvisible(ScrollerPanelSide side)
        {
            this.OnInvisible?.Invoke(side);
        }

        public void OnBecomeVisible(ScrollerPanelSide side)
        {
            this.OnVisible?.Invoke(side);
        }
    }
}