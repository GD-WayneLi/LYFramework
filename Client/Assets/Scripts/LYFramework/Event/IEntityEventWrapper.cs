namespace LYFramework.Event
{
    public interface IEntityEventWrapper : IEventWrapper
    {
        bool HasListeners(Entity entity);
        void RemoveAllListeners(Entity entity);
    }
}
