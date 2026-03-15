using System;

namespace LYFramework
{
    public abstract class SystemBase : ISystem
    {
        private IGameManager m_GameManager;

        IGameManager IGetGameManager.GetGameManager()
        {
            return m_GameManager;
        }

        void IDisposable.Dispose()
        {
            OnDispose();
            
            m_GameManager = null;
        }

        void ICanInit.Init(IGameManager gameManager)
        {
            m_GameManager = gameManager;

            OnInit();
        }
        
        protected abstract void OnInit();
        protected abstract void OnDispose();
    }
}