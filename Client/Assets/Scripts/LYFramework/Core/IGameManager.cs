namespace LYFramework
{
    public interface IGameManager
    {
        void RegisterModel<T>(T instance = default) where T : IModel;
        void RegisterUtility<T>(T instance = default) where T : IUtility;
        void RegisterSystem<T>(T instance = default) where T : ISystem;
        
        T GetModel<T>() where T : class, IModel;
        T GetUtility<T>() where T : class, IUtility;
        T GetSystem<T>() where T : class, ISystem;
    }
}