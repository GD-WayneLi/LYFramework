using System;
using System.Collections.Generic;

namespace LYFramework
{
    /// <summary>
    /// 管理 Component 的 System 生命周期处理器和逐帧更新队列。
    /// </summary>
    internal sealed class EntityEvent : SystemEventBase
    {
        private readonly Dictionary<Type, List<ISystemEventInvoker>> m_SystemAwakes = new();
        private readonly Dictionary<Type, List<ISystemEventInvoker>> m_SystemDisposes = new();
        private readonly Dictionary<Type, List<ISystemEventInvoker>> m_SystemUpdates = new();
        private readonly List<Entity> m_UpdateComponents = new();

        private bool m_IsUpdating;

        public override void RegisterSystem(ISystem system)
        {
            base.RegisterSystem(system);

            TryAddSystemEventInvokers(m_SystemAwakes, system, typeof(ISystemAwake<>), typeof(SystemEventInvoker<>));
            TryAddSystemEventInvokers(m_SystemDisposes, system, typeof(ISystemDispose<>), typeof(SystemDisposeInvoker<>));
            TryAddSystemEventInvokers(m_SystemUpdates, system, typeof(ISystemUpdate<>), typeof(SystemUpdateInvoker<>));
        }

        public void Awake(Entity component)
        {
            ThrowIfDisposed();

            var componentType = component.GetType();
            if (m_SystemAwakes.TryGetValue(componentType, out var invokers))
            {
                var count = invokers.Count;
                for (var i = 0; i < count && !component.IsDisposed; i++)
                {
                    invokers[i].Invoke(component);
                }
            }

            if (!component.IsDisposed && m_SystemUpdates.ContainsKey(componentType))
            {
                m_UpdateComponents.Add(component);
            }
        }

        public void Update()
        {
            ThrowIfDisposed();

            if (m_IsUpdating)
            {
                throw new InvalidOperationException("EntityLifecycle.Update cannot be called recursively.");
            }

            m_IsUpdating = true;
            try
            {
                var count = m_UpdateComponents.Count;
                for (var i = 0; i < count; i++)
                {
                    if (m_IsDisposed || i >= m_UpdateComponents.Count)
                    {
                        break;
                    }

                    var component = m_UpdateComponents[i];
                    if (!component.IsDisposed)
                    {
                        UpdateComponent(component);
                    }
                }
            }
            finally
            {
                m_IsUpdating = false;

                if (!m_IsDisposed)
                {
                    m_UpdateComponents.RemoveAll(component => component.IsDisposed);
                }
            }
        }

        public void DisposeComponent(Entity component)
        {
            List<Exception> exceptions = null;

            if (m_SystemDisposes.TryGetValue(component.GetType(), out var invokers))
            {
                var count = invokers.Count;
                for (var i = 0; i < count; i++)
                {
                    try
                    {
                        invokers[i].Invoke(component);
                    }
                    catch (Exception exception)
                    {
                        (exceptions ??= new List<Exception>()).Add(exception);
                    }
                }
            }

            Forget(component);

            if (exceptions != null)
            {
                throw new AggregateException(
                    $"Component dispose lifecycle failed: {component.GetType().Name}",
                    exceptions);
            }
        }

        public void Forget(Entity component)
        {
            if (!m_IsUpdating)
            {
                m_UpdateComponents.Remove(component);
            }
        }

        public override void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_UpdateComponents.Clear();
            m_SystemAwakes.Clear();
            m_SystemDisposes.Clear();
            m_SystemUpdates.Clear();
        }

        private void UpdateComponent(Entity component)
        {
            if (!m_SystemUpdates.TryGetValue(component.GetType(), out var invokers))
            {
                return;
            }

            var count = invokers.Count;
            for (var i = 0; i < count && !component.IsDisposed; i++)
            {
                invokers[i].Invoke(component);
            }
        }
    }
}
