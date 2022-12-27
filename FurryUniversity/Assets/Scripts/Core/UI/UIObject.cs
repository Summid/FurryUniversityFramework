using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public abstract class UIObject
    {
        /// <summary> UI所属的GameObject对象实例 </summary>
        public GameObject gameObject { get; private set; }
        public Type ClassType { get; private set; }

        private ReferenceCollector rc;
        private Dictionary<UIObject, GameObject> childrenUIList = new Dictionary<UIObject, GameObject>();

        public virtual void Awake(GameObject gameObjectHost)
        {
            this.ClassType = this.GetType();
            this.gameObject = gameObjectHost;
            this.rc = this.gameObject.GetComponent<ReferenceCollector>();

            //TODO update logic
            this.InitItemSelector(this.gameObject.transform);

            this.OnAwake();
        }

        /// <summary>
        /// 检查Prefab上的Selector组件，通过该组件初始化UIItem脚本
        /// </summary>
        /// <remarks>
        /// 使用递归的方式，获取下一级所有挂载有Selector脚本的节点，创建UIObject对象
        /// 但不再深入挂有Selector脚本的节点的子节点（它自己会检查自己的子节点）
        /// </remarks>
        /// <param name="rootTransform"></param>
        private void InitItemSelector(Transform rootTransform)
        {
            for (int i = 0; i < rootTransform.childCount; i++)
            {
                var child = rootTransform.GetChild(i);
                var selector = child.GetComponent<UIItemSelector>();
                if (selector != null)
                {
                    Type type = Type.GetType(selector.SelectClass);
                    this.AddMissingItem(selector, type);
                }
                else
                {
                    this.InitItemSelector(child);
                }
            }
        }

        /// <summary>
        /// 激活子UIItem对象
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private UIItemBase AddMissingItem(UIItemSelector selector, Type itemType)
        {
            if (itemType == null)
            {
                throw new Exception($"[{this.GetType().FullName}]下的[{selector.gameObject.name}]配置上有错误");
            }

            GameObject go = selector.gameObject;
            foreach (var pair in this.childrenUIList)
            {
                if (pair.Value == go)
                {
                    return pair.Key as UIItemBase;
                }
            }

            UIItemBase itemObj = null;
            if (selector.UIObject != null)
            {
                itemObj = selector.UIObject as UIItemBase;
            }
            else
            {
                itemObj = Activator.CreateInstance(itemType) as UIItemBase;
                selector.UIObject = itemObj;
                itemObj.Awake(go);
            }

            this.childrenUIList.Add(itemObj, go);

            return itemObj;
        }

        /// <summary> 首次创建时触发 </summary>
        protected virtual void OnAwake() { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class UIFieldInitAttribute : Attribute
    {
        public string RCKey { get; private set; }

        public UIFieldInitAttribute(string rcKey)
        {
            this.RCKey = rcKey;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class UISerializableAttribute : Attribute
    {

    }
}