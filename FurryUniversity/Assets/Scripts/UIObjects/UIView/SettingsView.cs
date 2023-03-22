using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    [UIView("SettingsView", EnumUIType.Window)]
    public partial class SettingsView : UIViewBase
    {
        [Flags]
        public enum PanelType
        {
            GameEnv = 1,
            Audio = 2,
        }

        private PanelType currentPanel
        {
            get
            {
                if (this.EnvToggle.isOn)
                    return PanelType.GameEnv;
                return PanelType.Audio;
            }
        }
        
        protected override void OnAwake()
        {
            this.EnvToggle.onValueChanged.AddListener(_ => this.OnClickToggle());
            this.AudioToggle.onValueChanged.AddListener(_ => this.OnClickToggle());
        }

        protected override void OnShow()
        {
            this.OnClickToggle();
        }

        private void OnClickToggle()
        {
            this.EnvPanel.gameObject.SetActive((this.currentPanel & PanelType.GameEnv) == PanelType.GameEnv);
            this.AudioPanel.gameObject.SetActive((this.currentPanel & PanelType.Audio) == PanelType.Audio);
        }
    }
}
