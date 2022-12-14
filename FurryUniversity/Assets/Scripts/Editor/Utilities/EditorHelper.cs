using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using static SFramework.Utilities.StaticVariables;

namespace SFramework.Utilities.Editor
{
    public static class EditorHelper
    {
        #region Utility Unity Methods
        public static void CreateFolder(string path, string folderName, bool autoRefresh = false)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            {
                return;
            }
            AssetDatabase.CreateFolder(path, folderName);

            if (autoRefresh)
                AssetDatabase.Refresh();
        }

        public static void CreateFolder(string path, bool autoRefresh = false)
        {
            string[] folders = path.Split('/');

            StringBuilder currentPath = new StringBuilder($"{Assets}");
            foreach (string folder in folders)
            {
                if (folder == Assets)
                    continue;
                CreateFolder(currentPath.ToString(), folder);
                currentPath.Append($"/{folder}");
            }

            if (autoRefresh)
                AssetDatabase.Refresh();
        }

        public static void DeleteFolder(string path, bool autoRefresh = false)
        {
            if (!AssetDatabase.IsValidFolder(path))
                return;
            AssetDatabase.DeleteAsset(path);
            if (autoRefresh)
                AssetDatabase.Refresh();
        }

        public static void ResetFolder(string path, bool autoRefresh = false)
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
            if (autoRefresh)
                AssetDatabase.Refresh();
        }

        public static void CreateUISpriteAtlasesPath()
        {
            CreateFolder($"{Assets}", $"{Bundles}");
            CreateFolder($"{BundlesPath}", "SpriteAtlases");
        }

        /// <summary>
        /// ??????????????????????????????
        /// </summary>
        /// <param name="folderPath">Assets/??????????????????</param>
        /// <returns></returns>
        public static string GetFullPath(this string folderPath, bool checkExist = true)
        {
            if (!folderPath.StartsWith(Assets))
            {
                Debug.LogError($"?????????Assets???????????????????????????????????????{folderPath}");
                return string.Empty;
            }

            if (checkExist && !AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"???????????????????????????????????????{folderPath}");
                return string.Empty;
            }

            if (folderPath.Contains("Assets/"))
            {
                folderPath = folderPath.Replace("Assets/", "");
            }
            else if (folderPath.Contains("Assets\\"))
            {
                folderPath = folderPath.Replace("Assets\\", "");
            }
            return $"{Application.dataPath}/{folderPath}";
        }

        /// <summary>
        /// ????????????????????????????????????
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static string GetRelativePath(this string folderPath)
        {
            return (Assets + folderPath.Substring(Application.dataPath.Length)).Replace(@"\\", "/");
        }

        public static string ConvertWindowsSeparatorToUnity(this string path)
        {
            return path.Replace(@"\\", "/").Replace(@"\", "/");
        }
        #endregion

        #region Utility C# Method
        public static bool ValidateFolder(string path)
        {
            return !string.IsNullOrEmpty(path) && Directory.Exists(path);
        }

        public static void PreparePathCSharp(string path, bool isFolder = false)
        {
            string p = isFolder ? path : Path.GetDirectoryName(path);

            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }
        }

        public static void ResetFolderCSharp(string path, bool create = true)
        {
            if (ValidateFolder(path))
            {
                Directory.Delete(path, true);
            }

            if (create)
            {
                AssetDatabase.Refresh();
                Directory.CreateDirectory(path);
            }
        }
        #endregion
    }
}