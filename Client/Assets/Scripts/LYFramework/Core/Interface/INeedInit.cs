using System;

namespace LYFramework
{
    public interface INeedInit
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Init(IGameManager gameManager);
        void Dispose();
    }
}