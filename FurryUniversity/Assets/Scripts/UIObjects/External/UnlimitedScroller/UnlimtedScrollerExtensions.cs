using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI.External.UnlimitedScroller
{
    public interface IUIScrollerCell<T>
    {
        void ScrollerSetData(T data);
    }

    public static class UnlimtedScrollerExtensions
    {
        public static void UpdateScrollCells<TData, TUIItem>(this IUnlimitedScroller scroller, UIObject host, IList<TData> datas, Action<int> onGenerated = null)
            where TUIItem : UIItemBase, IUIScrollerCell<TData>, new()
        {
            GameObject itemGO = scroller.CellPrefab;
            if (itemGO == null)
            {
                Debug.LogWarning($"{host.gameObject.name} scroller's cellPrefab is empty");
                return;
            }
            if (!itemGO.TryGetComponent<ICell>(out var iCell))
            {
                itemGO.AddComponent<RegularCell>();
            }

            Action<int, ICell> onGenerateHandler = (index, iCell) =>
            {
                var item = host.AddUIItemOnGameObject<TUIItem>(iCell.GameObject);
                item.ScrollerSetData(datas[index]);
                onGenerated?.Invoke(index);
            };

            Action<ICell> onDestroyHandler = (iCell) =>
            {
                var item = host.AddUIItemOnGameObject<TUIItem>(iCell.GameObject);
                item.Dispose();
            };
            //TODO onDestroy
            scroller.Generate(scroller.CellPrefab, datas.Count, onGenerateHandler, onDestroyHandler);
        }
    }
}