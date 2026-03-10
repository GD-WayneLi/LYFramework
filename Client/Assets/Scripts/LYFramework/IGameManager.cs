namespace LYFramework
{
    public interface IGameManager
    {
        void RegisterModel<T>() where T : IModel, new();
        void RegisterUtility<T>() where T : IUtility, new();
        void RegisterSystem<T>() where T : ISystem, new();
        
        T GetModel<T>() where T : class, IModel;
        T GetUtility<T>() where T : class, IUtility;
        T GetSystem<T>() where T : class, ISystem;
    }
}