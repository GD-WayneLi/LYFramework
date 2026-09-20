using System.Collections.Generic;

namespace LYFramework.Event
{
    public class EntityEventWrapper<TEvent> : IEntityEventWrapper where TEvent : IEvent
    {
        private readonly List<IEntityListener> m_EntityListeners = new();

        public bool IsEmpty => m_EntityListeners.Count == 0;

        public void Add<TComponent>(TComponent component, LYEventHandler<TComponent, TEvent> listener) where TComponent : Entity
        {
            m_EntityListeners.Add(new EntityListener<TComponent>(component, listener));
        }

        public bool Remove<TComponent>(TComponent component, LYEventHandler<TComponent, TEvent> listener) where TComponent : Entity
        {
            for (var i = m_EntityListeners.Count - 1; i >= 0; i--)
            {
                if (m_EntityListeners[i] is EntityListener<TComponent> entityListener && entityListener.Matches(component, listener))
                {
                    entityListener.Deactivate();
                    m_EntityListeners.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool HasListeners(Entity entity)
        {
            foreach (var entityListener in m_EntityListeners)
            {
                if (ReferenceEquals(entityListener.Entity, entity))
                {
                    return true;
                }
            }

            return false;
        }

        public void RemoveAllListeners(Entity entity)
        {
            for (var i = m_EntityListeners.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(m_EntityListeners[i].Entity, entity))
                {
                    m_EntityListeners[i].Deactivate();
                    m_EntityListeners.RemoveAt(i);
                }
            }
        }

        public void Trigger(object sender, TEvent e)
        {
            var entityListeners = m_EntityListeners.ToArray();
            foreach (var entityListener in entityListeners)
            {
                if (entityListener.IsActive)
                {
                    entityListener.Invoke(sender, e);
                }
            }
        }

        public void Trigger(object sender, object e)
        {
            Trigger(sender, (TEvent)e);
        }

        private interface IEntityListener
        {
            Entity Entity { get; }
            bool IsActive { get; }

            void Deactivate();
            void Invoke(object sender, TEvent e);
        }

        private sealed class EntityListener<TComponent> : IEntityListener where TComponent : Entity
        {
            private readonly TComponent m_Component;
            private readonly LYEventHandler<TComponent, TEvent> m_Listener;

            public Entity Entity => m_Component;
            public bool IsActive { get; private set; } = true;

            public EntityListener(TComponent component, LYEventHandler<TComponent, TEvent> listener)
            {
                m_Component = component;
                m_Listener = listener;
            }

            public bool Matches(TComponent component, LYEventHandler<TComponent, TEvent> listener)
            {
                return ReferenceEquals(m_Component, component) && m_Listener == listener;
            }

            public void Deactivate()
            {
                IsActive = false;
            }

            public void Invoke(object sender, TEvent e)
            {
                m_Listener(m_Component, sender, e);
            }
        }
    }
}
