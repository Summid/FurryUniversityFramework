using UnityEngine;

namespace SFramework.Core.UI.External.UnlimitedScroller
{
    /// <summary>
    /// the side of the scroller panel
    /// </summary>
    public enum ScrollerPanelSide
    {
        NoSide,
        Top,
        Bottom,
        Left,
        Right
    }

    public enum Aligment
    {
        Left,
        Center,
        Right
    }

    internal struct Cell
    {
        public int index;
        public GameObject go;
    }

    internal struct Padding
    {
        public int top, bottom, left, right;
    }

    /// <summary>
    /// for jump to cell method
    /// </summary>
    public enum JumpToMethod
    {
        /// <summary>
        /// scroll until the cell is visible
        /// </summary>
        OnScreen,

        /// <summary>
        /// scroll until the cell is on the center
        /// </summary>
        Center
    }
}