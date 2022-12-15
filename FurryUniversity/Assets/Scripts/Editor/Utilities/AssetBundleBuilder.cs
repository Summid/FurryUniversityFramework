using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SFramework.Utilities.Editor
{
    public static class AssetBundleBuilder
    {
        //打包前需将temp目录下的资源删除
        private static string tempFolder => Path.Combine(StaticVariables.StreamingAssetsPath, StaticVariables.AssetBundleEditorTemp,
            StaticVariables.Platform.ToString(), StaticVariables.AssetBundlesFolderName).ConvertWindowsSeparatorToUnity().GetRelativePath();
        private static string spriteAtlasExtension = ".spriteatlas";

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
            if (AssetDatabase.IsValidFolder(StaticVariables.UISpriteAtalasesPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(StaticVariables.UISpriteAtalasesPath.GetFullPath());
                FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
                int index = 1;
                foreach (var file in fileSystemInfos)
                {
                    EditorUtility.DisplayProgressBar("Update AssetBundle Tags", $"{file.Name}", index++ / fileSystemInfos.Length);
                    if (Path.GetExtension(file.Name) != spriteAtlasExtension)
                        continue;
                    var importer = AssetImporter.GetAtPath(file.FullName.GetRelativePath());
                    if (importer != null)
                    {
                        importer.assetBundleName = file.Name.ToLower();
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
                    EditorHelper.ResetFolder(tempFolder.ConvertWindowsSeparatorToUnity());
                    options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
                }
                else
                {
                    EditorHelper.CreateFolder(tempFolder.ConvertWindowsSeparatorToUnity());
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

            EditorHelper.ResetFolder(targetFolder,true);

            DirectoryInfo directoryInfo = new DirectoryInfo(sourceFolder);
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            int index = 1;
            foreach(FileInfo file in fileInfos)
            {
                if (file.Extension == ".manifest" && file.Name != "AssetBundles.manifest")
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar("Copy Asset Bundles...", targetFolder, index / fileInfos.Length);

                string sourceFilePath = Path.Combine(sourceFolder, file.Name).ConvertWindowsSeparatorToUnity();
                string targetFilePath = Path.Combine(targetFolder, file.Name).ConvertWindowsSeparatorToUnity().GetRelativePath();
                AssetDatabase.CopyAsset(sourceFilePath, targetFilePath);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }
}