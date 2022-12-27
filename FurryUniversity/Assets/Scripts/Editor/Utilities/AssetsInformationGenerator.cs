using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace SFramework.Utilities.Editor
{
    public class AssetsInformationGenerator
    {
        private static readonly Regex chineseRX = new Regex("^[\u4e00-\u9fa5]$");
        private static readonly HashSet<string> repeatedCheckSet = new HashSet<string>();

#if UNITY_EDITOR
        [MenuItem("Tools/生成图集清单")]
        public static void GenerateSpriteAtlasInfo()
        {
            EditorHelper.CreateFolder(StaticVariables.SpriteAtlasInfoPath);

            Generate();
        }
#endif

        private static readonly string FileName = "UISpriteManager.g.cs";
        private static readonly string Placeholder = "{Placeholder}";
        private static readonly string MainBody =
            $"using System.Collections.Generic;{Environment.NewLine}"+
            $"namespace SFramework.Core.GameManager{Environment.NewLine}" +
             "{" +
            $"{Environment.NewLine}" +
            $"   public static partial class UISpriteManager{Environment.NewLine}" +
            "   {" +
                $"{Environment.NewLine}" +
                $"{Placeholder}" +
            "   }" +
            $"{Environment.NewLine}" +
             "}";

        private static void Generate()
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
    }
}