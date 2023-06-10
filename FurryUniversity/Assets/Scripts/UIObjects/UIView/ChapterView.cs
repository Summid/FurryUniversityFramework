using SDS;
using SDS.ScriptableObjects;
using SFramework.Core.GameManagers;
using SFramework.Core.UI.External.UnlimitedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    [UIView("ChapterView", EnumUIType.Window)]
    public partial class ChapterView : UIViewBase
    {
        private SDSDialogue dialogue => GameManager.Instance.DialogueSystem; 
        
        protected override void OnShow()
        {
            this.RefreshView();
        }

        private void RefreshView()
        {
            this.Content.UpdateScrollCells<SDSDialogueContainerSO, ChapterItem>(this, this.dialogue.GetAllDialogues());
        }
    }
}
