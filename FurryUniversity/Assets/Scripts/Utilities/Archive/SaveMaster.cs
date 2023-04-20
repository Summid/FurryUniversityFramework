using SFramework.Core.GameManagers;
using SFramework.Core.UI;
using SFramework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        void SaveArchive(Archive archive);
        Archive LoadArchive();
        string Extension { get; }
    }
    
    public class BinaryArchiveManager : IArchiveManager
    {
        public string Extension => ".binary";
        
        public void SaveArchive(Archive archive)
        {
            if (archive == null)
                return;

            string archivePath = StaticVariables.ArchivePath;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fileStream = File.Create($"{archivePath}/{StaticVariables.ArchiveName}{this.Extension}"))
            {
                binaryFormatter.Serialize(fileStream, archive);
            }
        }

        public Archive LoadArchive()
        {
            string archiveFullPath = $"{StaticVariables.ArchivePath}/{StaticVariables.ArchiveName}{this.Extension}";
            if (!File.Exists(archiveFullPath))
                return null;
            
            Archive archive = null;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fileStream = File.Open(archiveFullPath, FileMode.Open))
            {
                archive = binaryFormatter.Deserialize(fileStream) as Archive;
            }
            return archive;
        }
    }

    public class JSONArchiveManager : IArchiveManager
    {
        public string Extension => ".json";
        //TODO 加密
        public void SaveArchive(Archive archive)
        {
            if (archive == null)
                return;

            string archivePath = StaticVariables.ArchivePath;
            using (StreamWriter streamWriter = new StreamWriter($"{archivePath}/{StaticVariables.ArchiveName}{this.Extension}"))
            {
                string jsonString = JsonUtility.ToJson(archive, true);
                streamWriter.Write(jsonString);
            }
        }

        public Archive LoadArchive()
        {
            string archiveFullPath = $"{StaticVariables.ArchivePath}/{StaticVariables.ArchiveName}{this.Extension}";
            if (!File.Exists(archiveFullPath))
                return null;

            string jsonString = string.Empty;
            using (StreamReader streamReader = new StreamReader(archiveFullPath))
            {
                jsonString = streamReader.ReadToEnd();
            }

            if (string.IsNullOrEmpty(jsonString))
                return null;

            Archive archive = JsonUtility.FromJson<Archive>(jsonString);
            return archive;
        }
    }

    public static class SaveMaster
    {
        private static IArchiveManager currentArchiveManager;
        public static IArchiveManager CurrentArchiveManager
        {
            get
            {
                switch(GameManager.Instance.GameSettings.ArchiveType)
                {
                    case Core.GameManagers.Init.ArchiveTypeEnum.Binary:
                        return currentArchiveManager ??= new BinaryArchiveManager();
                    case Core.GameManagers.Init.ArchiveTypeEnum.JSON:
                        return currentArchiveManager ??= new JSONArchiveManager();
                    default:
                        throw new ArgumentOutOfRangeException();
                }

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

        private static Archive archive;
        public static Archive Archive => archive;
        
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

            string archivePath = StaticVariables.ArchivePath;
            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
            }

            if (!File.Exists($"{archivePath}/{StaticVariables.ArchiveName}{CurrentArchiveManager.Extension}"))
            {
                Debug.LogWarning($"no archive file, init default one");
                archive = new Archive();
                archive.ArchiveObjects = new List<ArchiveObject>();
                for (int i = 0; i < GameManager.Instance.GameSettings.ArchiveCount; ++i)
                {
                    archive.ArchiveObjects.Add(new ArchiveObject(i));
                }
                CurrentArchiveManager.SaveArchive(archive);
            }
            else
            {
                archive = CurrentArchiveManager.LoadArchive();
            }
            
            isInit = true;
        }

        /// <summary>
        /// 保存存档
        /// </summary>
        /// <param name="index">存档索引位置</param>
        public static async STask Save(int index = 0)
        {
            if (!caches.TryGetValue(iSavableType, out List<Type> list) || index >= archive.ArchiveObjects.Count)
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
            
            if (archiveObject != null)
            {
                archive.ArchiveObjects[index] = archiveObject;
                CurrentArchiveManager.SaveArchive(archive);
            }
        }

        public static ArchiveObject Load(int index)
        {
            return index >= archive.ArchiveObjects.Count ? null : archive.ArchiveObjects[index];
        } 
    }
}