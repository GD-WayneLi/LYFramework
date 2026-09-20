namespace LYFramework.Event
{
    public interface IEventWrapper
    {
        bool IsEmpty { get; }

        void Trigger(object sender, object e);
    }
}
