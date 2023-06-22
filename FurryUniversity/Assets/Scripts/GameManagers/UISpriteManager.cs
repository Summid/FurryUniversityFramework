using SFramework.Threading.Tasks;
using SFramework.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace SFramework.Core.GameManagers
{
    //TODO: Object.name GC, string.Replace() method GC, 后期考虑用monobehavior给每个Image Object做一个缓存，记录其 bundleName和spriteName
    public static partial class UISpriteManager
    {
        public abstract class UIAtlasSpritesObject { }

        /// <summary> key:spriteName value:bundleName with out extension and lower case conversion </summary>
        private static readonly Dictionary<string, string> atlasSprite;

        public static async STask SetSpriteAsync(this Image image, string spriteName, bool setNativeSize = true)
        {
            if (string.IsNullOrEmpty(spriteName) || image == null || image.sprite != null && image.sprite.name == spriteName)
            {
                return;
            }

            if(atlasSprite.TryGetValue(spriteName, out var atlasName))
            {
                Sprite originSprite = image.sprite;
                if (originSprite != null)
                {
                    //unload first
                    if (originSprite.name.Contains("(Clone)") && atlasSprite.TryGetValue(originSprite.name.Replace("(Clone)", ""), out var originAtlasName))
                    {
                        if (!string.IsNullOrEmpty(originAtlasName))
                        {
                            string originBundleName = originAtlasName.ToLower() + StaticVariables.SpriteAtlasBundleExtension;
                            AssetBundleManager.UnloadAssetBundleAsync(originBundleName).Forget();
                            UnityEngine.Object.Destroy(originSprite);
                            image.sprite = null;
                        }
                    }
                }

                string bundleName = atlasName.ToLower() + StaticVariables.SpriteAtlasBundleExtension;//有gc，但无所谓了.jpg；不加后缀也能正常加载，不确定，有空试试看
                SpriteAtlas atlas = await AssetBundleManager.LoadAssetInAssetBundleAsync<SpriteAtlas>(atlasName, bundleName);
                Sprite targetSprite = atlas.GetSprite(spriteName);
                image.sprite = targetSprite;

                if (setNativeSize)
                    image.SetNativeSize();
            }
            else
            {
                Debug.LogError("SetSprite can not find sprite: " + spriteName);
            }
        }

        public static void UnloadSprite(this Image image)
        {
            if (image == null || image.sprite == null)
            {
                return;
            }

            if (atlasSprite.TryGetValue(image.sprite.name.Replace("(Clone)",""), out var atlasName))
            {
                Sprite originSprite = image.sprite;
                if (originSprite != null)
                {
                    //unload first
                    if (atlasSprite.TryGetValue(originSprite.name.Replace("(Clone)", ""), out var originAtlasName))
                    {
                        string originBundleName = originAtlasName.ToLower() + StaticVariables.SpriteAtlasBundleExtension;
                        AssetBundleManager.UnloadAssetBundleAsync(originBundleName).Forget();
                        UnityEngine.Object.Destroy(originSprite);
                        image.sprite = null;
                    }
                }
            }
            else
            {
                Debug.LogError("SetSprite can not find sprite: " + image.sprite.name);
            }
        }
    }
}