namespace LYFramework.Event
{
    public delegate void LYEventHandler<in TComponent, in TEvent>(TComponent component, object sender, TEvent e) where TComponent : Entity where TEvent : IEvent;
}
