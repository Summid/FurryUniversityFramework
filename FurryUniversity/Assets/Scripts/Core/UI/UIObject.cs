#pragma warning disable 1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
// #pragma warning disable 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.

using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace SFramework.Core.UI
{
    public abstract class UIObject
    {
        /// <summary> Type of List<> </summary>
        public static readonly Type ListType = typeof(List<>);

        /// <summary> View State </summary>
        public enum EnumViewState { Null, Shown, Hidden, Disposed, Hiding }


        /// <summary> UI所属的GameObject对象实例 </summary>
        public GameObject gameObject { get; private set; }

        /// <summary> Null, Shown, Hidden, Disposed, Hiding </summary>
        public virtual EnumViewState UIState { get; protected set; }

        /// <summary> RectTransform of the gameObject </summary>
        protected virtual RectTransform VisualRoot => this.gameObject.transform as RectTransform;
        
        /// <summary> The Type of the UIObject instance's script </summary>
        public Type ClassType { get; private set; }

        private ReferenceCollector rc;
        private Dictionary<UIObject, GameObject> childrenUIList = new Dictionary<UIObject, GameObject>();
        /// <summary>
        /// 该UI下所有UIItem及其GameObject，包括自己声明的被UIFieldInit标记的UIItem成员，也包括子节点声明的；
        /// 当自己Dispose后，它们会一起Dispose掉，子UIObject就不需要自己再Dispose了
        /// </summary>
        public Dictionary<UIObject, GameObject> ChildrenUIList => this.childrenUIList;
        protected UIManager UIManager { get { return GameManager.Instance.UIManager; } }
        private CancellationTokenSource updaterCTS;
        
        
        
        #region 内部方法
        /// <summary>
        /// 设置UIFieldInitAttribute和UISerializableAttribute标记的字段
        /// </summary>
        private void InitCustomAttribute()
        {
            FieldInfo[] fields =
                this.ClassType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            for (int i = 0, fieldsCount = fields.Length; i < fieldsCount; ++i)
            {
                FieldInfo field = fields[i];
                object[] attributes = field.GetCustomAttributes(true);
                for (int j = 0, count = attributes.Length; j < count; ++j)
                {
                    object attribute = attributes[j];
                    if (attribute is UIFieldInitAttribute uiFieldInitAttribute)
                    {
                        this.SetUIFieldInit(uiFieldInitAttribute, field);
                    }
                    else if (attribute is UISerializableAttribute uiSerializableAttribute)
                    {
                        UIItemSelector selector = this.gameObject.GetComponent<UIItemSelector>();
                        this.SetUISerialization(selector, field);
                    }
                }
            }
        }

        /// <summary>
        /// 设置序列化信息，将selector脚本中的值放到UIObject脚本中
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="field"></param>
        private void SetUISerialization(UIItemSelector selector, FieldInfo field)
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
        /// 实例化引用节点
        /// </summary>
        /// <param name="uiAtt"><see cref="UIFieldInitAttribute"/></param>
        /// <param name="field"></param>
        private void SetUIFieldInit(UIFieldInitAttribute uiAtt, FieldInfo field)
        {
            if (field.FieldType.Name == ListType.Name)//数组
            {
                Type type = field.FieldType.GetGenericArguments()[0];
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
        /// 获取ReferenceCollector脚本中引用的对象
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
            if (go == null)
                return null;

            if (type.IsSubclassOf(typeof(UIItemBase)))//是UIItem类型
            {
                UIItemSelector selector = go.GetComponent<UIItemSelector>();
                if (selector == null || string.IsNullOrEmpty(selector.SelectClass))
                    return null;

                Type itemType = Type.GetType(selector.SelectClass);
                if (itemType == null)
                {
                    Debug.LogWarning($"[{this}] 在初始化UIInitField标签字段时没有在程序集中找到字段类型 {selector.SelectClass}，是否缺少了该脚本");
                    return null;
                }
                else
                {
                    UIItemBase itemObj = this.AddUIItemToGameObject(selector, itemType);
                    if (itemObj != null)
                        return itemObj;
                }

                return null;
            }
            else
            {
                //是Unity.Object类型
                if (type == typeof(GameObject))
                    return go;
                Component target = go.GetComponent(type);
                return target;
            }
        }

        /// <summary>
        /// 获取ReferenceCollector脚本中引用的数组对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private List<object> GetUIElements(Type type, string key)
        {
            if (this.rc == null)
            {
                Debug.LogError($"[{this}] 没有挂在ReferenceCollector脚本，无法找到引用");
                return null;
            }

            List<GameObject> goList = this.rc.GetList<GameObject>(key);
            if (goList == null)
            {
                return null;
            }
            
            List<object> uiObjectList = new List<object>();
            
            if (goList.Count <= 0)
                return null;

            if (type.IsSubclassOf(typeof(UIItemBase)))//UIItem类型
            {
                for (int i = 0, count = goList.Count; i < count; ++i)
                {
                    GameObject go = goList[i];
                    UIItemSelector selector = go.GetComponent<UIItemSelector>();
                    if (selector == null || string.IsNullOrEmpty(selector.SelectClass))
                        continue;
                    Type itemType = Type.GetType(selector.SelectClass);
                    if (itemType == null)
                        Debug.LogWarning($"[{this}] 在初始化UIInitField标签字段时没有在程序集中找到字段类型 {selector.SelectClass}，是否缺少了该脚本");
                    else
                    {
                        UIItemBase itemObj = this.AddUIItemToGameObject(selector, itemType);
                        if (itemObj != null)
                            uiObjectList.Add(itemObj);
                    }
                }
            }
            else
            {
                //是Unity.Object类型
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
                            continue;
                        Component target = go.GetComponent(type);
                        uiObjectList.Add(target);
                    }
                }
            }
            
            return uiObjectList;
        }

        /// <summary>
        /// 激活UIItemSelector上的UIItem脚本，若脚本已经被激活，则直接返回
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private UIItemBase AddUIItemToGameObject(UIItemSelector selector, Type itemType)
        {
            if (itemType == null)
                throw new Exception($"[{this.GetType().FullName}]下的[{selector.gameObject.name}]配置上有错误");

            GameObject go = selector.gameObject;
            foreach (var pair in this.childrenUIList)
            {
                if (pair.Value == go)
                    return pair.Key as UIItemBase;
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
#pragma warning disable CS4014
                itemObj.AwakeAsync(go);//这里以同步方式执行
#pragma warning restore CS4014
            }

            this.childrenUIList.Add(itemObj, go);
            return itemObj;
        }

        /// <summary>
        /// 检查子节点Prefab上的Selector组件，通过该组件初始化UIItem脚本
        /// </summary>
        /// <remarks>
        /// 使用递归的方式，获取下一级所有挂载有Selector脚本的节点，创建UIObject对象
        /// 但不再深入挂有Selector脚本的节点的子节点（它自己会检查自己的子节点）
        /// </remarks>
        /// <param name="rootTransform"></param>
        private void InitChildItemSelector(Transform rootTransform)
        {
            for (int i = 0; i < rootTransform.childCount; ++i)
            {
                Transform child = rootTransform.GetChild(i);
                if (child.TryGetComponent<UIItemSelector>(out var selector))
                {
                    Type type = Type.GetType(selector.SelectClass);
                    this.AddUIItemToGameObject(selector, type);
                }
                else
                {
                    this.InitChildItemSelector(child);
                }
            }
        }

        private void InitUpdateLogic()
        {
            if (this is IUIUpdater updater)
            {
                if (this.updaterCTS != null)
                {
                    this.updaterCTS.Cancel();
                    this.updaterCTS.Dispose();
                    this.updaterCTS = null;
                }

                this.updaterCTS = new CancellationTokenSource();
                STask.UpdateTask(updater.OnUpdate, PlayerLoopTiming.Update, this.updaterCTS.Token);
            }
        }
        #endregion

        #region 外部接口
        /// <summary>
        /// 创建新Item及其GameObject
        /// </summary>
        /// <param name="itemAssetName"></param>
        /// <param name="parent"></param>
        /// <typeparam name="TUIItem"></typeparam>
        /// <returns></returns>
        protected async STask<TUIItem> CreateChildItemAsync<TUIItem>(string itemAssetName, Transform parent = null)
            where TUIItem : UIItemBase, new()
        {
            UIItemInfo info = this.UIManager.GetUIItemInfo(itemAssetName);
            if (info == null)
                return default;
            if (this.gameObject == null)
                return default;

            if (parent == null)
                parent = this.VisualRoot;
            string assetName = itemAssetName;
            string assetBundleName = info.UIItemAssetBundleName;
            string className = info.UIItemClassName;

            GameObject prefabObj =
                await AssetBundleManager.LoadAssetInAssetBundleAsync<GameObject>(assetName, assetBundleName);
            prefabObj = UnityEngine.Object.Instantiate(prefabObj, parent);
            prefabObj.transform.localScale = Vector3.one;
            prefabObj.transform.localPosition = Vector3.zero;
            if (!prefabObj.TryGetComponent<UIItemSelector>(out var selector))
                selector = prefabObj.AddComponent<UIItemSelector>();

            selector.SelectClass = className;
            Type itemType = Type.GetType(selector.SelectClass);
            if (itemType == null)
                return default;

            var itemObj = Activator.CreateInstance<TUIItem>();
            this.childrenUIList.Add(itemObj, prefabObj);
            await itemObj.AwakeAsync(prefabObj);
            itemObj.BundleName = assetBundleName;
            return itemObj;
        }

        /// <summary>
        /// 给GameObject添加UIItem脚本，若GameObject上已有需添加的脚本则直接返回
        /// </summary>
        /// <param name="go"></param>
        /// <typeparam name="TUIItem"></typeparam>
        /// <returns></returns>
        public async STask<TUIItem> AddUIItemToGameObjectAsync<TUIItem>(GameObject go) where TUIItem : UIItemBase, new()
        {
            foreach (var pair in this.childrenUIList)
            {
                if (pair.Value == go)
                    return pair.Key as TUIItem;
            }

            TUIItem itemObj = Activator.CreateInstance<TUIItem>();
            this.childrenUIList.Add(itemObj, go);
            await itemObj.AwakeAsync(go);
            return itemObj;
        }

        /// <summary>
        /// 手动释放UIItem脚本及其GameObject
        /// </summary>
        /// <param name="item"></param>
        /// <typeparam name="TUIItem"></typeparam>
        protected async STask DisposeChildItemAsync<TUIItem>(TUIItem item) where TUIItem : UIItemBase, new()
        {
            this.RemoveChild(item);
            await item.DisposeAsync();
        }
        
        public bool RemoveChild(UIItemBase item)
        {
            return this.childrenUIList.Remove(item);
        }
        #endregion

        #region 生命周期方法
        /// <summary>
        /// 因UIViewBase中Awake需要异步等待创建MaskView，这里提供异步版的Awake方法供View重写，为了统一接口，全用异步版
        /// </summary>
        /// <param name="gameObjectHost"></param>
        public virtual async STask AwakeAsync(GameObject gameObjectHost)
        {
            this.ClassType = this.GetType();
            this.gameObject = gameObjectHost;
            this.rc = this.gameObject.GetComponent<ReferenceCollector>();

            this.InitChildItemSelector(this.gameObject.transform);
            this.InitCustomAttribute();

            this.OnAwake();
        }

        /// <summary>
        /// 释放UI对象
        /// </summary>
        public virtual async STask DisposeAsync()
        {
            if (this.UIState == EnumViewState.Disposed)
                return;
            this.UIState = EnumViewState.Disposed;

            if (this.updaterCTS != null)
            {
                this.updaterCTS.Cancel();
                this.updaterCTS.Dispose();
                this.updaterCTS = null;
            }

            if (this.gameObject != null && !this.gameObject.Equals(null))
                GameObject.Destroy(this.gameObject);

            await this.UIManager.DisposeUIBundleAsync(this);

            foreach (var uiObj in this.childrenUIList.Keys)
            {
                if(uiObj == this)
                    continue;
                await uiObj.DisposeAsync();
            }
            
            this.OnDispose();
        }

        public virtual async STask ShowAsync() { }

        public virtual async STask HideAsync() { }
        
        /// <summary> 首次创建时触发，用户可使用 </summary>
        protected virtual void OnAwake() { }

        /// <summary> 每次显示后触发，用户可使用 </summary>
        protected virtual void OnShow() { }
        
        /// <summary> 每次隐藏后触发，用户可使用 </summary>
        protected virtual void OnHide() { }

        /// <summary> 调用<see cref="DisposeAsync"/>后触发，用户可使用 </summary>
        protected virtual void OnDispose() { }
        
        /// <summary> 每次显示后调用，可添加一些每次显示需要处理的内部逻辑，底层使用，不暴露给用户 </summary>
        protected virtual void OnEnable()
        {
            this.OnShow();
            this.InitUpdateLogic();
        }
        
        /// <summary> 每次隐藏后调用，可添加一些每次隐藏需要处理的内部逻辑,底层使用，不暴露给用户 </summary>
        protected virtual void OnDisable()
        {
            //Remove something here，协程、计时器等
            this.updaterCTS?.Cancel();
            this.OnHide();
        }
        #endregion
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
    public interface IUIUpdater
    {
        void OnUpdate();
    }

    public interface IUIPrepareShow
    {
        STask OnPrepareShow();
    }
}