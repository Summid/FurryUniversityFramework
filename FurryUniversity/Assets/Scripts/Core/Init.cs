using UnityEngine;

namespace SFramework.Core.GameManager
{
    public class Init : MonoBehaviour
    {
        void Start()
        {
            GameManager.InitializeGameManager();
        }
    }
}