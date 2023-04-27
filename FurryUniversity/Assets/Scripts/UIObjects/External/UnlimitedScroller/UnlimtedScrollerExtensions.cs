using SFramework.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI.External.UnlimitedScroller
{
    public interface IUIScrollerCell<T>
    {
        void ScrollerSetData(T data);
    }

    public static class UnlimitedScrollerExtensions
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

            async void OnGenerateHandler(int index, ICell iCell)
            {
                var item = await host.AddUIItemToGameObjectAsync<TUIItem>(iCell.GameObject);
                item.ScrollerSetData(datas[index]);
                onGenerated?.Invoke(index);
            }
            
            async void OnDestroyHandler(ICell iCell)
            {
                var item = await host.AddUIItemToGameObjectAsync<TUIItem>(iCell.GameObject);
                item.DisposeAsync().Forget();
            }

            scroller.Generate(scroller.CellPrefab, datas.Count, OnGenerateHandler, (Action<ICell>)OnDestroyHandler);
        }
    }
}