using System;

namespace LYFramework
{
    public interface ICanInit : IDisposable
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Init(IGameManager gameManager);
    }
}