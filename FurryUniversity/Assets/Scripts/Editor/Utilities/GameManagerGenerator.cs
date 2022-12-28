using SFramework.Core.GameManagers;
using System;
using System.IO;
using System.Text;
using UnityEditor;

namespace SFramework.Utilities.Editor
{
    public class GameManagerGenerator
    {
#if UNITY_EDITOR
        [MenuItem("Tools/GenerateGameManagerCode")]
        [InitializeOnLoadMethod]
        private static void GenerateGameMangerCode()
        {
            EditorHelper.CreateFolder(StaticVariables.Assets, StaticVariables.Scripts);
            EditorHelper.CreateFolder(StaticVariables.ScriptsPath, StaticVariables.GameManagers);
            EditorHelper.CreateFolder(StaticVariables.GameManagersPath, StaticVariables.Gen);

            Generate();
        }
#endif
        private static readonly string FileName = "GameManager.g.cs";
        private static readonly string Placeholder = "{Placeholder}";
        private static readonly string MainBody =
            $"namespace SFramework.Core.GameManagers{Environment.NewLine}" +
            "{" +
            $"{Environment.NewLine}" +
            $"   public partial class GameManager{Environment.NewLine}" +
            "   {" +
                $"{Environment.NewLine}" +
                $"{Placeholder}" +
            "   }" +
           $"{Environment.NewLine}" +
            "}";


        private static void Generate()
        {
            var managerTypes = typeof(GameManagerBase).GetSubTypesInAssemblies();
            StringBuilder sb = new StringBuilder();
            foreach (var managerType in managerTypes)
            {
                string typeName = managerType.Name;
                string typeNameLowerCase = typeName.ToLower();
                sb.AppendLine($"        private {typeName} {typeNameLowerCase};");
                sb.AppendLine($"        public {typeName} {typeName}");
                sb.AppendLine("        {");
                sb.AppendLine("             get");
                sb.AppendLine("             {");
                sb.AppendLine($"                if(this.{typeNameLowerCase} == null)");
                sb.AppendLine($"                    this.{typeNameLowerCase} = this.GetManager<{typeName}>(typeof({typeName}));");
                sb.AppendLine($"                return this.{typeNameLowerCase};");
                sb.AppendLine("             }");
                sb.AppendLine("        }");
            }
            string finalText = MainBody.Replace(Placeholder, sb.ToString());
            File.WriteAllText(StaticVariables.GameManagersGenPath.GetFullPath() + $"/{FileName}", finalText, Encoding.UTF8);
            AssetDatabase.Refresh();
        }
    }
}