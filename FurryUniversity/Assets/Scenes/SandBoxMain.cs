using SFramework.Core.GameManagers;
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
    public Button discipleshipButton;
    public Button dualityButton;
    public Button kingsFallButton;
    public Button clearButton;

    public Button updateTestButton;
    private CancellationTokenSource cts;

    private void Awake()
    {
        this.discipleshipButton.onClick.AddListener(() =>
        {
            this.showImage.SetSpriteAsync(UISpriteManager.Discipleship.Discipleship1).Forget();
        });

        this.dualityButton.onClick.AddListener(() =>
        {
            this.showImage.SetSpriteAsync(UISpriteManager.Duality.Duality1).Forget();
        });

        this.kingsFallButton.onClick.AddListener(() =>
        {
            this.showImage.SetSpriteAsync(UISpriteManager.KingsFall.KingsFall1).Forget();
        });

        this.clearButton.onClick.AddListener(() =>
        {
            this.showImage.UnloadSprite();
        });

        this.updateTestButton.onClick.AddListener(() =>
        {
            if (this.cts == null)
            {
                this.cts = new CancellationTokenSource();
                STask.UpdateTask(() =>
                {
                    Debug.Log("Update");
                }, PlayerLoopTiming.Update, this.cts.Token);
            }
            else
            {
                this.cts.Cancel();
                this.cts = null;
            }
        });
    }

    private void Start()
    {
        //var atlas = await AssetBundleManager.LoadAssetInAssetBundleAsync<SpriteAtlas>("Discipleship", "discipleship.spriteatlas");
        //var sprite = atlas.GetSprite("Discipleship0");
        //this.showImage.sprite = sprite;
        //this.showImage.SetNativeSize();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.U))
        //{
        //    await AssetBundleManager.UnloadAssetBundle("discipleship.spriteatlas");
        //    Destroy(this.showImage.sprite);
        //    this.showImage.sprite = null;
        //    GC.Collect();
        //}
    }
}
