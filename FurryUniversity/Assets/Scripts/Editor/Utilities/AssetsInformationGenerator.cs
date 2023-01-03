using SFramework.Core.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace SFramework.Utilities.Editor
{
    public class AssetsInformationGenerator
    {
        #region 生成图集信息
        private static readonly Regex chineseRX = new Regex("^[\u4e00-\u9fa5]$");
        private static readonly HashSet<string> repeatedCheckSet = new HashSet<string>();

#if UNITY_EDITOR
        public static void GenerateSpriteAtlasInfo()
        {
            EditorHelper.CreateFolder(StaticVariables.SpriteAtlasInfoPath);

            GenerateAtlasInfo();
        }
#endif

        private static readonly string FileName = "UISpriteManager.g.cs";
        private static readonly string Placeholder = "{Placeholder}";
        private static readonly string MainBody =
            $"using System.Collections.Generic;{Environment.NewLine}"+
            $"namespace SFramework.Core.GameManagers{Environment.NewLine}" +
             "{" +
            $"{Environment.NewLine}" +
            $"   public static partial class UISpriteManager{Environment.NewLine}" +
            "   {" +
                $"{Environment.NewLine}" +
                $"{Placeholder}" +
            "   }" +
            $"{Environment.NewLine}" +
             "}";

        private static void GenerateAtlasInfo()
        {
            repeatedCheckSet.Clear();

            var spriteDirectories = Directory.GetDirectories(StaticVariables.UISpritesPath).Select(path => new DirectoryInfo(path));
            StringBuilder constructorWorkSB = new StringBuilder();
            StringBuilder constructorSB = new StringBuilder();
            constructorSB.AppendLine("      static UISpriteManager()");
            constructorSB.AppendLine("      {");
            constructorSB.AppendLine("          atlasSprite = new Dictionary<string, string>();");
            constructorSB.AppendLine("{constructorPlaceHolder}");
            constructorSB.AppendLine("      }");
            constructorSB.AppendLine();

            StringBuilder singleAtlasSpriteObjectWorkSB = new StringBuilder();
            StringBuilder singleAtlasSpriteObjectSB = new StringBuilder();
            StringBuilder totalAtlasSpriteObjectSB = new StringBuilder();

            foreach(var spriteDirectory in spriteDirectories)
            {
                singleAtlasSpriteObjectWorkSB.Clear();
                singleAtlasSpriteObjectSB.Clear();
                singleAtlasSpriteObjectSB.AppendLine($"       public class {spriteDirectory.Name} : UIAtlasSpritesObject");
                singleAtlasSpriteObjectSB.AppendLine("        {");
                singleAtlasSpriteObjectSB.AppendLine("{atlasSpriteObjectPlaceHolder}");
                singleAtlasSpriteObjectSB.AppendLine("        }");

                var files = spriteDirectory.GetFiles();
                foreach(var file in files)
                {
                    if (Path.GetExtension(file.Name) == ".meta")
                        continue;
                    string atlasBundleName = spriteDirectory.Name;
                    string spriteName = Path.GetFileNameWithoutExtension(file.Name);

                    //检查是否命名是否重复
                    if (repeatedCheckSet.Contains(spriteName))
                    {
                        EditorUtility.DisplayDialog("ん?", $"发现重复命名图片：{spriteName}，图集信息生成打断", "我知错了");
                        return;
                    }
                    repeatedCheckSet.Add(spriteName);

                    //检查命名
                    int charIndex = 0;
                    foreach (var nameChar in spriteName)
                    {
                        if (!char.IsLetterOrDigit(nameChar) && nameChar != '_')
                        {
                            EditorUtility.DisplayDialog("ん?", $"发现非法命名图片：{spriteName}，图集信息生成打断", "我知错了");
                            return;
                        }

                        if (chineseRX.IsMatch(spriteName.Substring(charIndex, 1)))
                        {
                            EditorUtility.DisplayDialog("ん?", $"发现中文命名图片：{spriteName}，图集信息生成打断", "我知错了");
                            return;
                        }

                        charIndex++;
                    }

                    //atlasSprite Dictionary
                    constructorWorkSB.AppendLine($"        atlasSprite.Add(\"{spriteName}\",\"{atlasBundleName}\");");

                    //UIAtlasSpritesObject
                    singleAtlasSpriteObjectWorkSB.AppendLine($"            public static readonly string {spriteName} = \"{spriteName}\";");
                }
                totalAtlasSpriteObjectSB.AppendLine(singleAtlasSpriteObjectSB.ToString().Replace("{atlasSpriteObjectPlaceHolder}", singleAtlasSpriteObjectWorkSB.ToString()));
            }
            string constuctorInfo = constructorSB.ToString().Replace("{constructorPlaceHolder}", constructorWorkSB.ToString());



            File.WriteAllText(StaticVariables.SpriteAtlasInfoPath.GetFullPath() + $"/{FileName}", MainBody.Replace(Placeholder, constuctorInfo.ToString() + totalAtlasSpriteObjectSB.ToString()), Encoding.UTF8);
            AssetDatabase.Refresh();
        }
        #endregion

        #region 生成音效信息
        public static void GenerateAudioAssetListCode()
        {
            GenerateAudioAssetList();
        }

        private static void GenerateAudioAssetList()
        {
            Dictionary<HashSet<string>, AudioLoader.AudioAssetType> audioAssetInfo = new Dictionary<HashSet<string>, AudioLoader.AudioAssetType>();

            //收集BGM资源
            var bgmSet = new HashSet<string>();
            audioAssetInfo.Add(bgmSet, AudioLoader.AudioAssetType.BGM);
            string path = StaticVariables.AudioBGMPath;
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (var file in dir.GetFiles())
            {
                if (file.Extension == ".meta")
                    continue;
                string bgmName = Path.GetFileNameWithoutExtension(file.FullName);
                bgmSet.Add(bgmName);

                string assetDataPath = file.FullName.GetRelativePath();
                var importer = AssetImporter.GetAtPath(assetDataPath);
                if (importer != null)
                {
                    importer.assetBundleName = $"{bgmName}{StaticVariables.AudioBGMBundleExtension}".ToLower();
                    importer.assetBundleVariant = StaticVariables.AssetBundlesFileExtensionWithoutDot;
                }
            }

            //收集CommonSFX资源
            var commonSFXSet = new HashSet<string>();
            audioAssetInfo.Add(commonSFXSet, AudioLoader.AudioAssetType.CommonSFX);
            path = StaticVariables.AudioCommonSFXPath;
            dir = new DirectoryInfo(path);
            foreach (var file in dir.GetFiles())
            {
                if (file.Extension == ".meta")
                    continue;
                string commonSFXName = Path.GetFileNameWithoutExtension(file.FullName);
                commonSFXSet.Add(commonSFXName);

                string assetDataPath = file.FullName.GetRelativePath();
                var importer = AssetImporter.GetAtPath(assetDataPath);
                if (importer != null)
                {
                    importer.assetBundleName = $"{StaticVariables.AudioCommonSFXBundleName}".ToLower();
                    importer.assetBundleVariant = StaticVariables.AssetBundlesFileExtensionWithoutDot;
                }
            }

            //收集SFXGroup资源
            Dictionary<HashSet<string>, string> sfxGroupInfo = new Dictionary<HashSet<string>, string>();//value为Bundle Name，生成的代码也是
            path = StaticVariables.AudioSFXGroupPath;
            dir = new DirectoryInfo(path);
            foreach (var groupDir in dir.GetDirectories())
            {
                var groupAudioAssets = new HashSet<string>();
                string bundleName = $"{groupDir.Name}{StaticVariables.AudioSFXGroupBundleExtension}".ToLower();
                sfxGroupInfo.Add(groupAudioAssets, bundleName);

                foreach (var file in groupDir.GetFiles())
                {
                    if (file.Extension == ".meta")
                        continue;
                    string assetName = Path.GetFileNameWithoutExtension(file.FullName);
                    groupAudioAssets.Add(assetName);

                    string assetDataPath = file.FullName.GetRelativePath();
                    var importer = AssetImporter.GetAtPath(assetDataPath);
                    if (importer.assetBundleName != bundleName)
                    {
                        importer.assetBundleName = bundleName;
                        importer.assetBundleVariant = StaticVariables.AssetBundlesFileExtensionWithoutDot;
                    }
                }
            }

            //生成代码
            StringBuilder sb = new StringBuilder();
            HashSet<string> temp = new HashSet<string>();//用于检查同名资源，同样类型的音效不能重名
            HashSet<string> repeatedAssetName = new HashSet<string>();//有同名音效时，add it
            foreach (string assetName in bgmSet)
            {
                sb.AppendLine($"            {(temp.Contains(assetName) ? "//" : string.Empty)}audioAssetInfo.Add(\"{assetName}\",AudioLoader.AudioAssetType.BGM);");
                if (!temp.Add(assetName))
                    repeatedAssetName.Add(assetName);
            }
            foreach (string assetName in commonSFXSet)
            {
                sb.AppendLine($"            {(temp.Contains(assetName) ? "//" : string.Empty)}audioAssetInfo.Add(\"{assetName}\",AudioLoader.AudioAssetType.CommonSFX);");
                if (!temp.Add(assetName))
                    repeatedAssetName.Add(assetName);
            }

            temp.Clear();
            foreach (var item in sfxGroupInfo)
            {
                string groupName = item.Value;
                foreach (var assetName in item.Key)
                {
                    sb.AppendLine($"            {(temp.Contains(assetName) ? "//" : string.Empty)}sfxGroupAssetInfo.Add(\"{assetName}\",\"{groupName}\");");
                    if (!temp.Add(assetName))
                        repeatedAssetName.Add(assetName);
                }
            }

            if (repeatedAssetName.Count > 0)
            {
                StringBuilder errorSB = new StringBuilder();
                foreach (var assetName in repeatedAssetName)
                {
                    errorSB.AppendLine(assetName);
                }
                EditorUtility.DisplayDialog("生成音效资源清单错误", "具有重复的音效资源:\n" + errorSB.ToString(), "OK");
            }

            string templateCode = AudioCodeTemplate.Replace("[ADD]", $"{sb.ToString()}");
            File.WriteAllText(StaticVariables.AudioGenerateCodeFileFullName, templateCode);
        }

        private static readonly string AudioCodeTemplate =
@"using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.Audio
{
    public static class AudioAssetList
    {
        public static Dictionary<string, AudioLoader.AudioAssetType> audioAssetInfo = new Dictionary<string, AudioLoader.AudioAssetType>();
        public static Dictionary<string, string> sfxGroupAssetInfo = new Dictionary<string, string>();
        public static void Init()
        {
[ADD]
        }
    }
}";
        #endregion
    }
}