using SFramework.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SFramework.Core.UI
{
    public interface IUIPool<TData>
    {
        void PoolSetData(TData data);
    }

    public class UIItemPool : UIItemBase
    {
        [UIFieldInit("prefab")]
        public GameObject prefab;

        private List<GameObject> activtyGOPool = new List<GameObject>();
        private List<GameObject> freeGOPool = new List<GameObject>();
        private List<UIItemBase> activityUIItems = new List<UIItemBase>();
        private Dictionary<GameObject, UIItemBase> uiItemsCache = new Dictionary<GameObject, UIItemBase>();

        private int startSiblingIndex = 0;

        protected override void OnAwake()
        {
            if (this.prefab == null)
            {
                this.prefab = this.ChildrenUIList.Count > 0 ? this.ChildrenUIList.First().Value : null;
            }

            if (this.prefab != null)
            {
                this.prefab.SetActive(false);
                this.startSiblingIndex = this.prefab.transform.GetSiblingIndex();
            }
        }


        #region 公开接口
        public void Init()
        {
            this.ReleaseAll();
            this.activityUIItems.Clear();
        }

        public void ReleaseAll()
        {
            foreach (GameObject go in this.activtyGOPool)
            {
                if (!this.freeGOPool.Contains(go))
                {
                    this.freeGOPool.Add(go);
                }
                go.SetActive(false);
            }

            this.activtyGOPool.Clear();
        }

        public async STask UpdateList<TData, TItem>(IList<TData> list, Action<TItem, int> action = null) where TItem : UIItemBase, IUIPool<TData>, new()
        {
            this.Init();
            if (list == null)
                return;
            for (int i = 0; i < list.Count; ++i)
            {
                TData data = list[i];
                TItem item = await this.GetItem<TItem>();
                int siblingIndex = this.startSiblingIndex + i;
                item.gameObject.transform.SetSiblingIndex(siblingIndex);
                item.PoolSetData(data);
                this.activityUIItems.Add(item);
                action?.Invoke(item, i);
            }
        }
        #endregion

        #region 私有宝贝
        private async STask<TItem> GetItem<TItem>() where TItem : UIItemBase, new()
        {
            if (this.prefab == null)
            {
                Debug.LogError("UIItemPool script: prefab is null");
                return null;
            }

            TItem item = null;
            GameObject go = null;
            if (this.freeGOPool.Count <= 0)
            {
                go = GameObject.Instantiate(this.prefab);
                go.transform.SetParent(this.gameObject.transform);
                go.SetActive(true);
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = Vector3.zero;
                this.freeGOPool.Add(go);
            }
            go = this.freeGOPool[0];
            this.freeGOPool.RemoveAt(0);
            this.activtyGOPool.Add(go);

            item = await this.TryGetItemForGO<TItem>(go);
            await item.ShowAsync();
            return item;
        }

        /// <summary>
        /// GameObject和UIItemBase贴贴
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="prefabInstance"></param>
        /// <returns></returns>
        private async STask<TItem> TryGetItemForGO<TItem>(GameObject prefabInstance) where TItem : UIItemBase, new()
        {
            if (this.uiItemsCache.TryGetValue(prefabInstance, out UIItemBase value))
            {
                return value as TItem;
            }
            
            TItem item = await this.AddUIItemToGameObjectAsync<TItem>(prefabInstance);
            this.uiItemsCache.Add(prefabInstance, item);
            return item;
        }

        #endregion
    }
}
