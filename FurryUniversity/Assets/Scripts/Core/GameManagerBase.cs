namespace SFramework.Core.GameManager
{
    public abstract class GameManagerBase
    {
        private bool isInitialized;

        public bool Initialize()
        {
            if (this.isInitialized)
                return false;
            this.OnInitialized();
            return true;
        }

        protected virtual void OnInitialized() { }
    }
}