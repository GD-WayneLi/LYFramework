using System;
using System.Collections.Generic;
using LYFramework.Log;

namespace LYFramework
{
    /// <summary>
    /// 游戏的全局管理器，负责服务注册并持有管理 Entity 与唯一 EntityDomain 根节点的 World。
    /// </summary>
    public abstract class GameManagerBase<T> : IGameManager, IDisposable where T : GameManagerBase<T>, new()
    {
        private static T _gameManager;
        public static T Instance
        {
            get
            {
                if (_gameManager == null)
                {
                    _gameManager = new T();
                }
                return _gameManager;
            }
        }

        private bool m_IsDisposed;
        public bool IsDisposed => m_IsDisposed;

        /// <summary>
        /// 获取由当前 GameManager 独占的 World。销毁 GameManager 时会一并销毁根 EntityDomain 与全部 Entity。
        /// </summary>
        public World World { get; }

        private readonly Dictionary<Type, IUtility> m_Utilities = new();
        private readonly Dictionary<Type, ISystem> m_Systems = new();
        private readonly Dictionary<Type, List<ISystemAwakeInvoker>> m_SystemAwakes = new();

        protected GameManagerBase()
        {
            World = new World(OnComponentAwake);
        }

        public abstract void Init();

        public virtual void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;

            foreach (var system in m_Systems)
            {
                system.Value.Dispose();
            }

            m_Systems.Clear();
            m_SystemAwakes.Clear();
            m_Utilities.Clear();

            World.Dispose();

            _gameManager = null;
        }

        public void RegisterUtility<TUtility>(TUtility instance = default) where TUtility : IUtility
        {
            var type = typeof(TUtility);
            if (instance == null)
            {
                if (!type.IsClass)
                {
                    LYLogger.Error($"this type not class: {type.Name}");
                    return;
                }

                instance = Activator.CreateInstance<TUtility>();
            }
            
            if (!m_Utilities.TryAdd(type, instance))
            {
                LYLogger.Error($"this type is already add: {type.Name}");
            }
        }

        public void RegisterSystem<TSystem>(TSystem instance = default) where TSystem : ISystem
        {
            var type = typeof(TSystem);
            if (instance == null)
            {
                if (!type.IsClass)
                {
                    LYLogger.Error($"this type not class: {type.Name}");
                    return;
                }

                instance = Activator.CreateInstance<TSystem>();
            }

            if (!m_Systems.TryAdd(type, instance))
            {
                LYLogger.Error($"this type is already add: {type.Name}");
                return;
            }

            try
            {
                instance.Init(this);
                RegisterSystemAwakes(instance);
            }
            catch (Exception initException)
            {
                m_Systems.Remove(type);

                try
                {
                    instance.Dispose();
                }
                catch (Exception disposeException)
                {
                    throw new AggregateException(
                        $"System initialization and rollback both failed: {type.Name}",
                        initException,
                        disposeException);
                }

                throw;
            }
        }

        private void RegisterSystemAwakes(ISystem system)
        {
            var interfaceTypes = system.GetType().GetInterfaces();
            var invokers = new List<(Type ComponentType, ISystemAwakeInvoker Invoker)>();

            foreach (var interfaceType in interfaceTypes)
            {
                if (!interfaceType.IsGenericType ||
                    interfaceType.GetGenericTypeDefinition() != typeof(ISystemAwake<>))
                {
                    continue;
                }

                var componentType = interfaceType.GetGenericArguments()[0];
                var invokerType = typeof(SystemAwakeInvoker<>).MakeGenericType(componentType);
                var invoker = Activator.CreateInstance(invokerType, system) as ISystemAwakeInvoker;
                if (invoker == null)
                {
                    throw new InvalidOperationException(
                        $"Unable to create system awake invoker: {system.GetType().Name}, {componentType.Name}");
                }

                invokers.Add((componentType, invoker));
            }

            foreach (var item in invokers)
            {
                if (!m_SystemAwakes.TryGetValue(item.ComponentType, out var componentInvokers))
                {
                    componentInvokers = new List<ISystemAwakeInvoker>();
                    m_SystemAwakes.Add(item.ComponentType, componentInvokers);
                }

                componentInvokers.Add(item.Invoker);
            }
        }

        private void OnComponentAwake(Entity component)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            if (!m_SystemAwakes.TryGetValue(component.GetType(), out var invokers))
            {
                return;
            }

            var count = invokers.Count;
            for (var i = 0; i < count; i++)
            {
                invokers[i].Invoke(component);
            }
        }

        public TUtility GetUtility<TUtility>() where TUtility : class, IUtility
        {
            return m_Utilities.GetValueOrDefault(typeof(TUtility)) as TUtility;
        }

        public TSystem GetSystem<TSystem>() where TSystem : class, ISystem
        {
            return m_Systems.GetValueOrDefault(typeof(TSystem)) as TSystem;
        }
    }
}
