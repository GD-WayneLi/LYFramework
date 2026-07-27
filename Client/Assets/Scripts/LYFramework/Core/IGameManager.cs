namespace LYFramework
{
    public interface IGameManager
    {
        /// <summary>
        /// 当前游戏的 Entity World。GameManager 本身就是这个 World 的根 Scene。
        /// </summary>
        Entity World { get; }

        void RegisterUtility<T>(T instance = default) where T : IUtility;
        void RegisterSystem<T>(T instance = default) where T : ISystem;
        
        T GetUtility<T>() where T : class, IUtility;
        T GetSystem<T>() where T : class, ISystem;
    }
}
