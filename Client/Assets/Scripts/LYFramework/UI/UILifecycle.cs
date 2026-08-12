using System;
using System.Collections.Generic;

namespace LYFramework.UI
{
    public interface IUILoaded<T> where T : Entity
    {
        void Loaded(T self, object resource);
    }

    public interface IUIOpen<T> where T : Entity
    {
        void Open(T self);
    }

    public interface IUIUpdate<T> where T : Entity
    {
        void Update(T self);
    }

    public interface IUIClose<T> where T : Entity
    {
        void Close(T self);
    }

    public interface IUIDepthChanged<T> where T : Entity
    {
        void DepthChanged(T self, int depth);
    }

    public interface IUIVisibilityChanged<T> where T : Entity
    {
        void VisibilityChanged(T self, bool isVisible);
    }

    public interface IUIVisibilityPolicy<T> where T : Entity
    {
        bool HideLowerUI { get; }
    }

    internal interface IUILoadedInvoker
    {
        void Invoke(Entity component, object resource);
    }

    internal interface IUIOpenInvoker
    {
        void Invoke(Entity component);
    }

    internal interface IUIUpdateInvoker
    {
        void Invoke(Entity component);
    }

    internal interface IUICloseInvoker
    {
        void Invoke(Entity component);
    }

    internal interface IUIDepthChangedInvoker
    {
        void Invoke(Entity component, int depth);
    }

    internal interface IUIVisibilityChangedInvoker
    {
        void Invoke(Entity component, bool isVisible);
    }

    internal interface IUIVisibilityPolicyInvoker
    {
        bool HideLowerUI { get; }
    }

    internal sealed class UILoadedInvoker<T> : IUILoadedInvoker where T : Entity
    {
        private readonly IUILoaded<T> m_System;

        public UILoadedInvoker(object system)
        {
            m_System = (IUILoaded<T>)system;
        }

        public void Invoke(Entity component, object resource)
        {
            m_System.Loaded((T)component, resource);
        }
    }

    internal sealed class UIOpenInvoker<T> : IUIOpenInvoker where T : Entity
    {
        private readonly IUIOpen<T> m_System;

        public UIOpenInvoker(object system)
        {
            m_System = (IUIOpen<T>)system;
        }

        public void Invoke(Entity component)
        {
            m_System.Open((T)component);
        }
    }

    internal sealed class UIUpdateInvoker<T> : IUIUpdateInvoker where T : Entity
    {
        private readonly IUIUpdate<T> m_System;

        public UIUpdateInvoker(object system)
        {
            m_System = (IUIUpdate<T>)system;
        }

        public void Invoke(Entity component)
        {
            m_System.Update((T)component);
        }
    }

    internal sealed class UICloseInvoker<T> : IUICloseInvoker where T : Entity
    {
        private readonly IUIClose<T> m_System;

        public UICloseInvoker(object system)
        {
            m_System = (IUIClose<T>)system;
        }

        public void Invoke(Entity component)
        {
            m_System.Close((T)component);
        }
    }

    internal sealed class UIDepthChangedInvoker<T> : IUIDepthChangedInvoker where T : Entity
    {
        private readonly IUIDepthChanged<T> m_System;

        public UIDepthChangedInvoker(object system)
        {
            m_System = (IUIDepthChanged<T>)system;
        }

        public void Invoke(Entity component, int depth)
        {
            m_System.DepthChanged((T)component, depth);
        }
    }

    internal sealed class UIVisibilityChangedInvoker<T> : IUIVisibilityChangedInvoker where T : Entity
    {
        private readonly IUIVisibilityChanged<T> m_System;

        public UIVisibilityChangedInvoker(object system)
        {
            m_System = (IUIVisibilityChanged<T>)system;
        }

        public void Invoke(Entity component, bool isVisible)
        {
            m_System.VisibilityChanged((T)component, isVisible);
        }
    }

    internal sealed class UIVisibilityPolicyInvoker<T> : IUIVisibilityPolicyInvoker where T : Entity
    {
        private readonly IUIVisibilityPolicy<T> m_System;

        public UIVisibilityPolicyInvoker(object system)
        {
            m_System = (IUIVisibilityPolicy<T>)system;
        }

        public bool HideLowerUI => m_System.HideLowerUI;
    }

    /// <summary>
    /// 保存一个 UIManager 的 UI 专属生命周期处理器。
    /// 注册阶段反射 System 接口，运行阶段按具体 Component 类型直接分发。
    /// </summary>
    internal sealed class UILifecycle : IDisposable
    {
        private Dictionary<Type, List<IUILoadedInvoker>> m_LoadedInvokers = new();
        private Dictionary<Type, List<IUIOpenInvoker>> m_OpenInvokers = new();
        private Dictionary<Type, List<IUIUpdateInvoker>> m_UpdateInvokers = new();
        private Dictionary<Type, List<IUICloseInvoker>> m_CloseInvokers = new();
        private Dictionary<Type, List<IUIDepthChangedInvoker>> m_DepthChangedInvokers = new();
        private Dictionary<Type, List<IUIVisibilityChangedInvoker>> m_VisibilityChangedInvokers = new();
        private Dictionary<Type, IUIVisibilityPolicyInvoker> m_VisibilityPolicyInvokers = new();
        private bool m_IsRegistered;
        private bool m_IsDisposed;

        public void RegisterSystems(IReadOnlyList<ISystem> systems)
        {
            ThrowIfDisposed();

            if (systems == null)
            {
                throw new ArgumentNullException(nameof(systems));
            }

            if (m_IsRegistered)
            {
                throw new InvalidOperationException("UI lifecycle systems are already registered.");
            }

            var loadedInvokers = new Dictionary<Type, List<IUILoadedInvoker>>();
            var openInvokers = new Dictionary<Type, List<IUIOpenInvoker>>();
            var updateInvokers = new Dictionary<Type, List<IUIUpdateInvoker>>();
            var closeInvokers = new Dictionary<Type, List<IUICloseInvoker>>();
            var depthChangedInvokers = new Dictionary<Type, List<IUIDepthChangedInvoker>>();
            var visibilityChangedInvokers = new Dictionary<Type, List<IUIVisibilityChangedInvoker>>();
            var visibilityPolicyInvokers = new Dictionary<Type, IUIVisibilityPolicyInvoker>();

            foreach (var system in systems)
            {
                if (system == null)
                {
                    throw new InvalidOperationException("GameManager returned a null System.");
                }

                AddLifecycleInvokers(loadedInvokers, CreateLifecycleInvokers<IUILoadedInvoker>(system, typeof(IUILoaded<>), typeof(UILoadedInvoker<>)));
                AddLifecycleInvokers(openInvokers, CreateLifecycleInvokers<IUIOpenInvoker>(system, typeof(IUIOpen<>), typeof(UIOpenInvoker<>)));
                AddLifecycleInvokers(updateInvokers, CreateLifecycleInvokers<IUIUpdateInvoker>(system, typeof(IUIUpdate<>), typeof(UIUpdateInvoker<>)));
                AddLifecycleInvokers(closeInvokers, CreateLifecycleInvokers<IUICloseInvoker>(system, typeof(IUIClose<>), typeof(UICloseInvoker<>)));
                AddLifecycleInvokers(depthChangedInvokers, CreateLifecycleInvokers<IUIDepthChangedInvoker>(system, typeof(IUIDepthChanged<>), typeof(UIDepthChangedInvoker<>)));
                AddLifecycleInvokers(visibilityChangedInvokers, CreateLifecycleInvokers<IUIVisibilityChangedInvoker>(system, typeof(IUIVisibilityChanged<>), typeof(UIVisibilityChangedInvoker<>)));
                AddVisibilityPolicyInvokers(visibilityPolicyInvokers, CreateLifecycleInvokers<IUIVisibilityPolicyInvoker>(system, typeof(IUIVisibilityPolicy<>), typeof(UIVisibilityPolicyInvoker<>)), system);
            }

            m_LoadedInvokers = loadedInvokers;
            m_OpenInvokers = openInvokers;
            m_UpdateInvokers = updateInvokers;
            m_CloseInvokers = closeInvokers;
            m_DepthChangedInvokers = depthChangedInvokers;
            m_VisibilityChangedInvokers = visibilityChangedInvokers;
            m_VisibilityPolicyInvokers = visibilityPolicyInvokers;
            m_IsRegistered = true;
        }

        public void Loaded(Entity component, object resource)
        {
            ThrowIfInvalidComponent(component);

            if (!m_LoadedInvokers.TryGetValue(component.GetType(), out var invokers))
            {
                return;
            }

            for (var i = 0; i < invokers.Count && !component.IsDisposed; i++)
            {
                invokers[i].Invoke(component, resource);
            }
        }

        public void Open(Entity component)
        {
            ThrowIfInvalidComponent(component);

            if (!m_OpenInvokers.TryGetValue(component.GetType(), out var invokers))
            {
                return;
            }

            for (var i = 0; i < invokers.Count && !component.IsDisposed; i++)
            {
                invokers[i].Invoke(component);
            }
        }

        public void Update(Entity component)
        {
            ThrowIfInvalidComponent(component);

            if (!m_UpdateInvokers.TryGetValue(component.GetType(), out var invokers))
            {
                return;
            }

            for (var i = 0; i < invokers.Count && !component.IsDisposed; i++)
            {
                invokers[i].Invoke(component);
            }
        }

        public void Close(Entity component)
        {
            ThrowIfDisposed();

            if (component == null || !m_CloseInvokers.TryGetValue(component.GetType(), out var invokers))
            {
                return;
            }

            List<Exception> exceptions = null;
            for (var i = 0; i < invokers.Count; i++)
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

            if (exceptions != null)
            {
                throw new AggregateException($"UI close lifecycle failed: {component.GetType().Name}", exceptions);
            }
        }

        public void DepthChanged(Entity component, int depth)
        {
            ThrowIfInvalidComponent(component);

            if (!m_DepthChangedInvokers.TryGetValue(component.GetType(), out var invokers))
            {
                return;
            }

            for (var i = 0; i < invokers.Count && !component.IsDisposed; i++)
            {
                invokers[i].Invoke(component, depth);
            }
        }

        public void VisibilityChanged(Entity component, bool isVisible)
        {
            ThrowIfInvalidComponent(component);

            if (!m_VisibilityChangedInvokers.TryGetValue(component.GetType(), out var invokers))
            {
                return;
            }

            for (var i = 0; i < invokers.Count && !component.IsDisposed; i++)
            {
                invokers[i].Invoke(component, isVisible);
            }
        }

        public bool GetHideLowerUI(Type componentType)
        {
            ThrowIfDisposed();

            return componentType != null &&
                   m_VisibilityPolicyInvokers.TryGetValue(componentType, out var invoker) &&
                   invoker.HideLowerUI;
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_LoadedInvokers.Clear();
            m_OpenInvokers.Clear();
            m_UpdateInvokers.Clear();
            m_CloseInvokers.Clear();
            m_DepthChangedInvokers.Clear();
            m_VisibilityChangedInvokers.Clear();
            m_VisibilityPolicyInvokers.Clear();
        }

        private static List<(Type ComponentType, TInvoker Invoker)> CreateLifecycleInvokers<TInvoker>(ISystem system, Type lifecycleInterfaceType, Type invokerTypeDefinition) where TInvoker : class
        {
            var invokers = new List<(Type ComponentType, TInvoker Invoker)>();

            foreach (var interfaceType in system.GetType().GetInterfaces())
            {
                if (!interfaceType.IsGenericType || interfaceType.GetGenericTypeDefinition() != lifecycleInterfaceType)
                {
                    continue;
                }

                var componentType = interfaceType.GetGenericArguments()[0];
                var invokerType = invokerTypeDefinition.MakeGenericType(componentType);
                var invoker = Activator.CreateInstance(invokerType, system) as TInvoker;
                if (invoker == null)
                {
                    throw new InvalidOperationException($"Unable to create UI lifecycle invoker: {system.GetType().Name}, {componentType.Name}");
                }

                invokers.Add((componentType, invoker));
            }

            return invokers;
        }

        private static void AddLifecycleInvokers<TInvoker>(Dictionary<Type, List<TInvoker>> lifecycleInvokers, List<(Type ComponentType, TInvoker Invoker)> invokers)
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

        private static void AddVisibilityPolicyInvokers(Dictionary<Type, IUIVisibilityPolicyInvoker> policyInvokers, List<(Type ComponentType, IUIVisibilityPolicyInvoker Invoker)> invokers, ISystem system)
        {
            foreach (var item in invokers)
            {
                if (!policyInvokers.TryAdd(item.ComponentType, item.Invoker))
                {
                    throw new InvalidOperationException($"UI visibility policy is already registered: {item.ComponentType.FullName}, System: {system.GetType().FullName}");
                }
            }
        }

        private void ThrowIfInvalidComponent(Entity component)
        {
            ThrowIfDisposed();

            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (component.IsDisposed)
            {
                throw new ObjectDisposedException(component.GetType().Name);
            }
        }

        private void ThrowIfDisposed()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UILifecycle));
            }
        }
    }
}
