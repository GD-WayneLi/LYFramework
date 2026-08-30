using System;
using System.Collections.Generic;
using LYFramework;

namespace LYUnity.UI
{
    public interface IUILoaded<T> where T : Entity
    {
        void OnLoaded(T self);
    }

    public interface IUIOpen<T> where T : Entity
    {
        void OnOpen(T self);
    }

    public interface IUIUpdate<T> where T : Entity
    {
        void Update(T self);
    }

    public interface IUIClose<T> where T : Entity
    {
        void OnClose(T self);
    }

    public interface IUIDepthChanged<T> where T : Entity
    {
        void OnDepthChanged(T self);
    }

    public interface IUIVisibilityChanged<T> where T : Entity
    {
        void OnVisibilityChanged(T self);
    }
    
    internal sealed class UILoadedInvoker<T> : ISystemEventInvoker where T : Entity
    {
        private readonly IUILoaded<T> m_System;

        public UILoadedInvoker(object system)
        {
            m_System = (IUILoaded<T>)system;
        }

        public void Invoke(Entity component)
        {
            m_System.OnLoaded((T)component);
        }
    }

    internal sealed class UIOpenInvoker<T> : ISystemEventInvoker where T : Entity
    {
        private readonly IUIOpen<T> m_System;

        public UIOpenInvoker(object system)
        {
            m_System = (IUIOpen<T>)system;
        }

        public void Invoke(Entity component)
        {
            m_System.OnOpen((T)component);
        }
    }

    internal sealed class UIUpdateInvoker<T> : ISystemEventInvoker where T : Entity
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

    internal sealed class UICloseInvoker<T> : ISystemEventInvoker where T : Entity
    {
        private readonly IUIClose<T> m_System;

        public UICloseInvoker(object system)
        {
            m_System = (IUIClose<T>)system;
        }

        public void Invoke(Entity component)
        {
            m_System.OnClose((T)component);
        }
    }

    internal sealed class UIDepthChangedInvoker<T> : ISystemEventInvoker where T : Entity
    {
        private readonly IUIDepthChanged<T> m_System;

        public UIDepthChangedInvoker(object system)
        {
            m_System = (IUIDepthChanged<T>)system;
        }

        public void Invoke(Entity component)
        {
            m_System.OnDepthChanged((T)component);
        }
    }

    internal sealed class UIVisibilityChangedInvoker<T> : ISystemEventInvoker where T : Entity
    {
        private readonly IUIVisibilityChanged<T> m_System;

        public UIVisibilityChangedInvoker(object system)
        {
            m_System = (IUIVisibilityChanged<T>)system;
        }

        public void Invoke(Entity component)
        {
            m_System.OnVisibilityChanged((T)component);
        }
    }
    
    /// <summary>
    /// 保存一个 UIManager 的 UI 专属生命周期处理器。
    /// 注册阶段反射 System 接口，运行阶段按具体 Component 类型直接分发。
    /// </summary>
    internal sealed class UIEvent : SystemEventBase
    {
        private Dictionary<Type, List<ISystemEventInvoker>> m_LoadedInvokers = new();
        private Dictionary<Type, List<ISystemEventInvoker>> m_OpenInvokers = new();
        private Dictionary<Type, List<ISystemEventInvoker>> m_UpdateInvokers = new();
        private Dictionary<Type, List<ISystemEventInvoker>> m_CloseInvokers = new();
        private Dictionary<Type, List<ISystemEventInvoker>> m_DepthChangedInvokers = new();
        private Dictionary<Type, List<ISystemEventInvoker>> m_VisibilityChangedInvokers = new();
        private bool m_IsRegistered;

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

            var loadedInvokers = new Dictionary<Type, List<ISystemEventInvoker>>();
            var openInvokers = new Dictionary<Type, List<ISystemEventInvoker>>();
            var updateInvokers = new Dictionary<Type, List<ISystemEventInvoker>>();
            var closeInvokers = new Dictionary<Type, List<ISystemEventInvoker>>();
            var depthChangedInvokers = new Dictionary<Type, List<ISystemEventInvoker>>();
            var visibilityChangedInvokers = new Dictionary<Type, List<ISystemEventInvoker>>();

            foreach (var system in systems)
            {
                if (system == null)
                {
                    throw new InvalidOperationException("GameManager returned a null System.");
                }

                TryAddSystemEventInvokers(loadedInvokers, system, typeof(IUILoaded<>), typeof(UILoadedInvoker<>));
                TryAddSystemEventInvokers(openInvokers, system, typeof(IUIOpen<>), typeof(UIOpenInvoker<>));
                TryAddSystemEventInvokers(updateInvokers, system, typeof(IUIUpdate<>), typeof(UIUpdateInvoker<>));
                TryAddSystemEventInvokers(closeInvokers, system, typeof(IUIClose<>), typeof(UICloseInvoker<>));
                TryAddSystemEventInvokers(depthChangedInvokers, system, typeof(IUIDepthChanged<>), typeof(UIDepthChangedInvoker<>));
                TryAddSystemEventInvokers(visibilityChangedInvokers, system, typeof(IUIVisibilityChanged<>), typeof(UIVisibilityChangedInvoker<>));
            }

            m_LoadedInvokers = loadedInvokers;
            m_OpenInvokers = openInvokers;
            m_UpdateInvokers = updateInvokers;
            m_CloseInvokers = closeInvokers;
            m_DepthChangedInvokers = depthChangedInvokers;
            m_VisibilityChangedInvokers = visibilityChangedInvokers;
            m_IsRegistered = true;
        }

        public void Loaded(Entity component)
        {
            ThrowIfInvalidComponent(component);

            if (!m_LoadedInvokers.TryGetValue(component.GetType(), out var invokers))
            {
                return;
            }

            for (var i = 0; i < invokers.Count && !component.IsDisposed; i++)
            {
                invokers[i].Invoke(component);
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

        public void DepthChanged(Entity component)
        {
            ThrowIfInvalidComponent(component);

            if (!m_DepthChangedInvokers.TryGetValue(component.GetType(), out var invokers))
            {
                return;
            }

            for (var i = 0; i < invokers.Count && !component.IsDisposed; i++)
            {
                invokers[i].Invoke(component);
            }
        }

        public void VisibilityChanged(Entity component)
        {
            ThrowIfInvalidComponent(component);

            if (!m_VisibilityChangedInvokers.TryGetValue(component.GetType(), out var invokers))
            {
                return;
            }

            for (var i = 0; i < invokers.Count && !component.IsDisposed; i++)
            {
                invokers[i].Invoke(component);
            }
        }
        
        public override void Dispose()
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
    }
}
