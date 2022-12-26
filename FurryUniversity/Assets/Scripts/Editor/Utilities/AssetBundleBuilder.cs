using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SFramework.Utilities.Editor
{
    public static class AssetBundleBuilder
    {
        private static string tempFolder => Path.Combine(StaticVariables.AssetBundleEditorTemp,
            StaticVariables.Platform.ToString(), StaticVariables.AssetBundlesFolderName);

        [MenuItem("AssetOperation/增量更新AssetBundles")]
        public static void UpdateAssetBundles()
        {
            UpdateAssetBundleTags();
            Update(false);
        }

        [MenuItem("AssetOperation/全量更新AssetBundles")]
        public static void FullyUpdateAssetBundles()
        {
            UpdateAssetBundleTags();
            Update(true);
        }

        public static void UpdateAssetBundleTags()
        {

            UpdateAssetBundleTagsHandler(StaticVariables.UISpriteAtalasesPath, StaticVariables.SpriteAtlasExtension, StaticVariables.SpriteAtlasBundleExtension);
            UpdateAssetBundleTagsHandler(StaticVariables.UIViewPrefabsPath, StaticVariables.PrefabExtension, StaticVariables.UIViewBundleExtension);
            UpdateAssetBundleTagsHandler(StaticVariables.UIItemPrefabsPath, StaticVariables.PrefabExtension, StaticVariables.UIItemBundleExtension);
        }

        private static void UpdateAssetBundleTagsHandler(string path, string assetExtension, string bundleExtension)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path.GetFullPath());
                FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
                int index = 1;
                foreach (var file in fileSystemInfos)
                {
                    EditorUtility.DisplayProgressBar("Update AssetBundle Tags", $"{file.Name}", index++ / fileSystemInfos.Length);
                    if (Path.GetExtension(file.Name) != assetExtension)
                        continue;
                    var importer = AssetImporter.GetAtPath(file.FullName.GetRelativePath());
                    if (importer != null)
                    {
                        importer.assetBundleName = Path.GetFileNameWithoutExtension(file.Name).ToLower() + bundleExtension;
                        importer.assetBundleVariant = StaticVariables.AssetBundlesFileExtensionWithoutDot;
                    }
                }
                EditorUtility.ClearProgressBar();
            }
        }

        private static void Update(bool isFully)
        {
            AssetDatabase.SaveAssets();

            try
            {
                EditorUtility.DisplayProgressBar("Hold On", $"Update Asset Bundles {tempFolder}", 0f);

                var options = BuildAssetBundleOptions.ChunkBasedCompression |
                    BuildAssetBundleOptions.StrictMode |
                    BuildAssetBundleOptions.DeterministicAssetBundle;
                if (isFully)
                {
                    EditorHelper.ResetFolderCSharp(tempFolder);
                    options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
                }
                else
                {
                    EditorHelper.PreparePathCSharp(tempFolder);
                }

                Debug.Log(options);
                Debug.Log(EditorUserBuildSettings.activeBuildTarget);
                var info = BuildPipeline.BuildAssetBundles(tempFolder, options, EditorUserBuildSettings.activeBuildTarget);
                if (info != null)
                {
                    Debug.Log("BuildAssetBundles return result: " + info);
                }
                else
                {
                    throw new Exception("资源包创建失败，查看Console报错");
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("AssetBundlesBuilder", e.ToString(), "OD");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Copy();
            AssetDatabase.Refresh();
        }

        private static void Copy()
        {
            string sourceFolder = tempFolder;
            string targetFolder = $"{StaticVariables.StreamingAssetsPath}/{StaticVariables.AssetBundlesFolderName}";

            EditorHelper.ResetFolder(targetFolder, true);

            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(sourceFolder);
                FileInfo[] fileInfos = directoryInfo.GetFiles();
                int index = 1;
                for (int i = 0; i < fileInfos.Length; ++i)
                {
                    var file = fileInfos[i];
                    if (file.Extension == ".manifest" && file.Name != "AssetBundles.manifest")
                    {
                        continue;
                    }

                    EditorUtility.DisplayProgressBar("Copy Asset Bundles...", targetFolder, index / fileInfos.Length);

                    file.CopyTo(Path.Combine(targetFolder, file.Name), false);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }
    }
}