using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.GameManager
{
    public static partial class UISpriteManager
    {
        public abstract class UIAtlasSpritesObject { }

        /// <summary> key:spriteName value:atlasName </summary>
        private static readonly Dictionary<string, string> atlasSprite;
    }
}