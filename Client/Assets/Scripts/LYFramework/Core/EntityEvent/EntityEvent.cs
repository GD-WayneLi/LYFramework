using System;
using System.Collections.Generic;

namespace LYFramework
{
    /// <summary>
    /// 管理 Component 的 System 生命周期处理器和逐帧更新队列。
    /// </summary>
    internal sealed class EntityEvent : IDisposable
    {
        private readonly Dictionary<Type, List<ISystemEventInvoker>> m_SystemAwakes = new();
        private readonly Dictionary<Type, List<ISystemEventInvoker>> m_SystemDisposes = new();
        private readonly Dictionary<Type, List<ISystemEventInvoker>> m_SystemUpdates = new();
        private readonly List<Entity> m_UpdateComponents = new();

        private bool m_IsDisposed;
        private bool m_IsUpdating;

        public void RegisterSystem(ISystem system)
        {
            ThrowIfDisposed();

            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            var awakeInvokers = CreateSystemLifecycleInvokers<ISystemEventInvoker>(
                system,
                typeof(ISystemAwake<>),
                typeof(SystemEventInvoker<>));
            var disposeInvokers = CreateSystemLifecycleInvokers<ISystemEventInvoker>(
                system,
                typeof(ISystemDispose<>),
                typeof(SystemDisposeInvoker<>));
            var updateInvokers = CreateSystemLifecycleInvokers<ISystemEventInvoker>(
                system,
                typeof(ISystemUpdate<>),
                typeof(SystemUpdateInvoker<>));

            AddSystemLifecycleInvokers(m_SystemAwakes, awakeInvokers);
            AddSystemLifecycleInvokers(m_SystemDisposes, disposeInvokers);
            AddSystemLifecycleInvokers(m_SystemUpdates, updateInvokers);
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

        public void Dispose()
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

        private static List<(Type ComponentType, TInvoker Invoker)> CreateSystemLifecycleInvokers<TInvoker>(ISystem system, Type lifecycleInterfaceType, Type invokerTypeDefinition) where TInvoker : class
        {
            var invokers = new List<(Type ComponentType, TInvoker Invoker)>();

            foreach (var interfaceType in system.GetType().GetInterfaces())
            {
                if (!interfaceType.IsGenericType ||
                    interfaceType.GetGenericTypeDefinition() != lifecycleInterfaceType)
                {
                    continue;
                }

                var componentType = interfaceType.GetGenericArguments()[0];
                var invokerType = invokerTypeDefinition.MakeGenericType(componentType);
                var invoker = Activator.CreateInstance(invokerType, system) as TInvoker;
                if (invoker == null)
                {
                    throw new InvalidOperationException(
                        $"Unable to create system lifecycle invoker: {system.GetType().Name}, {componentType.Name}");
                }

                invokers.Add((componentType, invoker));
            }

            return invokers;
        }

        private static void AddSystemLifecycleInvokers<TInvoker>(Dictionary<Type, List<TInvoker>> lifecycleInvokers, List<(Type ComponentType, TInvoker Invoker)> invokers)
        {
            foreach (var item in invokers)
            {
                if (!lifecycleInvokers.TryGetValue(item.ComponentType, out var componentInvokers))
                {
                    componentInvokers = new List<TInvoker>();
                    lifecycleInvokers.Add(item.ComponentType, componentInvokers);
                }

                componentInvokers.Add(item.Invoker);
            }
        }

        private void ThrowIfDisposed()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(EntityEvent));
            }
        }
    }
}
