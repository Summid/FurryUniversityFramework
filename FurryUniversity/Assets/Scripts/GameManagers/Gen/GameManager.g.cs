namespace SFramework.Core.GameManagers
{
   public partial class GameManager
   {
        private AudioManager audiomanager;
        public AudioManager AudioManager
        {
             get
             {
                if(this.audiomanager == null)
                    this.audiomanager = this.GetManager<AudioManager>(typeof(AudioManager));
                return this.audiomanager;
             }
        }
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