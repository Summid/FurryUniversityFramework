namespace SFramework.Core.UI.External.UnlimitedScroller
{
    /// <summary>
    /// 实现该接口来实现自定义Cell，用于 <see cref="IUnlimitedScroller"/>
    /// </summary>
    public interface ICell
    {
        /// <summary>
        /// 当cell可见时触发
        /// </summary>
        /// <param name="side">
        /// cell出现的方位；例如<see cref="ScrollerPanelSide.Right"/> 意味着玩家向左拖动panel时，cell出现在panel右边
        /// </param>
        void OnBecomeVisible(ScrollerPanelSide side);

        /// <summary>
        /// 当cell不可见时触发
        /// </summary>
        /// <param name="side">
        /// cell出现的方位；例如<see cref="ScrollerPanelSide.Right"/> 意味着玩家向右拖动panel时，cell从panel右边消失
        /// </param>
        void OnBecomeInvisible(ScrollerPanelSide side);

        UnityEngine.GameObject GameObject { get; }
    }
}