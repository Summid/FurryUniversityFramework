using SFramework.Core.GameManagers;
using SFramework.Core.UI;
using SFramework.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SFramework.Utilities.Archive
{
    /// <summary>
    /// 保存存档时通过该接口收集存档信息，由Manager和View继承；如果多个接口写入同一个字段，后面写入的值将覆盖之前的
    /// </summary>
    public interface ISavable
    {
        ArchiveObject OnSave();
    }
    /// <summary>
    /// 加载存档时通过该接口读取存档信息，由Manager和View继承
    /// </summary>
    public interface ILoadable
    {
        void OnLoad(ArchiveObject archiveObject);
    }
    
    public interface IArchiveManager
    {
        void SaveArchive();
        void LoadArchive();
    }

    public class BinaryArchiveManager : IArchiveManager
    {
        public void SaveArchive()
        {
            throw new System.NotImplementedException();
        }

        public void LoadArchive()
        {
            throw new System.NotImplementedException();
        }
    }
    
    public static class SaveMaster
    {
        private static IArchiveManager currentArchiveManager;
        public static IArchiveManager CurrentArchiveManager
        {
            get
            {
                return currentArchiveManager ??= new BinaryArchiveManager();//TODO 用配置决定使用的存档管理器
            }
        }

        private static bool isInit = false;
        public static bool IsInit => isInit;
        // <typeof(ISavable/ILoadable), {InstanceType}>
        private static Dictionary<Type, List<Type>> caches = new Dictionary<Type, List<Type>>();

        private static readonly Type iSavableType = typeof(ISavable);
        private static readonly Type iLoadableType = typeof(ILoadable);
        private static readonly Type uiViewType = typeof(UIViewBase);
        private static readonly Type managerType = typeof(GameManagerBase);
        
        public static void Init()
        {
            if (isInit) return;

            if (!caches.ContainsKey(iSavableType))
            {
                caches.Add(iSavableType, new List<Type>());
            }
            
            if (!caches.ContainsKey(iLoadableType))
            {
                caches.Add(iLoadableType, new List<Type>());
            }
            
            var savableTypes = typeof(ISavable).GetInterfaceTypesInAssemblies();
            if (savableTypes.Any())
            {
                var list = caches[iSavableType];
                list.AddRange(savableTypes);
            }

            var loadableTypes = typeof(ILoadable).GetInterfaceTypesInAssemblies();
            if (loadableTypes.Any())
            {
                var list = caches[iLoadableType];
                list.AddRange(loadableTypes);
            }

            isInit = true;
        }

        public static async STask Save()
        {
            if (!caches.TryGetValue(iSavableType, out List<Type> list))
            {
                return;
            }

            ArchiveObject archiveObject = null;
            foreach (Type type in list)
            {
                await STask.NextFrame();
                object result = null;
                if (type.IsSubclassOf(uiViewType))
                {
                    UIManager.UIInstanceInfo uiInfo = GameManager.Instance.UIManager.GetShowingUI(type);
                    if (uiInfo != null)
                    {
                        //ui显示中
                        result = type.GetMethod("OnSave")?.Invoke(uiInfo.ViewInstance, null);
                    }
                }
                else if (type.IsSubclassOf(managerType))
                {
                    GameManagerBase manager = GameManager.Instance.GetManager(type);
                    if (manager != null)
                    {
                        result = type.GetMethod("OnSave")?.Invoke(manager, null);
                    }
                }

                if (result is ArchiveObject resultAO)
                {
                    archiveObject = archiveObject == null ? resultAO : archiveObject.Merge(resultAO);
                }
            }
        }

        public static ArchiveObject Load()
        {
            return null;
        } 
    }
}