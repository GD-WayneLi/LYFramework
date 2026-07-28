namespace LYFramework
{
    public interface IGameManager
    {
        /// <summary>
        /// 获取当前游戏的 World。World 仅负责管理 Scene 的生命周期与查询。
        /// </summary>
        World World { get; }

        void RegisterUtility<T>(T instance = default) where T : IUtility;
        void RegisterSystem<T>(T instance = default) where T : ISystem;
        
        T GetUtility<T>() where T : class, IUtility;
        T GetSystem<T>() where T : class, ISystem;
    }
}
