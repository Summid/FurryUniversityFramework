using UnityEditor;
using UnityEngine;

namespace Utilities
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

        public static void CreateFolder(string path,string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            {
                return;
            }
            AssetDatabase.CreateFolder(path, folderName);
        }

        public static void CreateUISpriteAtlasesPath()
        {
            CreateFolder($"{Assets}", $"{Bundles}");
            CreateFolder($"{BundlesPath}","SpriteAtlases");
        }

        /// <summary>
        /// 获取相对路径的全路径
        /// </summary>
        /// <param name="folderPath">Assets/下的相对路径</param>
        /// <returns></returns>
        public static string GetFullPath(this string folderPath)
        {
            if (!folderPath.StartsWith(Assets))
            {
                Debug.LogError($"只支持Assets开头的相对路径，当前路径：{folderPath}");
                return string.Empty;
            }

            if (!AssetDatabase.IsValidFolder(folderPath))
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
    }
}