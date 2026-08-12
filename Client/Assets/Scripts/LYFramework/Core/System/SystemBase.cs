using System;

namespace LYFramework
{
    public abstract class SystemBase : ISystem
    {
        private IGameManager m_GameManager;
        private bool m_IsInitialized;

        void INeedInit.Init(IGameManager gameManager)
        {
            if (gameManager == null)
            {
                throw new ArgumentNullException(nameof(gameManager));
            }

            if (m_IsInitialized || m_GameManager != null)
            {
                throw new InvalidOperationException($"System is already initialized: {GetType().Name}");
            }

            m_GameManager = gameManager;
            try
            {
                OnInit();
                m_IsInitialized = true;
            }
            catch
            {
                m_GameManager = null;
                throw;
            }
        }

        void INeedInit.Dispose()
        {
            if (!m_IsInitialized && m_GameManager == null)
            {
                return;
            }

            try
            {
                if (m_IsInitialized)
                {
                    OnDispose();
                }
            }
            finally
            {
                m_IsInitialized = false;
                m_GameManager = null;
            }
        }

        IGameManager IGetGameManager.GetGameManager()
        {
            return m_GameManager;
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnDispose()
        {
        }
    }
}
