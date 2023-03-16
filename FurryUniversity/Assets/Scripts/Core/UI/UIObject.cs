using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace SFramework.Core.UI
{
    public abstract class UIObject
    {
        public static readonly Type ListType = typeof(List<>);

        /// <summary> UI所属的GameObject对象实例 </summary>
        public GameObject gameObject { get; private set; }
        public virtual EnumViewState UIState { get; protected set; }

        protected virtual RectTransform VisualRoot => this.gameObject.transform as RectTransform;
        public Type ClassType { get; private set; }

        private ReferenceCollector rc;
        private Dictionary<UIObject, GameObject> childrenUIList = new Dictionary<UIObject, GameObject>();
        public Dictionary<UIObject, GameObject> ChildrenUIList => this.childrenUIList;
        private UIManager UIManager { get { return GameManager.Instance.UIManager; } }

        private CancellationTokenSource updateCTS;

        public virtual void Awake(GameObject gameObjectHost)
        {
            this.ClassType = this.GetType();
            this.gameObject = gameObjectHost;
            this.rc = this.gameObject.GetComponent<ReferenceCollector>();

            this.InitItemSelector(this.gameObject.transform);
            this.InitCustomAttribute();

            this.OnAwake();
        }

#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        /// <summary>
        /// 因UIViewBase中Awake需要异步等待创建MaskView，这里提供异步版的Awake方法
        /// </summary>
        /// <param name="gameObjectHost"></param>
        /// <returns></returns>
        public virtual async STask AwakeAsync(GameObject gameObjectHost) { }
#pragma warning restore CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行

        #region 内部方法
        /// <summary>
        /// 设置UIFieldInitAttribute和UISerializableAttribute字段
        /// </summary>
        private void InitCustomAttribute()
        {
            var selector = this.gameObject.GetComponent<UIItemSelector>();

            var fields = this.ClassType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            for (int i = 0, fieldsCount = fields.Length; i < fieldsCount; i++)
            {
                var field = fields[i];
                var atts = field.GetCustomAttributes(true);
                for (int j = 0, attCount = atts.Length; j < attCount; j++)
                {
                    var att = atts[j];
                    if (att is UIFieldInitAttribute uiFieldAtt)
                    {
                        this.SetUIInitField(uiFieldAtt, field);
                    }
                    else if (att is UISerializableAttribute uiSerAtt)
                    {
                        this.SetUISerialzation(selector, field);
                    }
                }
            }
        }

        /// <summary>
        /// 设置序列化信息，将selector脚本中的值放到UIObject脚本中
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="field"></param>
        private void SetUISerialzation(UIItemSelector selector, FieldInfo field)
        {
            var paramList = selector.UIConfigParam;
            int count = paramList.Count;
            for (int i = 0; i < count; i++)
            {
                var param = paramList[i];
                if (param.Name == field.Name)
                {
                    if (field.FieldType == UIItemSelector.INI_TYPE)
                        field.SetValue(this, param.IntValue);
                    else if (field.FieldType == UIItemSelector.LONG_TYPE)
                        field.SetValue(this, param.LongValue);
                    else if (field.FieldType == UIItemSelector.FLOAT_TYPE)
                        field.SetValue(this, param.FloatValue);
                    else if (field.FieldType == UIItemSelector.DOUBLE_TYPE)
                        field.SetValue(this, param.DoubleValue);
                    else if (field.FieldType == UIItemSelector.STR_TYPE)
                        field.SetValue(this, param.StrValue);
                    else if (field.FieldType == UIItemSelector.BOOL_TYPE)
                        field.SetValue(this, param.BoolValue);
                    else if (field.FieldType.IsSubclassOf(UIItemSelector.ENUM_TYPE))
                        field.SetValue(this, param.IntValue);
                }
            }
        }

        /// <summary>
        /// 设置引用节点
        /// </summary>
        /// <param name="uiAtt"></param>
        /// <param name="field"></param>
        private void SetUIInitField(UIFieldInitAttribute uiAtt, FieldInfo field)
        {
            if (field.FieldType.Name == ListType.Name)//数组
            {
                Type type = field.FieldType.GetGenericArguments()[0];//泛型类型
                List<object> values = this.GetUIElements(type, uiAtt.RCKey);
                if (values == null)
                {
                    Debug.LogWarning($"未找到名为{uiAtt.RCKey}对应的List<GameObject>");
                    return;
                }

                object listInstance = Activator.CreateInstance(field.FieldType);
                MethodInfo addMethod = field.FieldType.GetMethod("Add");//List.Add() Method
                for (int i = 0, count = values.Count; i < count; ++i)
                {
                    object value = values[i];
                    addMethod.Invoke(listInstance, new object[] { value });
                }
                field.SetValue(this, listInstance);
            }
            else
            {
                object value = this.GetUIElement(field.FieldType, uiAtt.RCKey);
                field.SetValue(this, value);
            }
        }

        /// <summary>
        /// 获取ReferenceCollector中的引用
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private object GetUIElement(Type type, string key)
        {
            if (this.rc == null)
            {
                Debug.LogError($"[{this}] 没有挂在ReferenceCollector脚本，无法找到引用");
                return null;
            }

            GameObject go = this.rc.Get<GameObject>(key);
            if (go != null)
            {
                if (type.IsSubclassOf(typeof(UIItemBase)))//UIItem类型
                {
                    var selector = go.GetComponent<UIItemSelector>();
                    if (selector == null || string.IsNullOrEmpty(selector.SelectClass))
                    {
                        return null;
                    }
                    else
                    {
                        Type itemType = Type.GetType(selector.SelectClass);
                        if (itemType != null)
                        {
                            var itemObj = this.AddMissingItem(selector, itemType);
                            if (itemObj != null)
                            {
                                return itemObj;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[{this}] 在初始化InitField标签字段时没有在程序集中找到字段类型 {selector.SelectClass}");
                        }
                        return null;
                    }
                }
                else
                {
                    if (type == typeof(GameObject))
                        return go;
                    else
                    {
                        Component target = go.GetComponent(type);
                        return target;
                    }
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取ReferenceCollector中的引用
        /// </summary>
        /// <param name="type">List<T> 中T的类型</param>
        /// <param name="key"></param>
        /// <returns></returns>
        private List<object> GetUIElements(Type type, string key)
        {
            if (this.rc == null)
            {
                Debug.LogError($"[{this}] 没有挂在ReferenceCollector脚本，无法找到引用");
                return null;
            }

            List<object> uiObjectList = new List<object>();
            List<GameObject> goList = this.rc.GetList<GameObject>(key);
            if (goList == null)
            {
                return null;
            }

            if (goList.Count > 0)
            {
                if (type.IsSubclassOf(typeof(UIItemBase)))//UIItem类型
                {
                    for (int i = 0, count = goList.Count; i < count; i++)
                    {
                        GameObject go = goList[i];
                        var selector = go.GetComponent<UIItemSelector>();
                        if (selector == null || string.IsNullOrEmpty(selector.SelectClass))
                        {
                            continue;
                        }
                        else
                        {
                            Type itemType = Type.GetType(selector.SelectClass);
                            if (itemType != null)
                            {
                                var itemObj = this.AddMissingItem(selector, itemType);
                                if (itemObj != null)
                                {
                                    uiObjectList.Add(itemObj);
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"[{this}] 在初始化InitField标签字段时没有在程序集中找到字段类型 {selector.SelectClass}");
                            }
                            continue;
                        }
                    }
                }
                else
                {
                    if (type == typeof(GameObject))
                    {
                        return new List<object>(goList);
                    }
                    else
                    {
                        for (int i = 0, count = goList.Count; i < count; ++i)
                        {
                            GameObject go = goList[i];
                            if (go == null)
                            {
                                continue;
                            }
                            Component target = go.GetComponent(type);
                            uiObjectList.Add(target);
                        }
                    }
                }
            }
            else
            {
                return null;
            }

            return uiObjectList;
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
                if (child.TryGetComponent<UIItemSelector>(out var selector))
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
        /// 激活子UIItem对象脚本
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

        /// <summary>
        /// 初始化Update迭代逻辑
        /// </summary>
        private void InitUpdateLogic()
        {
            if (this is IUIUpdator updator)
            {
                if (this.updateCTS != null)
                {
                    this.updateCTS.Cancel();
                    this.updateCTS.Dispose();
                    this.updateCTS = null;
                }
                this.updateCTS = new CancellationTokenSource();
                STask.UpdateTask(updator.OnUpdate, PlayerLoopTiming.Update, this.updateCTS.Token);
            }
        }
        #endregion

        #region 外部接口
        protected async STask<UIItem> CreateChildItemAsync<UIItem>(string itemAssetName, Transform parent = null)
            where UIItem : UIItemBase, new()
        {
            UIItemInfo info = this.UIManager.GetUIItemInfo(itemAssetName);
            if (info != null)
            {
                if (parent == null)
                    parent = this.VisualRoot;
                string assetName = itemAssetName;
                string assetBundleName = info.UIItemAssetBundleName;
                string className = info.UIItemClassName;

                if (this.gameObject == null)
                {
                    return default;
                }

                GameObject prefabObj = await AssetBundleManager.LoadAssetInAssetBundleAsync<GameObject>(assetName, assetBundleName);
                prefabObj = UnityEngine.Object.Instantiate(prefabObj, parent);
                prefabObj.transform.localScale = Vector3.one;
                prefabObj.transform.localPosition = Vector3.zero;
                if (!prefabObj.TryGetComponent<UIItemSelector>(out var selector))
                    selector = prefabObj.AddComponent<UIItemSelector>();

                selector.SelectClass = className;
                var itemType = Type.GetType(selector.SelectClass);
                if (itemType != null)
                {
                    var itemObj = Activator.CreateInstance<UIItem>();
                    this.childrenUIList.Add(itemObj, prefabObj);
                    itemObj.Awake(prefabObj);
                    itemObj.BundleName = assetBundleName;
                    return itemObj;
                }
            }
            return default;
        }

        /// <summary>
        /// 给GameObject添加UIItem脚本，若GameObject上已有需添加的脚本则直接返回
        /// </summary>
        /// <typeparam name="TUIItem"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        public TUIItem AddUIItemOnGameObject<TUIItem>(GameObject go) where TUIItem : UIItemBase, new()
        {
            foreach (var pair in this.childrenUIList)
            {
                if (pair.Value == go)
                    return pair.Key as TUIItem;
            }

            TUIItem itemObj = Activator.CreateInstance<TUIItem>();
            this.childrenUIList.Add(itemObj, go);
            itemObj.Awake(go);
            return itemObj;
        }

        protected void DisposeChildItem<UIItem>(UIItem item) where UIItem : UIItemBase, new()
        {
            this.RemoveChild(item);
            item.Dispose();
        }

        public bool RemoveChild(UIItemBase item)
        {
            return this.childrenUIList.Remove(item);
        }
        #endregion

        #region 生命周期方法
        public virtual void Dispose()
        {
            if (this.UIState == EnumViewState.Disposed)
                return;
            this.UIState = EnumViewState.Disposed;

            if (this.updateCTS != null)
            {
                this.updateCTS.Cancel();
                this.updateCTS.Dispose();
                this.updateCTS = null;
            }

            if (this.gameObject != null && !this.gameObject.Equals(null))
                GameObject.Destroy(this.gameObject);

            this.UIManager.DisposeUIBundle(this);

            //handle children
            foreach (var uiObj in this.childrenUIList.Keys)
            {
                if (uiObj == this)
                    continue;
                uiObj.Dispose();
            }

            this.OnDispose();
        }

        public virtual void Show() { }
        public virtual void Hide() { }


        /// <summary> 首次创建时触发 </summary>
        protected virtual void OnAwake() { }

        /// <summary> 调用<see cref="Show"/>后触发 </summary>
        protected virtual void OnShow() { }

        /// <summary> 调用<see cref="Hide"/>后触发，用户使用 </summary>
        protected virtual void OnHide() { }

        /// <summary> 调用<see cref="Dispose"/>后触发 </summary>
        protected virtual void OnDispose() { }

        /// <summary> 调用<see cref="Show"/>后触发，底层使用，不暴露给用户 </summary>
        protected virtual void OnEnable()
        {
            this.OnShow();
            this.InitUpdateLogic();
        }

        /// <summary> 调用<see cref="Hide"/>后触发，底层使用，不暴露给用户 </summary>
        protected virtual void OnDisable()
        {
            //Remove something here，协程、计时器等
            this.updateCTS?.Cancel();
            this.OnHide();
        }
        #endregion

        public enum EnumViewState
        {
            NULL,
            Shown,
            Hidden,
            Disposed,
            Hidding
        }
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

    /// <summary>
    /// 实现该接口来实现类似Update的功能
    /// </summary>
    public interface IUIUpdator
    {
        void OnUpdate();
    }
}