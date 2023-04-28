using SFramework.Core.GameManagers;
using SFramework.Threading.Tasks;
using SFramework.Utilities;
using SFramework.Utilities.Archive;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public Button timerTestButton;
    private CancellationTokenSource timerCTS;
    private PlayerLoopTimer timer;

    public Button loadSceneTestButton;

    public RectTransform ViewRectTransform;
    public RectTransform RectTransform;

    private void Awake()
    {
        //this.discipleshipButton.onClick.AddListener(() =>
        //{
        //    this.showImage.SetSpriteAsync(UISpriteManager.Discipleship.Discipleship1).Forget();
        //});

        //this.dualityButton.onClick.AddListener(() =>
        //{
        //    this.showImage.SetSpriteAsync(UISpriteManager.Duality.Duality1).Forget();
        //});

        //this.kingsFallButton.onClick.AddListener(() =>
        //{
        //    this.showImage.SetSpriteAsync(UISpriteManager.KingsFall.KingsFall1).Forget();
        //});

        // this.clearButton?.onClick.AddListener(() =>
        // {
        //     this.showImage.UnloadSprite();
        // });
        //
        // this.updateTestButton?.onClick.AddListener(() =>
        // {
        //     if (this.cts == null)
        //     {
        //         this.cts = new CancellationTokenSource();
        //         this.cts.CancelAfterSilm(3000);
        //         STask.UpdateTask(() =>
        //         {
        //             Debug.Log("Update");
        //         }, PlayerLoopTiming.Update, this.cts.Token);
        //     }
        //     else
        //     {
        //         this.cts.Cancel();
        //         this.cts = null;
        //     }
        // });

        this.timerTestButton?.onClick.AddListener(() =>
        {
            //if (this.timer == null)
            //{
            //    this.timerCTS = new CancellationTokenSource();
            //    this.timer = PlayerLoopTimer.StartNew(TimeSpan.FromSeconds(2), true, DelayType.RealTime, PlayerLoopTiming.Update,
            //        this.timerCTS.Token, (state) => { Debug.Log(state); }, "timer state");
            //}
            //else
            //{
            //    this.timerCTS.Cancel();
            //    this.timer.Dispose();
            //    this.timer = null;
            //}
            this.TimerAutoDisposeTest();
        });

        this.loadSceneTestButton?.onClick.AddListener(async () =>
        {
            var progress = Progress.Create<float>(x => Debug.Log(x));
            await SceneManager.LoadSceneAsync("TestLoadScene").ToSTask(progress: progress);
        });
    }

    private async void TimerAutoDisposeTest()
    {
        Debug.Log("before create timer");
        using (PlayerLoopTimer timer = PlayerLoopTimer.StartNew(TimeSpan.FromSeconds(1), true, DelayType.RealTime, PlayerLoopTiming.Update,
            new CancellationToken(), (state) => { Debug.Log(state); }, "tiktok"))
        {
            Debug.Log("before delay timer");
            await STask.Delay(5000);
            Debug.Log("after delay timer");
        }
        Debug.Log("after create timer");
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

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.D))
        {
            AssetBundleManager.Dump();
        }
#endif

        if (Input.GetKeyDown(KeyCode.S))
        {
            AssetBundleManager.SweepAssetBundleVO();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            UIUtility.ClampToScreenRect(this.ViewRectTransform,this.RectTransform);
        }
        
    }
}
