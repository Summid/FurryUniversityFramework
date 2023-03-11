using System;
using UnityEngine;

namespace SFramework.Core.UI.External.UnlimitedScroller
{
    public interface IUnlimitedScroller
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// 是否已初始化并生成cell
        /// </summary>
        bool Generated { get; }

        /// <summary>
        /// 总行数
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// 可见的首行
        /// </summary>
        int FirstRow { get; }

        /// <summary>
        /// 可见的最后一行
        /// </summary>
        int LastRow { get; }

        /// <summary>
        /// 可见的首列
        /// </summary>
        int FirstColumn { get; }

        /// <summary>
        /// 可见的最后一列
        /// </summary>
        int LastColumn { get; }

        /// <summary>
        /// 容器高度
        /// </summary>
        float ContentHeight { get; }

        /// <summary>
        /// 容器宽度
        /// </summary>
        float ContentWidth { get; }

        /// <summary>
        /// 视口高度
        /// </summary>
        float ViewportHeight { get; }

        /// <summary>
        /// 视口宽度
        /// </summary>
        float ViewportWidth { get; }

        /// <summary>
        /// 每行拥有cell的真实数量，或每行有多少列
        /// </summary>
        int CellPerRow { get; }

        /// <summary>
        /// 调用该方法来初始化并生成cell
        /// </summary>
        /// <param name="newCell">the cell game object</param>
        /// <param name="newTotalCount">意图生成cell的总数量</param>
        /// <param name="onGenerate">生成cell后的回调，可在此设置cell的相关数据</param>
        void Generate(GameObject newCell, int newTotalCount, Action<int, ICell> onGenerate);

        /// <summary>
        /// 调用该方法，通过索引来定位cell
        /// </summary>
        /// <param name="index">cell的索引</param>
        /// <param name="method"></param>
        void JumpTo(uint index, JumpToMethod method);

        /// <summary>
        /// 设置新缓存容量；如果新值比当前值小，多余的缓存将被优化
        /// </summary>
        /// <param name="newSize"></param>
        void SetCacheSize(uint newSize);

        /// <summary>
        /// 清理所有缓存中的cell
        /// </summary>
        void Clear();

        /// <summary>
        /// 清理所有缓存中的cell，不会修改缓存容量
        /// </summary>
        void ClearCache();
    }
}