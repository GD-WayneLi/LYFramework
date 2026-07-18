namespace LYFramework.UI
{
    public abstract class UIController : UIBase, IController
    {
        private IGameManager m_GameManager;
        
        void INeedInit.Init(IGameManager gameManager)
        {
            m_GameManager = gameManager;
        }

        public IGameManager GetGameManager()
        {
            return m_GameManager;
        }
        
        void INeedInit.Dispose()
        {
            Dispose();
        }
    }
}