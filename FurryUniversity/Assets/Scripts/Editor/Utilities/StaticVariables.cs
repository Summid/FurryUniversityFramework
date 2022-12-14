using System.Text;
using UnityEditor;
using UnityEngine;

namespace SFramework.Utilities
{
    public static class StaticVariables
    {
        public static readonly string Assets = "Assets";
        public static readonly string Res = "Res";
        public static readonly string Bundles = "Bundles";

        public static readonly string ResPath = $"{Assets}/{Res}";
        public static readonly string BundlesPath = $"{Assets}/{Bundles}";

        public static readonly string SpritesPath = $"{ResPath}/Sprites";
        public static readonly string UISpritesPath = $"{SpritesPath}/UI";
        public static readonly string UISpriteAtalasesPath = $"{BundlesPath}/SpriteAtlases";

        public static readonly string Scripts = "Scripts";
        public static readonly string GameManagers = "GameManagers";
        public static readonly string Gen = "Gen";
        public static readonly string ScriptsPath = $"{Assets}/{Scripts}";
        public static readonly string GameManagersPath = $"{ScriptsPath}/{GameManagers}";
        public static readonly string GameManagersGenPath = $"{GameManagersPath}/{Gen}";

        public static readonly string AssetBundlesFolderName = "AssetBundles";
        public static readonly string AssetBundlesFileExtension = ".bundle";
        public static readonly string AssetBundlesFileExtensionWithoutDot = "bundle";
        public static readonly string AssetBundleEditorTemp = "AssetBundleEditorTemp";

        public static RuntimePlatform Platform
        {
            get
            {
#if UNITY_STANDALONE_WIN
                return RuntimePlatform.WindowsPlayer;
#elif UNITY_STANDALONE_OSX
                return RuntimePlatform.OSXPlayer;
#elif UNITY_ANDROID
                return RuntimePlatform.Android;
#elif UNITY_IOS
                return RuntimePlatform.IPhonePlayer;
#endif
            }
        }

        public static string StreamingAssetsPath
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                var t = Application.streamingAssetsPath;
#elif UNITY_ANDROID
                var t = $"{Application.dataPath}!assets";
#elif UNITY_IOS
                var t = $"{Application.dataPath}/Raw";
#endif
                return t;
            }
        }

        #region Utility Methods
        public static void CreateFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            {
                return;
            }
            AssetDatabase.CreateFolder(path, folderName);
            AssetDatabase.Refresh();
        }

        public static void CreateFolder(string path)
        {
            string[] folders = path.Split("/");

            StringBuilder currentPath = new StringBuilder($"{Assets}");
            foreach (string folder in folders)
            {
                if (folder == Assets)
                    continue;
                CreateFolder(currentPath.ToString(), folder);
                currentPath.Append($"/{folder}");
            }
            AssetDatabase.Refresh();
        }

        public static void DeleteFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
                return;
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();
        }

        public static void ResetFolder(string path)
        {
            if (!path.StartsWith(Assets))
            {
                path = path.GetRelativePath();
            }
            if (AssetDatabase.IsValidFolder(path))
            {
                DeleteFolder(path);
            }
            CreateFolder(path);
            AssetDatabase.Refresh();
        }

        public static void CreateUISpriteAtlasesPath()
        {
            CreateFolder($"{Assets}", $"{Bundles}");
            CreateFolder($"{BundlesPath}", "SpriteAtlases");
        }

        /// <summary>
        /// 获取相对路径的全路径
        /// </summary>
        /// <param name="folderPath">Assets/下的相对路径</param>
        /// <returns></returns>
        public static string GetFullPath(this string folderPath, bool checkExist = true)
        {
            if (!folderPath.StartsWith(Assets))
            {
                Debug.LogError($"只支持Assets开头的相对路径，当前路径：{folderPath}");
                return string.Empty;
            }

            if (checkExist && !AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"当前目录不存在，当前目录：{folderPath}");
                return string.Empty;
            }


            return $"{Application.dataPath}/{folderPath.Replace("Assets/", "")}";
        }

        /// <summary>
        /// 获取全路径的相对路径部分
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static string GetRelativePath(this string folderPath)
        {
            return Assets + folderPath.Substring(Application.dataPath.Length);
        }

        public static string ConvertWindowsSeparatorToUnity(this string path)
        {
            return path.Replace(@"\\", "/").Replace(@"\", "/");
        }
        #endregion

    }
}