using SFramework.Threading.Tasks;
using System;

namespace SFramework.Core.GameManager
{
    public class GameManager
    {
        public static GameManager Instance { get; private set; }
        private GameManager() { }
        public static void InitializeGameManager()
        {
            try
            {
                Instance = new GameManager();
                Instance.InitializeGameManagers();
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }

        private void InitializeGameManagers()
        {

        }
    }
}