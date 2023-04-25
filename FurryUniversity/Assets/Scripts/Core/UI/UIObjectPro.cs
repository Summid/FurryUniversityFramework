using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace SFramework.Core.UI
{
    public abstract class UIObjectPro
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
        private Dictionary<UIObjectPro, GameObject> childrenUIList = new Dictionary<UIObjectPro, GameObject>();
        public Dictionary<UIObjectPro, GameObject> ChildrenUIList => this.childrenUIList;
        private UIManagerPro UIManager { get { return GameManager.Instance.UIManagerPro; } }
        private CancellationTokenSource updaterCTS;

        #region 生命周期方法

        public virtual STask Dispose()
        {
            //TODO Dispose
            return STask.CompletedTask;
        }
        
        /// <summary> 首次创建时触发，用户可使用 </summary>
        protected virtual void OnAwake() { }

        /// <summary> 每次显示后触发，用户可使用 </summary>
        protected virtual void OnShow() { }
        
        /// <summary> 每次隐藏后触发，用户可使用 </summary>
        protected virtual void OnHide() { }

        /// <summary> 调用<see cref="Dispose"/>后触发，用户可使用 </summary>
        protected virtual void OnDispose() { }
        
        /// <summary> 每次显示后调用，可添加一些每次显示需要处理的内部逻辑，底层使用，不暴露给用户 </summary>
        protected virtual void OnEnable()
        {
            this.OnShow();
            //todo this.InitUpdateLogic();
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