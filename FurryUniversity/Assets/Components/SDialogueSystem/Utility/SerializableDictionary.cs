using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SDS.Utility
{
    public class SerializableDictionary
    {
    }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : SerializableDictionary, IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<SerializableKeyValuePair> list = new List<SerializableKeyValuePair>();

        [Serializable]
        public struct SerializableKeyValuePair
        {
            public TKey Key;
            public TValue Value;

            public SerializableKeyValuePair(TKey key, TValue value)
            {
                this.Key = key;
                this.Value = value;
            }

            public void SetValue(TValue value)
            {
                this.Value = value;
            }
        }

        private Dictionary<TKey, uint> KeyPositions => this._keyPositions.Value;
        private Lazy<Dictionary<TKey, uint>> _keyPositions;//延迟加载一下

        public SerializableDictionary()
        {
            this._keyPositions = new Lazy<Dictionary<TKey, uint>>(this.MakeKeyPositions);
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            this._keyPositions = new Lazy<Dictionary<TKey, uint>>(this.MakeKeyPositions);

            if (dictionary == null)
            {
                throw new ArgumentException("The passed dictionary is null.");
            }

            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                this.Add(pair.Key, pair.Value);
            }
        }

        private Dictionary<TKey, uint> MakeKeyPositions()
        {
            int numEntries = this.list.Count;

            Dictionary<TKey, uint> result = new Dictionary<TKey, uint>(numEntries);

            for (int i = 0; i < numEntries; ++i)
            {
                result[this.list[i].Key] = (uint)i;
            }

            return result;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            //反序列化为对象之后，key的索引可能会改变
            this._keyPositions = new Lazy<Dictionary<TKey, uint>>(this.MakeKeyPositions);
        }

        #region IDictionary
        public TValue this[TKey key]
        {
            get => this.list[(int)this.KeyPositions[key]].Value;
            set
            {
                if (this.KeyPositions.TryGetValue(key, out uint index))
                {
                    this.list[(int)index].SetValue(value);
                }
                else
                {
                    this.KeyPositions[key] = (uint)this.list.Count;

                    this.list.Add(new SerializableKeyValuePair(key, value));
                }
            }
        }

        public ICollection<TKey> Keys => this.list.Select(tuple => tuple.Key).ToArray();
        public ICollection<TValue> Values => this.list.Select(tuple => tuple.Value).ToArray();

        public void Add(TKey key, TValue value)
        {
            if (this.KeyPositions.ContainsKey(key))
            {
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            }
            else
            {
                this.KeyPositions[key] = (uint)this.list.Count;

                this.list.Add(new SerializableKeyValuePair(key, value));
            }
        }

        public bool ContainsKey(TKey key) => this.KeyPositions.ContainsKey(key);

        public bool Remove(TKey key)
        {
            if (this.KeyPositions.TryGetValue(key, out uint index))
            {
                Dictionary<TKey, uint> kp = this.KeyPositions;

                kp.Remove(key);

                this.list.RemoveAt((int)index);

                int numEntries = this.list.Count;

                for (uint i = index; i < numEntries; i++)
                {
                    kp[this.list[(int)i].Key] = i;
                }

                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this.KeyPositions.TryGetValue(key, out uint index))
            {
                value = this.list[(int)index].Value;

                return true;
            }

            value = default;

            return false;
        }
        #endregion

        #region ICollection
        public int Count => this.list.Count;
        public bool IsReadOnly => false;

        public void Add(KeyValuePair<TKey, TValue> kvp) => this.Add(kvp.Key, kvp.Value);

        public void Clear()
        {
            this.list.Clear();
            this.KeyPositions.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> kvp) => this.KeyPositions.ContainsKey(kvp.Key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int numKeys = this.list.Count;

            if (array.Length - arrayIndex < numKeys)
            {
                throw new ArgumentException("arrayIndex");
            }

            for (int i = 0; i < numKeys; ++i, ++arrayIndex)
            {
                SerializableKeyValuePair entry = this.list[i];

                array[arrayIndex] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> kvp) => this.Remove(kvp.Key);
        #endregion

        #region IEnumerable
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.list.Select(ToKeyValuePair).GetEnumerator();

            static KeyValuePair<TKey, TValue> ToKeyValuePair(SerializableKeyValuePair skvp)
            {
                return new KeyValuePair<TKey, TValue>(skvp.Key, skvp.Value);
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        #endregion
    }
}