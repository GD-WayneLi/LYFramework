using System;

namespace LYFramework.Event
{
    public interface IEventManager : IDisposable
    {
        void AddListener<T>(EventHandler<T> listener) where T : IEvent;
        void RemoveListener<T>(EventHandler<T> listener) where T : IEvent;
        void Send<T>(object sender, T e) where T : IEvent;
        void Post<T>(object sender, T e) where T : IEvent;
        
        void Update();
    }
}