using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SFramework.Utilities.Editor
{
    public class AssetsInformationGenerator
    {
#if UNITY_EDITOR
        [MenuItem("Tools/生成图集清单")]
        private static void GenerateSpriteAtlasInfo()
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