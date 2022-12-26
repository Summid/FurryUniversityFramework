using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.Linq;
#if !UNITY_2021_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif

namespace SFramework.Core.UI.Editor
{
    [CustomEditor(typeof(ReferenceCollector))]
    [CanEditMultipleObjects]
    public class ReferenceCollectorEditor : UnityEditor.Editor
    {
        private ReferenceCollector referenceCollector;
        private Object heroPrefab;
        private string searchKey = "";

        private string SearchKey
        {
            get
            {
                return this.searchKey;
            }
            set
            {
                if(this.searchKey != value)
                {
                    this.searchKey = value;
                    this.heroPrefab = this.referenceCollector.Get<Object>(this.searchKey);
                }
            }
        }

        private void OnEnable()
        {
            if (this.targets.Count() > 1)//只允许一个target
            {
                return;
            }
            this.referenceCollector = this.target as ReferenceCollector;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        /// <summary>
        /// 删除空引用的对象
        /// </summary>
        private void DeleteNullReference()
        {
            var dataProperty = this.serializedObject.FindProperty("data");
            for (int i = dataProperty.arraySize; i >= 0; --i)
            {
                var gameObjectProperty = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
                if (gameObjectProperty.objectReferenceValue == null)
                {
                    dataProperty.DeleteArrayElementAtIndex(i);
                }
            }
            dataProperty.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 是否是UIView
        /// </summary>
        /// <returns></returns>
        public bool IsViewPrefab()
        {
            if (this.referenceCollector.gameObject.GetComponent<UIItemSelector>() == null)
            {
                //UIView没有selector组件
                var stage = PrefabStageUtility.GetCurrentPrefabStage();//处于预制件模式下
                if (stage != null)
                {
                    if (stage.prefabContentsRoot == this.referenceCollector.gameObject)
                        return true;
                }
                else
                {
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(this.referenceCollector.gameObject))
                        return true;
                }
            }

            return false;
        }

        private void AddReference(SerializedProperty dataProperty, string key, params Object[] objects)
        {

        }
    }
}