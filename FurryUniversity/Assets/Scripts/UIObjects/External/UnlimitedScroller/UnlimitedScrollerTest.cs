using SFramework.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI.External.UnlimitedScroller
{
    public class UnlimitedScrollerTest : MonoBehaviour
    {
        public GameObject cell;
        public int number;
        public IUnlimitedScroller scroller;

        // Start is called before the first frame update
        async void Start()
        {
            this.scroller = this.GetComponent<IUnlimitedScroller>();
            await STask.NextFrame();
            this.scroller.Generate(this.cell, this.number, null);
        }
    }
}