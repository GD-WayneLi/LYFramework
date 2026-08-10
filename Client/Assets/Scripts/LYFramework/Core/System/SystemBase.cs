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
    }
}