using SFramework.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SFramework.Core.UI
{
    [Serializable]
    public class ReferenceCollectorData
    {
        public string key;
        public UnityEngine.Object gameObject;
        public List<UnityEngine.Object> gameObjectList;
        public bool IsList;
    }

    public class ReferenceCollectorDataComparer : IComparer<ReferenceCollectorData>
    {
        public int Compare(ReferenceCollectorData x, ReferenceCollectorData y)
        {
            return string.Compare(x.key, y.key, StringComparison.Ordinal);
        }
    }

    public class ReferenceCollector : MonoBehaviour, IIgnoreUIGenCode, ISerializationCallbackReceiver
    {
        public List<ReferenceCollectorData> data = new List<ReferenceCollectorData>();

        private readonly SerializableDictionary<string,UnityEngine.Object> dic = new SerializableDictionary<string, UnityEngine.Object>();
        private readonly SerializableDictionary<string, List<UnityEngine.Object>> dicList = new SerializableDictionary<string, List<UnityEngine.Object>>();

#if UNITY_EDITOR
        public void Add(string key, UnityEngine.Object obj)
        {
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty dataProperty = serializedObject.FindProperty("data");
            int i;
            for (i = 0; i < this.data.Count; i++)
            {
                if (this.data[i].key == key)
                {
                    break;
                }
            }
            if (i != this.data.Count)
            {
                SerializedProperty element = dataProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
            }
            else
            {
                dataProperty.InsertArrayElementAtIndex(i);
                SerializedProperty element = dataProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("key").stringValue = key;
                element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
            }
            EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }

        public void Remove(string key)
        {
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty dataProperty = serializedObject.FindProperty("data");
            int i;
            for (i = 0; i < this.data.Count; ++i)
            {
                if (this.data[i].key == key)
                {
                    break;
                }
            }
            if (i != this.data.Count)
            {
                dataProperty.DeleteArrayElementAtIndex(i);
            }
            EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }

        public void Clear()
        {
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty dataProperty = serializedObject.FindProperty("data");
            dataProperty.ClearArray();
            EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }

        public void Sort()
        {
            SerializedObject serializedObject = new SerializedObject(this);
            this.data.Sort(new ReferenceCollectorDataComparer());
            EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }
#endif

        public bool IsList(string key)
        {
            if (this.dicList.ContainsKey(key))
                return true;

            return false;
        }

        public T Get<T>(string key) where T : class
        {
            if (!this.dic.TryGetValue(key, out UnityEngine.Object dictGO))
            {
                return null;
            }
            return dictGO as T;
        }

        public List<T> GetList<T>(string key) where T : UnityEngine.Object
        {
            if (!this.dicList.TryGetValue(key, out List<UnityEngine.Object> dictGO))
            {
                return null;
            }
            List<T> result = new List<T>();
            foreach (var go in dictGO)
            {
                result.Add(go as T);
            }
            return result;
        }

        public UnityEngine.Object GetObject(string key)
        {
            if (!this.dic.TryGetValue(key, out UnityEngine.Object dictGO))
            {
                return null;
            }
            return dictGO;
        }

        public void OnAfterDeserialize()
        {
            this.dic.Clear();
            this.dicList.Clear();
            foreach (ReferenceCollectorData referenceCollectorData in this.data)
            {
                if (!referenceCollectorData.IsList)
                {
                    if (!this.dic.ContainsKey(referenceCollectorData.key))
                    {
                        this.dic.Add(referenceCollectorData.key, referenceCollectorData.gameObject);
                    }
                }
                else
                {
                    if (!this.dicList.ContainsKey(referenceCollectorData.key))
                    {
                        this.dicList.Add(referenceCollectorData.key, new List<UnityEngine.Object>(referenceCollectorData.gameObjectList));
                    }
                }
            }
        }

        public void OnBeforeSerialize()
        {
            
        }
    }
}