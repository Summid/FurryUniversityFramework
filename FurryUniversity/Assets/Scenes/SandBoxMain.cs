using SFramework.Core.GameManager;
using SFramework.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class SandBoxMain : MonoBehaviour
{
    public Image showImage;

    private async void Start()
    {
        var atlas = await AssetBundleManager.LoadAssetInAssetBundleAsync<SpriteAtlas>("Discipleship", "discipleship.spriteatlas");
        var sprite = atlas.GetSprite("Discipleship0");
        this.showImage.sprite = sprite;
        this.showImage.SetNativeSize();
    }

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            await AssetBundleManager.UnloadAssetBundle("discipleship.spriteatlas");
            this.showImage.sprite = null;
            GC.Collect();
        }
    }
}
