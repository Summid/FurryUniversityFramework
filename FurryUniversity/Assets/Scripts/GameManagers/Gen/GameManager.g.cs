namespace SFramework.Core.GameManager
{
   public partial class GameManager
   {
        private UIManager uimanager;
        public UIManager UIManager
        {
             get
             {
                if(this.uimanager == null)
                    this.uimanager = this.GetManager<UIManager>(typeof(UIManager));
                return this.uimanager;
             }
        }
   }
}