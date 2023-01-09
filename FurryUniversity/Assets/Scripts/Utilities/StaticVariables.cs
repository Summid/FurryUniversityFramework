using System.Text;
using UnityEditor;
using UnityEngine;

namespace SFramework.Utilities
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

        public static readonly string Scripts = "Scripts";
        public static readonly string GameManagers = "GameManagers";
        public static readonly string Gen = "Gen";
        public static readonly string ScriptsPath = $"{Assets}/{Scripts}";
        public static readonly string GameManagersPath = $"{ScriptsPath}/{GameManagers}";
        public static readonly string GameManagersGenPath = $"{GameManagersPath}/{Gen}";

        public static readonly string AssetBundlesFolderName = "AssetBundles";
        public static readonly string AssetBundlesFileExtension = ".bundle";
        public static readonly string AssetBundlesFileExtensionWithoutDot = "bundle";
        public static readonly string AssetBundleEditorTemp = "AssetBundleEditorTemp";

        public static readonly string SpriteExtensionPNG = ".png";
        public static readonly string SpriteExtensionJPG = ".jpg";

        public static readonly string SpriteAtlasExtension = ".spriteatlas";
        public static readonly string SpriteAtlasBundleExtension = "_spriteatlas";
        public static readonly string SpriteAtlasInfoPath = $"{Assets}/{Scripts}/{GameManagers}/{Gen}";

        public static readonly string PrefabExtension = ".prefab";
        public static readonly string UIViewBundleExtension = "_uiview";
        /// <summary> UIView Prefab文件放置路径 </summary>
        public static readonly string UIViewPrefabsPath = $"{BundlesPath}/Prefabs/UIView";
        /// <summary> UI Prefab脚本自动生成引用代码文件放置路径 </summary>
        public static readonly string UIViewGenerateCodePath = $"{ScriptsPath}/UIObjects/Gen";

        public static readonly string UIItemBundleExtension = "_uiitem";
        public static readonly string UIItemPrefabsPath = $"{BundlesPath}/Prefabs/UIItemTemplate";
        public static readonly string UIItemGenerateCodeFileName = "UIItemBase.g.cs";

        /// <summary> UIList清单文件名称 </summary>
        public static readonly string UIListName = "uiinfolist.asset";
        /// <summary> UIList清单Bundle名称 </summary>
        public static readonly string UIListBundleName = "uiinfolist_asset";

        /// <summary> 游戏BGM存放目录 </summary>
        public static readonly string AudioBGMPath = "Assets/Bundles/AudioAssets/BGM/";
        /// <summary> 常驻内存音效存放目录 </summary>
        public static readonly string AudioCommonSFXPath = "Assets/Bundles/AudioAssets/CommonSFX/";
        /// <summary> 音效组存放目录 </summary>
        public static readonly string AudioSFXGroupPath = "Assets/Bundles/AudioAssets/SFXGroup/";

        /// <summary> 游戏BGM Bundle 名后缀；一个文件一个Bundle </summary>
        public static readonly string AudioBGMBundleExtension = "_bgm";
        /// <summary> 常驻内存音效Bundle名；所有音效打包成一个Bundle </summary>
        public static readonly string AudioCommonSFXBundleName = "commonsfx_audio";
        /// <summary> 音效组Bundle名后缀；按照组来生成Bundle </summary>
        public static readonly string AudioSFXGroupBundleExtension = "_sfxgroup";
        /// <summary> AudioAssetList.g保存目录 </summary>
        public static readonly string AudioGenerateCodeFileFullName = "Assets/Scripts/GameManagers/Gen/AudioAssetList.g.cs";

        public static RuntimePlatform Platform
        {
            get
            {
#if UNITY_STANDALONE_WIN
                return RuntimePlatform.WindowsPlayer;
#elif UNITY_STANDALONE_OSX
                return RuntimePlatform.OSXPlayer;
#elif UNITY_ANDROID
                return RuntimePlatform.Android;
#elif UNITY_IOS
                return RuntimePlatform.IPhonePlayer;
#endif
            }
        }

        public static string StreamingAssetsPath
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                var t = Application.streamingAssetsPath;
#elif UNITY_ANDROID
                var t = $"{Application.dataPath}!assets";
#elif UNITY_IOS
                var t = $"{Application.dataPath}/Raw";
#endif
                return t;
            }
        }


    }
}