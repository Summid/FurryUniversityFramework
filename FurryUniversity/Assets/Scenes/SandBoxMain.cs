using SFramework.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SandBoxMain : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    PlayerLoopHelper.AddAction(PlayerLoopTiming.Update, this);
        //}

        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    PlayerLoopHelper.AddContinuation(PlayerLoopTiming.Update, () => { Debug.Log("Yield Update"); });
        //}

        if(Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("before forget");
            this.WaitSeconds().Forget();
            //this.WaitSTask().Forget();
            Debug.Log("after forget");
        }
    }

    public async STask WaitSeconds()
    {
        Debug.Log("before wait");
        await Task.Delay(1000);
        Debug.Log("after wait");
    }

    public async STask WaitSTask()
    {
        Debug.Log("before wait");
        var result = await this.WaitTaskDelaySeconds();

        Debug.Log("after wait");
        Debug.Log(result);
    }

    public async STask<int> WaitTaskDelaySeconds()
    {
        await STask.Delay(1000);
        return 233;
    }

    public void TestSTaskVoid()
    {
        Debug.Log("before invoke");
        _=this.STaskVoid();
        Debug.Log("after invoke");
    }

    public async STaskVoid STaskVoid()
    {
        Debug.Log("before await");
        await STask.Delay(1000);
        Debug.Log("after await");
    }
}
