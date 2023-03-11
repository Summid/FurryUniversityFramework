using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SFramework.Core.UI.External.UnlimitedScroller
{
    public class VerticalUnlimitedScroller : VerticalLayoutGroup, IUnlimitedScroller
    {
        #region Properties

        /// <inheritdoc cref="IUnlimitedScroller.Initialized"/>
        public bool Initialized { get; private set; }

        /// <inheritdoc cref="IUnlimitedScroller.Generated"/>
        public bool Generated { get; private set; }

        /// <inheritdoc cref="IUnlimitedScroller.RowCount"/>
        public int RowCount => this.totalCount;

        /// <inheritdoc cref="IUnlimitedScroller.FirstRow"/>
        public int FirstRow
        {
            get
            {
                //锚点为(0,1)，若contentTrans高度超过parentTrans，只计算超出部分；若没超过，则肯定为0
                int row = (int)((this.contentTrans.anchoredPosition.y - this.offsetPadding.top) / (this.cellY + this.spacingY));
                return Mathf.Clamp(row, 0, this.RowCount - 1);
            }
        }

        /// <inheritdoc cref="IUnlimitedScroller.LastRow"/>
        public int LastRow
        {
            get
            {
                //锚点为(0,1)，若contentTrans高度超过parentTrans，只计算viewport高度加超出部分
                int row = (int)((this.contentTrans.anchoredPosition.y + this.ViewportHeight - this.offsetPadding.top) / (this.cellY + this.spacingY));//锚点为(0,1)
                return Mathf.Clamp(row, 0, this.RowCount - 1);
            }
        }

        /// <inheritdoc cref="IUnlimitedScroller.FirstColumn"/>
        /// 永远只有1列
        public int FirstColumn => 0;

        /// <inheritdoc cref="IUnlimitedScroller.LastColumn"/>
        /// 永远只有1列
        public int LastColumn => 0;

        /// <inheritdoc cref="IUnlimitedScroller.ContentHeight"/>
        public float ContentHeight
        {
            get => this.contentTrans.rect.height;
            private set => this.contentTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
        }

        /// <inheritdoc cref="IUnlimitedScroller.ContentWidth"/>
        public float ContentWidth
        {
            get => this.contentTrans.rect.width;
            private set => this.contentTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
        }

        /// <inheritdoc cref="IUnlimitedScroller.ViewportHeight"/>
        public float ViewportHeight => this.viewportRectTransform.rect.height;

        /// <inheritdoc cref="IUnlimitedScroller.ViewportWidth"/>
        public float ViewportWidth => this.viewportRectTransform.rect.width;

        /// <inheritdoc cref="IUnlimitedScroller.CellPerRow"/>
        public int CellPerRow => 1;

        #endregion

        #region Public Fields

        [Tooltip("Max size of cached cell")]
        public uint cacheSize;

        [Tooltip("The ScrollRect component on ScrollView")]
        public ScrollRect scrollRect;

        #endregion

        #region Private Fields

        private RectTransform contentTrans;
        private LayoutGroup layoutGroup;
        private RectTransform viewportRectTransform;

        private float cellX;
        private float cellY;
        private float spacingX;
        private float spacingY;
        private Padding offsetPadding;

        private int totalCount;
        private GameObject cellPrefab;
        private List<Cell> currentCells;

        private int currentFirstRow;
        private int currentLastRow;
        private int currentFirstCol;
        private int currentLastCol;

        private Action<int, ICell> onCellGenerate;

        private GameObject pendingDestroyGo;
        private UnlimitedScrollerLRUCache<int, GameObject> cachedCells;

        #endregion

        /// <inheritdoc cref="IUnlimitedScroller.Clear"/>
        public void Clear()
        {
            if (!this.Generated)
                return;

            this.DestroyAllCells();
            this.ClearCache();
            Destroy(pendingDestroyGo);
            this.Generated = false;

            this.ContentHeight = 0f;
            this.ContentWidth = 0f;
            this.layoutGroup.padding.top = this.offsetPadding.top;
            this.layoutGroup.padding.bottom = this.offsetPadding.bottom;
            this.layoutGroup.padding.left = this.offsetPadding.left;
            this.layoutGroup.padding.right = this.offsetPadding.right;
        }

        /// <inheritdoc cref="IUnlimitedScroller.ClearCache"/>
        public void ClearCache()
        {
            this.cachedCells.Clear();
        }

        /// <inheritdoc cref="IUnlimitedScroller.Generate(GameObject, int, Action{int, ICell})"/>
        public void Generate(GameObject newCell, int newTotalCount, Action<int, ICell> onGenerate)
        {
            if (this.Generated)
                return;

            if (!this.Initialized)
                this.Initialize();

            this.cellPrefab = newCell;
            this.totalCount = newTotalCount;
            this.onCellGenerate = onGenerate;
            this.InitParams();
            this.Generated = true;

            if (this.totalCount <= 0)
                return;
            this.GenerateAllCells();
        }

        /// <inheritdoc cref="IUnlimitedScroller.JumpTo(uint, JumpToMethod)"/>
        public void JumpTo(uint index, JumpToMethod method)
        {
            if (index >= this.totalCount)
                return;

            var cellRowCount = index / this.CellPerRow;//在第几行
            float verticalPosition;
            switch (method)
            {
                case JumpToMethod.OnScreen:
                    if (cellRowCount >= this.FirstRow && cellRowCount <= this.LastRow)
                        return;


                    if (cellRowCount > this.LastRow)
                    {
                        //在viewport下方
                        verticalPosition =
                            (this.offsetPadding.bottom +
                            (this.RowCount - cellRowCount - 1) * this.cellY +
                            (this.RowCount - cellRowCount - 1) * this.spacingY) /
                            (this.ContentHeight - this.ViewportHeight); //除以content与viewport的高度差delta，而不是content的高度，因为scroll的移动距离就是delta的高度
                    }
                    else
                    {
                        //在viewport上方
                        verticalPosition =
                            (this.offsetPadding.bottom +
                            (this.RowCount - cellRowCount) * this.cellY +
                            (this.RowCount - cellRowCount - 1) * this.spacingY - this.ViewportHeight / //同上，因为scroll的移动距离是delta的高度，这里cell放在viewport最顶上，因此计算出的坐标需减去viewport的高度后再计算比值
                            (this.ContentHeight - this.ViewportHeight));
                    }

                    if(this.ContentHeight > this.ViewportHeight && !(cellRowCount >= this.FirstRow && cellRowCount <= this.LastRow))
                    {
                        this.scrollRect.verticalNormalizedPosition = verticalPosition;//[0,1]，0为最底处
                    }
                    break;
                case JumpToMethod.Center:
                    verticalPosition =
                        (this.offsetPadding.bottom +
                        (this.RowCount - cellRowCount - 0.5f) * this.cellY +
                        (this.RowCount - cellRowCount - 1) * this.spacingY - this.ViewportHeight / 2f) /
                        (this.ContentHeight - this.ViewportHeight);

                    if (this.ContentHeight > this.ViewportHeight)
                    {
                        this.scrollRect.verticalNormalizedPosition = verticalPosition;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }

        /// <inheritdoc cref="IUnlimitedScroller.SetCacheSize(uint)"/>
        public void SetCacheSize(uint newSize)
        {
            this.cachedCells.SetCapacity(newSize);
        }

        private void Initialize()
        {
            this.layoutGroup = this.GetComponent<LayoutGroup>();
            this.viewportRectTransform = this.scrollRect.viewport;
            this.contentTrans = this.GetComponent<RectTransform>();

            this.offsetPadding = new Padding
            {
                top = this.layoutGroup.padding.top,
                bottom = this.layoutGroup.padding.bottom,
                left = this.layoutGroup.padding.left,
                right = this.layoutGroup.padding.right
            };

            this.scrollRect.onValueChanged.AddListener(this.OnScroll);

            this.Initialized = true;
        }

        private void InitParams()
        {
            Rect rect = this.cellPrefab.GetComponent<RectTransform>().rect;
            this.cellX = rect.width;
            this.cellY = rect.height;
            this.spacingX = 0f;//vertical scroller固定为0
            this.spacingY = ((HorizontalOrVerticalLayoutGroup)this.layoutGroup).spacing;

            this.currentCells = new List<Cell>();
            this.contentTrans.anchoredPosition = Vector2.zero;
            this.contentTrans.anchorMin = Vector2.up;
            this.contentTrans.anchorMax = Vector2.up;
            this.ContentHeight = this.cellY * this.RowCount + this.spacingY * (this.RowCount - 1) + this.offsetPadding.top + this.offsetPadding.bottom;
            this.ContentWidth = this.cellX * this.CellPerRow + this.spacingX * (this.CellPerRow - 1) + this.offsetPadding.left + this.offsetPadding.right;

            this.pendingDestroyGo = new GameObject("[Cache Node]");
            this.pendingDestroyGo.transform.SetParent(this.transform);
            this.pendingDestroyGo.SetActive(false);

            this.cachedCells = new UnlimitedScrollerLRUCache<int, GameObject>((_, go) => Destroy(go), this.cacheSize);
        }

        private int GetCellIndex(int row, int column)
        {
            return this.CellPerRow * row + column;
        }

        /// <summary>
        /// 获取第一个比index大的index，主要解决越界
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int GetFirstGreater(int index)
        {
            int start = 0;
            int end = this.currentCells.Count;
            while (start != end)
            {
                int middle = start + (end - start) / 2;
                if (this.currentCells[middle].index <= index)
                {
                    start = middle + 1;
                }
                else
                {
                    end = middle;
                }
            }

            return start;
        }

        private void GenerateCell(int index, ScrollerPanelSide side)
        {
            ICell iCell;
            if (this.cachedCells.TryGet(index, out var instance))
            {
                instance.transform.SetParent(this.contentTrans);
                this.cachedCells.Remove(index);

                iCell = instance.GetComponent<ICell>();
            }
            else
            {
                instance = Instantiate(this.cellPrefab, this.contentTrans);
                instance.name = this.cellPrefab.name + "_" + index;

                iCell = instance.GetComponent<ICell>();
                this.onCellGenerate?.Invoke(index, iCell);
            }

            int order = this.GetFirstGreater(index);
            instance.GetComponent<Transform>().SetSiblingIndex(order);
            Cell cell = new Cell() { go = instance, index = index };
            this.currentCells.Insert(order, cell);

            iCell.OnBecomeVisible(side);
        }

        private void DestroyCell(int index, ScrollerPanelSide side)
        {
            int order = this.GetFirstGreater(index - 1);
            Cell cell = this.currentCells[order];
            this.currentCells.RemoveAt(order);
            cell.go.GetComponent<ICell>().OnBecomeInvisible(side);
            cell.go.transform.SetParent(this.pendingDestroyGo.transform);
            this.cachedCells.Add(index, cell.go);
        }

        private void DestroyAllCells()
        {
            int total = this.currentCells.Count;
            for (int i = 0; i < total; i++)
            {
                Cell cell = this.currentCells[0];
                this.currentCells.RemoveAt(0);
                cell.go.GetComponent<ICell>().OnBecomeInvisible(ScrollerPanelSide.NoSide);
                Destroy(cell.go);
            }
        }

        private void GenerateAllCells()
        {
            this.currentFirstCol = this.FirstColumn;
            this.currentLastCol = this.LastColumn;
            this.currentFirstRow = this.FirstRow;
            this.currentLastRow = this.LastRow;

            //由于contentTrans的宽高是一开始就确定好的，超过viewport之外的cell会被放入池中，即其gameObject会被放在cache node下，
            //这里需要控制layoutGroup的padding数值，让需要正常显示的cell处于viewport中
            this.layoutGroup.padding.left = this.offsetPadding.left + (this.currentFirstCol == 0
                ? 0
                : (int)(this.currentFirstCol * this.cellX + (this.currentFirstCol - 1) * this.spacingX));
            this.layoutGroup.padding.right = this.offsetPadding.right + (int)((this.CellPerRow - this.LastColumn - 1) * (this.cellX + this.spacingX));
            this.layoutGroup.padding.top = this.offsetPadding.top + (this.currentFirstRow == 0
                ? 0
                : (int)(this.currentFirstRow * this.cellY + (this.currentFirstRow - 1) * this.spacingY));
            this.layoutGroup.padding.bottom = this.offsetPadding.bottom + (int)((this.RowCount - this.LastRow - 1) * (this.cellY + this.spacingY));
            
            for (int r = this.currentFirstRow; r <= this.currentLastRow; ++r)
            {
                for (int c = this.currentFirstCol; c <= this.currentLastCol; ++c)
                {
                    var index = this.GetCellIndex(r, c);
                    if (index >= this.totalCount)
                        continue;
                    this.GenerateCell(index, ScrollerPanelSide.NoSide);
                }
            }
        }

        private void GenerateRow(int row, bool onTop)
        {
            int firstCol = this.currentFirstCol;
            int lastCol = this.currentLastCol;

            int indexEnd = this.GetCellIndex(row, lastCol);
            indexEnd = indexEnd >= this.totalCount ? this.totalCount - 1 : indexEnd;
            for (int i = this.GetCellIndex(row, firstCol); i <= indexEnd; ++i)
            {
                this.GenerateCell(i, onTop ? ScrollerPanelSide.Top : ScrollerPanelSide.Bottom);
            }

            //控制需cell处于viewport中央
            if (onTop)
            {
                this.layoutGroup.padding.top -= (int)(this.cellY + this.spacingY);
            }
            else
            {
                this.layoutGroup.padding.bottom -= (int)(this.cellY + this.spacingY);
            }
        }

        private void GenerateCol(int col, bool onLeft)
        {
            int firstRow = this.currentFirstRow;
            int lastRow = this.currentLastRow;

            for(int i = firstRow; i <= lastRow; ++i)
            {
                int index = this.GetCellIndex(i, col);
                if (index >= this.totalCount)
                    continue;
                this.GenerateCell(index, onLeft ? ScrollerPanelSide.Left : ScrollerPanelSide.Right);
            }

            if (onLeft)
            {
                this.layoutGroup.padding.left -= (int)(this.cellX + this.spacingX);
            }
            else
            {
                this.layoutGroup.padding.right -= (int)(this.cellX + this.spacingX);
            }
        }

        private void DestroyRow(int row, bool onTop)
        {
            int firstCol = this.currentFirstCol;
            int lastCol = this.currentLastCol;

            int indexEnd = this.GetCellIndex(row, lastCol);
            indexEnd = indexEnd >= this.totalCount ? this.totalCount - 1 : indexEnd;
            for (int i = this.GetCellIndex(row, firstCol); i <= indexEnd; ++i)
            {
                this.DestroyCell(i, onTop ? ScrollerPanelSide.Top : ScrollerPanelSide.Bottom);
            }

            if (onTop)
            {
                this.layoutGroup.padding.top += (int)(this.cellY + this.spacingY);
            }
            else
            {
                this.layoutGroup.padding.bottom += (int)(this.cellY + this.spacingY);
            }
        }

        private void DesstroyCol(int col, bool onLeft)
        {
            int firstRow = this.currentFirstRow;
            int lastRow = this.currentLastRow;

            for (int i = firstRow; i <= lastRow; ++i)
            {
                int index = this.GetCellIndex(i, col);
                if (index >= this.totalCount)
                    continue;
                this.DestroyCell(index, onLeft ? ScrollerPanelSide.Left : ScrollerPanelSide.Right);
            }

            if (onLeft)
            {
                this.layoutGroup.padding.left += (int)(this.cellX + this.spacingX);
            }
            else
            {
                this.layoutGroup.padding.right += (int)(this.cellX + this.spacingX);
            }
        }

        private void OnScroll(Vector2 position)
        {
            if (!this.Generated || this.totalCount <= 0)
                return;

            if (this.LastColumn < this.currentFirstCol || this.FirstColumn > this.currentLastCol || this.FirstRow > this.currentLastRow ||
                this.LastRow < this.currentFirstRow)
            {
                this.DestroyAllCells();
                this.GenerateAllCells();
                return;
            }

            if (this.currentFirstCol > this.FirstColumn)
            {
                //new left column
                for (int col = this.currentFirstCol - 1; col >= this.FirstColumn; --col)
                {
                    this.GenerateCol(col, true);
                }

                this.currentFirstCol = this.FirstColumn;
            }

            if (this.currentLastCol < this.LastColumn)
            {
                //new right col
                for (int col = this.currentLastCol + 1; col <= this.LastColumn; ++col)
                {
                    this.GenerateCol(col, true);
                }

                this.currentLastCol = this.LastColumn;
            }

            if (this.currentFirstCol < this.FirstColumn)
            {
                //left col invisible
                for (int col = this.currentFirstCol; col < this.FirstColumn; ++col)
                {
                    this.DesstroyCol(col, true);
                }

                this.currentFirstCol = this.FirstColumn;
            }

            if (this.currentLastCol > this.LastColumn)
            {
                //right col invisible
                for (int col = this.currentLastCol; col > this.LastColumn; --col)
                {
                    this.DesstroyCol(col, false);
                }

                this.currentLastCol = this.LastColumn;
            }

            if (this.currentFirstRow > this.FirstRow)
            {
                //new top row
                for (int row = this.currentFirstRow - 1; row >= this.FirstRow; --row)
                {
                    this.GenerateRow(row, true);
                }

                this.currentFirstRow = this.FirstRow;
            }

            if (this.currentLastRow < this.LastRow)
            {
                //new bottom row
                for (int row = this.currentLastRow + 1; row <= this.LastRow; ++row)
                {
                    this.GenerateRow(row, false);
                }

                this.currentLastRow = this.LastRow;
            }

            if (this.currentFirstRow < this.FirstRow)
            {
                for (int row = this.currentFirstRow; row < this.FirstRow; ++row)
                {
                    this.DestroyRow(row, true);
                }

                this.currentFirstRow = this.FirstRow;
            }

            if (this.currentLastRow > this.LastRow)
            {
                for (int row = this.currentLastRow; row > this.LastRow; --row)
                {
                    this.DestroyRow(row, false);
                }

                this.currentFirstRow = this.LastRow;
            }
        }
    }
}