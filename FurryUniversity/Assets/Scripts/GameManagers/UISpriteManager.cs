using SFramework.Threading.Tasks;
using SFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace SFramework.Core.GameManager
{
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
                    if(atlasSprite.TryGetValue(originSprite.name.Replace("(Clone)",""), out var originAtlasName))
                    {
                        string originBundleName = originAtlasName.ToLower() + StaticVariables.SpriteAtlasBundleExtension;
                        AssetBundleManager.UnloadAssetBundle(originBundleName).Forget();
                        UnityEngine.Object.Destroy(originSprite);
                        image.sprite = null;
                    }
                }

                string bundleName = atlasName.ToLower() + StaticVariables.SpriteAtlasBundleExtension;//有gc，但无所谓了.jpg
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
                        AssetBundleManager.UnloadAssetBundle(originBundleName).Forget();
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