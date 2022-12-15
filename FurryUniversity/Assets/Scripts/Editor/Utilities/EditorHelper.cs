using System.Text;
using UnityEditor;
using UnityEngine;
using static SFramework.Utilities.StaticVariables;

namespace SFramework.Utilities.Editor
{
    public static class EditorHelper
    {
        #region Utility Methods
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
            string[] folders = path.Split("/");

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