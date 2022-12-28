using UnityEngine;

namespace SFramework.Core.GameManagers
{
    public class Init : MonoBehaviour
    {
        void Awake()
        {
            AssetBundleManager.LoadManifest();//load manifest before GameManagers' Initialization
            GameManager.InitializeGameManager();
        }
    }
}