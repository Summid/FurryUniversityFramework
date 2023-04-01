using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SFramework.Utilities.Archive
{
    public interface ISavable
    {
        ArchiveObject OnSave();
    }

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

        public static void Save()
        {
            var types = typeof(ISavable).GetInterfaceTypesInAssemblies();
            Debug.Log($"types count : {types.Count()}");
            foreach (var type in types)
            {
                //TODO 改用通用的Manager，在Init时将接口对象保存到dic中，调用接口方法时通过UIManager或GameManger的接口获取调用对象
                // type.GetMethod("OnSave").Invoke();
                Debug.Log(type);
            }
        }

        public static ArchiveObject Load()
        {
            return null;
        } 
    }
}