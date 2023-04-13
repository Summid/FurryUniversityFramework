using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace SFramework.Utilities.Editor
{
    public class SpriteAtlasesBuilder
    {
        private static Dictionary<string, SpriteAtlas> atlasInfos = new Dictionary<string, SpriteAtlas>();
#if UNITY_EDITOR

        [InitializeOnEnterPlayMode]
        private static void Initialize()
        {
            BuildAtlas();
        }
#endif

        [MenuItem("AssetOperation/BuildAtlases For Sprites")]
        public static void BuildAtlas()
        {
            atlasInfos.Clear();

            EditorHelper.CreateUISpriteAtlasesPath();
            InitializeAtlases();
            FindFiles();
            AssetDatabase.Refresh();
        }

        private static void InitializeAtlases()
        {
            DirectoryInfo atlasDirectoryInfo = new DirectoryInfo(EditorHelper.GetFullPath(StaticVariables.UISpriteAtlasesPath));
            FileSystemInfo[] atlases = atlasDirectoryInfo.GetFileSystemInfos();
            foreach (FileSystemInfo atlas in atlases)
            {
                if (Path.GetExtension(atlas.Name) != ".spriteatlas")
                    continue;
                string atlasName = Path.GetFileNameWithoutExtension(atlas.Name);
                if (!atlasInfos.ContainsKey(atlasName))
                {
                    string atlasRelativePath = atlas.FullName.GetRelativePath().ConvertWindowsSeparatorToUnity();
                    atlasInfos.Add(atlasName, AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasRelativePath));
                }
            }
        }

        /// <summary>
        /// 寻找所有存放UI Sprite的目录，并创建图集
        /// </summary>
        private static void FindFiles()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo($"{EditorHelper.GetFullPath(StaticVariables.UISpritesPath)}");
            //FileSystemInfo提供FileInfo和DirectoryInfo的共有方法，用于不确定操作对象为File还是Direcory时很有用
            FileSystemInfo[] files = directoryInfo.GetFileSystemInfos();//获取目录下所有文件和文件夹

            //以该目录下的直接子目录创建图集
            foreach (FileSystemInfo file in files)
            {
                if (Path.GetExtension(file.Name) == ".meta")
                {
                    continue;
                }

                if (file is DirectoryInfo)//是文件夹
                {
                    CreateAtlasFile(file.FullName);
                }
            }
        }

        public static void CreateAtlasFile(string folderFullPath)
        {
            //Debug.Log(folderFullPath);

            DirectoryInfo directoryInfo = new DirectoryInfo(folderFullPath);
            FileSystemInfo[] files = directoryInfo.GetFileSystemInfos();

            if (files.Length <= 0) return;//目录下没有图片，返回

            //创建图集
            string atlasName = Path.GetFileNameWithoutExtension(folderFullPath);
            if (!atlasInfos.ContainsKey(Path.GetFileNameWithoutExtension(atlasName)))
            {
                bool alpha = !atlasName.Contains("_NA");
                SpriteAtlas atlas = SetAtlasSettings(alpha);
                string finalPath = StaticVariables.UISpriteAtlasesPath + $"/{directoryInfo.Name}" + ".spriteatlas";
                AssetDatabase.CreateAsset(atlas, finalPath);
                atlasInfos.Add(atlasName, atlas);

                //设置图集图片
                string spritesRelativePath = folderFullPath.GetRelativePath().ConvertWindowsSeparatorToUnity();//获取图片文件夹的相对路径
                Object spriteFolder = AssetDatabase.LoadAssetAtPath<Object>(spritesRelativePath);
                atlas.Add(new Object[] { spriteFolder });
            }

        }

        private static SpriteAtlas SetAtlasSettings(bool alpha)
        {
            SpriteAtlas atlas = new SpriteAtlas();

            atlas.SetIncludeInBuild(true);

            SpriteAtlasPackingSettings packingSettings = new SpriteAtlasPackingSettings()
            {
                blockOffset = 1,
                enableRotation = false,
                enableTightPacking = false,
                padding = 4,
            };
            atlas.SetPackingSettings(packingSettings);

            SpriteAtlasTextureSettings textureSettings = new SpriteAtlasTextureSettings()
            {
                readable = false,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear,
            };
            atlas.SetTextureSettings(textureSettings);

            TextureImporterPlatformSettings standalonePlatformSettings = new TextureImporterPlatformSettings()
            {
                name = "Standalone",
                overridden = true,
                maxTextureSize = 2048,
                format = alpha ? TextureImporterFormat.DXT5 : TextureImporterFormat.DXT1,
                //crunchedCompression = true,
                textureCompression = TextureImporterCompression.Compressed,
                //compressionQuality = 50,
            };
            atlas.SetPlatformSettings(standalonePlatformSettings);

            TextureImporterPlatformSettings androidPlatformSettings = new TextureImporterPlatformSettings()
            {
                name = "Android",
                overridden = true,
                maxTextureSize = 2048,
                format = alpha ? TextureImporterFormat.ASTC_6x6 : TextureImporterFormat.ASTC_6x6,
                textureCompression = TextureImporterCompression.Compressed,
            };
            atlas.SetPlatformSettings(androidPlatformSettings);

            return atlas;
        }
    }
}