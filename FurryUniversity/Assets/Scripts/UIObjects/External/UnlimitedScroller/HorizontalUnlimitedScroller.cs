using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SFramework.Core.UI.External.UnlimitedScroller
{
    public class HorizontalUnlimitedScroller : HorizontalLayoutGroup, IUnlimitedScroller
    {
        #region Properties

        /// <inheritdoc cref="IUnlimitedScroller.Initialized"/>
        public bool Initialized { get; private set; }

        /// <inheritdoc cref="IUnlimitedScroller.Generated"/>
        public bool Generated { get; private set; }

        /// <inheritdoc cref="IUnlimitedScroller.RowCount"/>
        public int RowCount => 1;//horizontal scroller 行数固定为1

        /// <inheritdoc cref="IUnlimitedScroller.FirstRow"/>
        public int FirstRow => 0;

        /// <inheritdoc cref="IUnlimitedScroller.LastRow"/>
        public int LastRow => 0;

        /// <inheritdoc cref="IUnlimitedScroller.FirstColumn"/>
        public int FirstColumn
        {
            get
            {
                //锚点为(0,1)，pivot为(0,1)，若contentTrans宽度超过parentTrans，只计算超出部分；若没超过，则肯定为0
                int col = (int)((-this.contentTrans.anchoredPosition.x - this.offsetPadding.left) / (this.cellX + this.spacingX));
                return Mathf.Clamp(col, 0, this.CellPerRow - 1);
            }
        }

        /// <inheritdoc cref="IUnlimitedScroller.LastColumn"/>
        public int LastColumn
        {
            get
            {
                int col = (int)((-this.contentTrans.anchoredPosition.x + this.ViewportWidth - this.offsetPadding.left) / (this.cellX + this.spacingX));
                return Mathf.Clamp(col, 0, this.CellPerRow - 1);
            }
        }

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

        /// <inheritdoc cref="IUnlimitedScroller.ViewportWidth"/>
        public int CellPerRow => this.totalCount;

        /// <inheritdoc cref="IUnlimitedScroller.CellPrefab"/>
        public GameObject CellPrefab => this.cellPrefab;

        #endregion

        #region Public Fields

        [Tooltip("Max size of cached cell")]
        public uint cacheSize;

        [Tooltip("The scrollRect component on ScrollView")]
        public ScrollRect scrollRect;

        [Tooltip("The template GameObject")]
        public GameObject cellPrefab;

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
            Destroy(this.pendingDestroyGo);
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

            var cellColCount = index % this.CellPerRow;//在第几列
            float horizontalPosition;
            switch (method)
            {
                case JumpToMethod.OnScreen:
                    if (cellColCount >= this.FirstColumn && cellColCount <= this.LastColumn)
                        return;

                    if (cellColCount <= this.LastColumn)
                    {
                        //在viewport左边
                        horizontalPosition =
                            (this.offsetPadding.left +
                             (cellColCount) * this.cellX + cellColCount * this.spacingX) /
                            (this.ContentWidth - this.ViewportWidth); //除以content与viewport的宽度差delta，而不是content的宽度，因为scroll的移动距离就是delta的宽度
                    }
                    else
                    {
                        //在viewport右边
                        horizontalPosition =
                            (this.offsetPadding.left +
                            (cellColCount + 1) * this.cellX + cellColCount * this.spacingX - this.ViewportWidth) / //同上，因为scroll的移动距离是delta的宽度，这里要把cell放在viewport最右方，因此计算出的坐标需减去viewport的高度后再计算比值
                            (this.ContentWidth - this.ViewportWidth);
                    }

                    if (this.ContentWidth > this.ViewportWidth && !(cellColCount >= this.FirstColumn && cellColCount <= this.LastColumn))
                    {
                        this.scrollRect.horizontalNormalizedPosition = horizontalPosition;//[0,1]，0为最左方
                    }
                    return;
                case JumpToMethod.Center:
                    horizontalPosition =
                        (this.offsetPadding.left +
                        (cellColCount + 0.5f) * this.cellX + cellColCount * this.spacingX - this.ViewportWidth / 2f) /
                        (this.ContentWidth - this.ViewportWidth);

                    if (this.ContentWidth > this.ViewportWidth)
                    {
                        this.scrollRect.horizontalNormalizedPosition = horizontalPosition;
                    }
                    return;
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
            this.spacingX = ((HorizontalOrVerticalLayoutGroup)this.layoutGroup).spacing;
            this.spacingY = 0f;

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

        private int GetCellIndex(int row, int col)
        {
            return this.CellPerRow * row + col;
        }

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
            instance.transform.SetSiblingIndex(order);
            Cell cell = new Cell { go = instance, index = index };
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
                for (var c = this.currentFirstCol; c <= this.currentLastCol; ++c)
                {
                    int index = this.GetCellIndex(r, c);
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

            for (int i = firstRow; i <= lastRow; ++i)
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

        private void DestroyCol(int col, bool onLeft)
        {
            int firstRow = this.currentFirstRow;
            int lastRow = this.currentLastRow;

            for(int i = firstRow; i <= lastRow; ++i)
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

        private void OnScroll(Vector2 positon)
        {
            if (!this.Generated || this.totalCount <= 0)
                return;

            if (this.LastColumn < this.currentFirstCol || this.FirstColumn > this.currentLastCol || this.FirstRow > this.currentLastRow || this.LastRow < this.currentFirstRow)
            {
                this.DestroyAllCells();
                this.GenerateAllCells();
                return;
            }

            if (this.currentFirstCol > this.FirstColumn)
            {
                //new left col
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
                    this.GenerateCol(col, false);
                }

                this.currentLastCol = this.LastColumn;
            }

            if (this.currentFirstCol < this.FirstColumn)
            {
                //left col invisible
                for (int col = this.currentFirstCol; col < this.FirstColumn; ++col)
                {
                    this.DestroyCol(col, true);
                }

                this.currentFirstCol = this.FirstColumn;
            }

            if (this.currentLastCol > this.LastColumn)
            {
                //right col invisible
                for (int col = this.currentLastCol; col > this.LastColumn; --col)
                {
                    this.DestroyCol(col, false);
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
                //top row invisible
                for (int row = this.currentFirstRow; row < this.FirstRow; ++row)
                {
                    this.DestroyRow(row, true);
                }

                this.currentFirstRow = this.FirstRow;
            }

            if (this.currentLastRow > this.LastRow)
            {
                //bottom row invisible
                for (int row = this.currentLastRow; row > this.LastRow; --row)
                {
                    this.DestroyRow(row, false);
                }

                this.currentLastRow = this.LastRow;
            }
        }
    }
}