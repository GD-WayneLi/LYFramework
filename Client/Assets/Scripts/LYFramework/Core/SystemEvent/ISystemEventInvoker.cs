namespace LYFramework
{
    public interface ISystemEventInvoker
    {
        void Invoke(Entity component);
    }

    public interface ISystemEventInvoker<T> : ISystemEventInvoker
    {
        void Invoke(Entity component, T param);
    }
}