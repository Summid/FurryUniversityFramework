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

        public static readonly string SpriteAtlasBundleExtension = ".spriteatlas";

        public static readonly string SpriteAtlasInfoPath = $"{Assets}/{Scripts}/{GameManagers}/{Gen}";

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