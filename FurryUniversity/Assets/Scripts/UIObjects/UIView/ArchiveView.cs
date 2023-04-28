using SFramework.Core.UI.External.UnlimitedScroller;
using SFramework.Threading.Tasks;
using SFramework.Utilities.Archive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    [UIView("ArchiveView", EnumUIType.Window)]
    public partial class ArchiveView : UIViewBase, IUIPrepareShow
    {
        public bool NeedPrepare = true;
        protected override void OnShow()
        {
            this.RefreshArchiveList();
        }

        private void RefreshArchiveList()
        {
            if (!SaveMaster.IsInit)
                return;

            var archiveList = SaveMaster.Archive.ArchiveObjects;
            this.Content.UpdateScrollCells<ArchiveObject, ArchiveItem>(this, archiveList);
        }

        public async STask OnPrepareShow()
        {
            if(this.NeedPrepare)
                await STask.Delay(1500);
            this.NeedPrepare = false;
        }
    }
}
