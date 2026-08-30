using System.Collections.Generic;

namespace LYFramework
{
    public interface IGameManager
    {
        /// <summary>
        /// 同步更新当前所有声明了 ISystemUpdate 生命周期的存活 Component。
        /// </summary>
        void Update();

        void RegisterUtility<T>(T instance = default) where T : IUtility;
        void RegisterSystem<T>(T instance = default) where T : ISystem;

        T GetUtility<T>() where T : class, IUtility;
        T GetSystem<T>() where T : class, ISystem;

        /// <summary>
        /// 按注册顺序获取当前 System 的只读快照。调用方不拥有返回集合或其中的 System。
        /// </summary>
        IReadOnlyList<ISystem> GetSystems();
    }
}
