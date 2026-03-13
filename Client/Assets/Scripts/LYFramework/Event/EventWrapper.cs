using System;

namespace LYFramework.Event
{
    public class EventWrapper<T> : IEventWrapper where T : IEvent
    {
        private EventHandler<T> m_Handler;

        public void Add(EventHandler<T> listener)
        {
            m_Handler += listener;
        }

        public void Remove(EventHandler<T> listener)
        {
            m_Handler -= listener;
        }

        public void Trigger(object sender, T e)
        {
            m_Handler?.Invoke(sender, e);
        }

        public void Trigger(object sender, object e)
        {
            Trigger(sender, (T)e);
        }
    }
}