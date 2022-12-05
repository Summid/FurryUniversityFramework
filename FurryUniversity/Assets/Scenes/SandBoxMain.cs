using SFramework.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
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

    }
}
