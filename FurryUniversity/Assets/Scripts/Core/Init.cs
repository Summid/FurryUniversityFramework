using UnityEngine;

namespace SFramework.Core.GameManager
{
    public class Init : MonoBehaviour
    {
        void Awake()
        {
            GameManager.InitializeGameManager();
            AssetBundleManager.LoadManifest();
        }
    }
}