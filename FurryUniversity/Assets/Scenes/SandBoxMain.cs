using SFramework.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SandBoxMain : MonoBehaviour, IPlayerLoopItem
{
    public bool MoveNext()
    {
        Debug.Log("MoveNext");
        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayerLoopHelper.AddAction(PlayerLoopTiming.Update, this);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            PlayerLoopHelper.AddContinuation(PlayerLoopTiming.Update, () => { Debug.Log("Yield Update"); });
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            Time.timeScale = 0.5f;
            this.WaitSTask();
        }
    }

    public async void WaitSTask()
    {
        Debug.Log("before wait");
        await this.WaitTaskDelaySeconds();

        Debug.Log("after wait");
    }

    public async STask WaitTaskDelaySeconds()
    {
        await STask.Delay(2000,DelayType.RealTime);
    }
}
