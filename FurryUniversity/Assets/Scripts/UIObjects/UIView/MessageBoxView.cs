using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    [UIView("MessageBoxView", EnumUIType.Window)]
    public partial class MessageBoxView : UIViewBase
    {
        public enum MessageFlag { Confirm = 1, Cancel = 2, Both = 3 }

        private Action<MessageFlag, MessageBoxView> onClickFlagButton;

        protected override void OnAwake()
        {
            this.CloseButton_Button.onClick.AddListener(this.Hide);
        }

        public void SetData(string message, MessageFlag messageFlag = MessageFlag.Confirm,
            Action<MessageFlag, MessageBoxView> onClickFlagButton = null, string confirmText = null, string cancelText = null)
        {
            this.MessageText.text = message;

            if (!string.IsNullOrEmpty(confirmText))
            {
                this.MessageText.text = confirmText;
            }

            if (!string.IsNullOrEmpty(cancelText))
            {
                this.MessageText.text = cancelText;
            }

            this.ConfirmButton_Button.gameObject.SetActive((messageFlag & MessageFlag.Confirm) == MessageFlag.Confirm);
            this.CancelButton_Button.gameObject.SetActive((messageFlag & MessageFlag.Cancel) == MessageFlag.Cancel);
            this.onClickFlagButton = onClickFlagButton;

            this.ConfirmButton_Button.onClick.AddListener(() =>
            {
                this.onClickFlagButton?.Invoke(MessageFlag.Confirm, this);
            });
            this.CancelButton_Button.onClick.AddListener(() =>
            {
                this.onClickFlagButton?.Invoke(MessageFlag.Cancel, this);
            });
        }

        #region ExternalUse

        public static async STask<MessageBoxView> ShowAsync(string message, MessageFlag messageFlag = MessageFlag.Confirm, 
            Action<MessageFlag, MessageBoxView> onClickFlagButton = null, string confirmText = null, string cancelText = null)
        {
            var view = await GameManager.Instance.UIManager.ShowUIAsync<MessageBoxView>();

            view.SetData(message, messageFlag, onClickFlagButton, confirmText, cancelText);

            return view;
        }

        #endregion
    }
}
