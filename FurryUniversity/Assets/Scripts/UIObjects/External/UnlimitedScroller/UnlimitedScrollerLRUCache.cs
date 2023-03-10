using System;
using System.Collections.Generic;

namespace SFramework.Core.UI.External.UnlimitedScroller
{
    internal readonly struct LRUCacheItem<TK, TV>
    {
        public readonly TK key;
        public readonly TV value;

        public LRUCacheItem(TK key, TV value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public class UnlimitedScrollerLRUCache<TK, TV>
    {
        /// <summary> 淘汰缓存对象时触发 </summary>
        private readonly Action<TK, TV> onDestroy;
        private uint capacity;

        /// <summary> 保存缓存节点及其数据，越靠近队首，被淘汰的优先级越高 </summary>
        private readonly LinkedList<LRUCacheItem<TK, TV>> lruList = new LinkedList<LRUCacheItem<TK, TV>>();//新增数据 和 刚访问过的旧数据 放在队尾

        /// <summary> 用于快速查找链表 <see cref="lruList"/> 中的节点 </summary>
        private readonly Dictionary<TK, LinkedListNode<LRUCacheItem<TK, TV>>> cacheMap = new Dictionary<TK, LinkedListNode<LRUCacheItem<TK, TV>>>();

        /// <summary> 当前缓存的节点数量 </summary>
        public int Count => this.cacheMap.Count;

        public UnlimitedScrollerLRUCache(Action<TK, TV> onDestroy, uint capacity)
        {
            this.onDestroy = onDestroy;
            this.capacity = capacity;
        }

        public TV Get(TK key)
        {
            if (!this.cacheMap.TryGetValue(key, out var node))
            {
                return default;
            }

            TV value = node.Value.value;
            this.lruList.Remove(node);
            this.lruList.AddLast(node);
            return value;
        }

        public bool TryGet(TK key, out TV value)
        {
            if (!this.cacheMap.TryGetValue(key, out var node))
            {
                value = default;
                return false;
            }

            value = node.Value.value;
            this.lruList.Remove(node);
            this.lruList.AddLast(node);
            return true;
        }

        public void Add(TK key, TV value)
        {
            //已存在key相同的节点，只修改淘汰级别
            if (this.cacheMap.TryGetValue(key, out var existingNode))
            {
                this.lruList.Remove(existingNode);
                this.lruList.AddLast(existingNode);
                return;
            }

            //容量设置为0，立即删除
            if (this.capacity <= 0)
            {
                this.onDestroy?.Invoke(key,value);
                return;
            }

            //容量超出限制，淘汰一个节点
            if (this.Count >= this.capacity)
            {
                this.RemoveFirst();
            }

            var cacheItem = new LRUCacheItem<TK, TV>(key, value);
            var node = new LinkedListNode<LRUCacheItem<TK, TV>>(cacheItem);
            this.lruList.AddLast(node);
            this.cacheMap.Add(key, node);
        }

        public TV Remove(TK key)
        {
            if (!this.cacheMap.TryGetValue(key, out var existingNode))
            {
                return default;
            }

            this.lruList.Remove(existingNode);
            this.cacheMap.Remove(key);
            return existingNode.Value.value;
        }

        public void SetCapacity(uint newCapacity)
        {
            this.capacity = newCapacity;
            this.Trim();
        }

        private void Trim()
        {
            while (this.Count > this.capacity)
            {
                this.RemoveFirst();
            }
        }

        public void Clear()
        {
            while (this.Count > 0)
            {
                this.RemoveFirst();
            }
        }

        private void RemoveFirst()
        {
            var node = this.lruList.First;
            this.lruList.RemoveFirst();
            this.cacheMap.Remove(node.Value.key);

            this.onDestroy?.Invoke(node.Value.key,node.Value.value);
        }
    }
}