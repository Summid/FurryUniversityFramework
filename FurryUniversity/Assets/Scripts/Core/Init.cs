using SDS;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFramework.Core.GameManagers
{
    public class Init : MonoBehaviour
    {
        [Header("存档相关")]
        public int ArchiveCount;
        public enum ArchiveTypeEnum { Binary, JSON }

        public ArchiveTypeEnum ArchiveType;
        
        void Awake()
        {
            AssetBundleManager.LoadManifest();//load manifest before GameManagers' Initialization
            GameManager.InitializeGameManager();

            DontDestroyOnLoad(this.gameObject);
        }


    }
}