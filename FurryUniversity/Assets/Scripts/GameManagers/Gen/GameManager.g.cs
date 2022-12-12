namespace SFramework.Core.GameManager
{
   public partial class GameManager
   {
        private AssetBundleManager assetbundlemanager;
        public AssetBundleManager AssetBundleManager
        {
             get
             {
                if(this.assetbundlemanager == null)
                    this.assetbundlemanager = this.GetManager<AssetBundleManager>(typeof(AssetBundleManager));
                return this.assetbundlemanager;
             }
        }
   }
}