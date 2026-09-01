using System;
using System.Collections.Generic;

namespace LYFramework
{
    public abstract class SystemEventBase : IDisposable
    {
        protected bool m_IsDisposed;
        
        public virtual void RegisterSystem(ISystem system)
        {
            ThrowIfDisposed();

            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }
        }

        public abstract void Dispose();
        
        protected void ThrowIfDisposed()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(EntityEvent));
            }
        }
        
        protected static void TryAddSystemEventInvokers<TInvoker>(Dictionary<Type, List<TInvoker>> eventInvokers, ISystem system, Type eventInterfaceType, Type invokerTypeDefinition) where TInvoker : class
        {
            foreach (var interfaceType in system.GetType().GetInterfaces())
            {
                if (!interfaceType.IsGenericType ||
                    interfaceType.GetGenericTypeDefinition() != eventInterfaceType)
                {
                    continue;
                }

                var componentTypes = interfaceType.GetGenericArguments();
                var invokerType = invokerTypeDefinition.MakeGenericType(componentTypes);
                var invoker = Activator.CreateInstance(invokerType, system) as TInvoker;
                if (invoker == null)
                {
                    throw new InvalidOperationException(
                        $"Unable to create system lifecycle invoker: {system.GetType().Name}, {componentTypes[0].Name}");
                }

                AddSystemEventInvoker(eventInvokers, componentTypes[0], invoker);
            }
        }

        protected static void AddSystemEventInvoker<TInvoker>(Dictionary<Type, List<TInvoker>> eventInvokers, Type componentType, TInvoker invoker)
        {
            if (!eventInvokers.TryGetValue(componentType, out var componentInvokers))
            {
                componentInvokers = new List<TInvoker>();
                eventInvokers.Add(componentType, componentInvokers);
            }

            componentInvokers.Add(invoker);
        }
    }
}