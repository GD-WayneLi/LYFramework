using System;
using System.Collections.Generic;
using LYFramework.Log;

namespace LYFramework.Event
{
    public class EventManager : IEventManager
    {
        private Dictionary<Type, IEventWrapper> m_EventDic = new();
        private Dictionary<Type, IEntityEventWrapper> m_EntityEventDic = new();
        private Queue<PostEventWrapper> m_EventList = new();
        private Dictionary<Entity, HashSet<Type>> m_EntityEventTypes = new();
        
        public void AddListener<T>(EventHandler<T> listener) where T : IEvent
        {
            if (listener == null)
            {
                LYLogger.Error("need to add listener, but listener is null");
                return;
            }
            var type = typeof(T);
            if (!m_EventDic.TryGetValue(type, out var wrapper))
            {
                wrapper = new EventWrapper<T>();
                m_EventDic[type] = wrapper;
            }
            
            ((EventWrapper<T>)wrapper).Add(listener);
        }

        public void AddListener<TComponent, T>(TComponent component, LYEventHandler<TComponent, T> listener) where TComponent : Entity where T : IEvent
        {
            if (component == null)
            {
                LYLogger.Error("need to add listener, but component is null");
                return;
            }

            if (component.IsDisposed)
            {
                LYLogger.Error("need to add listener, but component is disposed");
                return;
            }

            if (listener == null)
            {
                LYLogger.Error("need to add listener, but listener is null");
                return;
            }

            var type = typeof(T);
            if (!m_EntityEventDic.TryGetValue(type, out var wrapper))
            {
                wrapper = new EntityEventWrapper<T>();
                m_EntityEventDic[type] = wrapper;
            }

            ((EntityEventWrapper<T>)wrapper).Add(component, listener);
            AddEntityEventType(component, type);
        }

        public void RemoveListener<T>(EventHandler<T> listener) where T : IEvent
        {
            if (listener == null)
            {
                LYLogger.Error("need to add listener, but listener is null");
                return;
            }
            var type = typeof(T);
            if (!m_EventDic.TryGetValue(type, out var wrapper))
            {
                return;
            }
            
            ((EventWrapper<T>)wrapper).Remove(listener);
            RemoveWrapperIfEmpty(type, wrapper);
        }

        public void RemoveListener<TComponent, T>(TComponent component, LYEventHandler<TComponent, T> listener) where TComponent : Entity where T : IEvent
        {
            if (component == null)
            {
                LYLogger.Error("need to remove listener, but component is null");
                return;
            }

            if (listener == null)
            {
                LYLogger.Error("need to remove listener, but listener is null");
                return;
            }

            var type = typeof(T);
            if (!m_EntityEventDic.TryGetValue(type, out var wrapper))
            {
                return;
            }

            if (((EntityEventWrapper<T>)wrapper).Remove(component, listener))
            {
                RemoveEntityEventTypeIfUnused(component, type, wrapper);
            }

            RemoveEntityWrapperIfEmpty(type, wrapper);
        }

        public void RemoveAllListeners(Entity entity)
        {
            if (entity == null)
            {
                LYLogger.Error("need to remove listeners, but entity is null");
                return;
            }

            if (!m_EntityEventTypes.Remove(entity, out var eventTypes))
            {
                return;
            }

            foreach (var eventType in eventTypes)
            {
                if (m_EntityEventDic.TryGetValue(eventType, out var wrapper))
                {
                    wrapper.RemoveAllListeners(entity);
                    RemoveEntityWrapperIfEmpty(eventType, wrapper);
                }
            }
        }

        public void Send<T>(object sender, T e) where T : IEvent
        {
            Send(sender, e, typeof(T));
        }

        public void Post<T>(object sender, T e) where T : IEvent
        {
            m_EventList.Enqueue(new PostEventWrapper(sender, e, typeof(T)));
        }

        public void Dispose()
        {
            m_EventDic.Clear();
            m_EntityEventDic.Clear();
            m_EventList.Clear();
            m_EntityEventTypes.Clear();
        }

        void IEventManager.Update()
        {
            var count = m_EventList.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var e = m_EventList.Dequeue();
                    Send(e.Sender, e.Event, e.EventType);
                }
            }
        }

        private void Send(object sender, IEvent e, Type eventType)
        {
            if (m_EventDic.TryGetValue(eventType, out var wrapper))
            {
                wrapper.Trigger(sender, e);
            }

            if (m_EntityEventDic.TryGetValue(eventType, out var entityWrapper))
            {
                entityWrapper.Trigger(sender, e);
            }
        }

        private void RemoveWrapperIfEmpty(Type eventType, IEventWrapper wrapper)
        {
            if (wrapper.IsEmpty)
            {
                m_EventDic.Remove(eventType);
            }
        }

        private void AddEntityEventType(Entity entity, Type eventType)
        {
            if (!m_EntityEventTypes.TryGetValue(entity, out var eventTypes))
            {
                eventTypes = new HashSet<Type>();
                m_EntityEventTypes.Add(entity, eventTypes);
            }

            eventTypes.Add(eventType);
        }

        private void RemoveEntityWrapperIfEmpty(Type eventType, IEntityEventWrapper wrapper)
        {
            if (wrapper.IsEmpty)
            {
                m_EntityEventDic.Remove(eventType);
            }
        }

        private void RemoveEntityEventTypeIfUnused(Entity entity, Type eventType, IEntityEventWrapper wrapper)
        {
            if (wrapper.HasListeners(entity) || !m_EntityEventTypes.TryGetValue(entity, out var eventTypes))
            {
                return;
            }

            eventTypes.Remove(eventType);
            if (eventTypes.Count == 0)
            {
                m_EntityEventTypes.Remove(entity);
            }
        }
    }
}
