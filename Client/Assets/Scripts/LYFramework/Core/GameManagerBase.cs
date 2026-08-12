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
        private bool m_IsDisposing;
        public bool IsDisposed => m_IsDisposed;

        /// <summary>
        /// 获取由当前 GameManager 独占的 World。销毁 GameManager 时会一并销毁根 EntityDomain 与全部 Entity。
        /// </summary>
        public World World { get; }

        private readonly Dictionary<Type, IUtility> m_Utilities = new();
        private readonly Dictionary<Type, ISystem> m_Systems = new();
        private readonly List<ISystem> m_SystemRegistrationOrder = new();
        private readonly EntityLifecycle m_EntityLifecycle = new();

        protected GameManagerBase()
        {
            World = new World(m_EntityLifecycle);
        }

        public abstract void Init();

        public void Update()
        {
            ThrowIfUnavailable();
            m_EntityLifecycle.Update();
        }

        public virtual void Dispose()
        {
            if (m_IsDisposed || m_IsDisposing)
            {
                return;
            }

            m_IsDisposing = true;
            List<Exception> exceptions = null;

            try
            {
                World.Dispose();
            }
            catch (Exception exception)
            {
                exceptions = new List<Exception> { exception };
            }

            for (var i = m_SystemRegistrationOrder.Count - 1; i >= 0; i--)
            {
                try
                {
                    m_SystemRegistrationOrder[i].Dispose();
                }
                catch (Exception exception)
                {
                    (exceptions ??= new List<Exception>()).Add(exception);
                }
            }

            m_EntityLifecycle.Dispose();
            m_SystemRegistrationOrder.Clear();
            m_Systems.Clear();
            m_Utilities.Clear();

            m_IsDisposed = true;
            m_IsDisposing = false;
            _gameManager = null;

            if (exceptions != null)
            {
                throw new AggregateException($"GameManager disposal failed: {GetType().Name}", exceptions);
            }
        }

        public void RegisterUtility<TUtility>(TUtility instance = default) where TUtility : IUtility
        {
            ThrowIfUnavailable();

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
            ThrowIfUnavailable();

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
                m_EntityLifecycle.RegisterSystem(instance);
                m_SystemRegistrationOrder.Add(instance);
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
                    throw new AggregateException($"System initialization and rollback both failed: {type.Name}", initException, disposeException);
                }

                throw;
            }
        }

        private void ThrowIfUnavailable()
        {
            if (m_IsDisposed || m_IsDisposing)
            {
                throw new ObjectDisposedException(GetType().Name);
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

        public IReadOnlyList<ISystem> GetSystems()
        {
            ThrowIfUnavailable();
            return m_SystemRegistrationOrder.ToArray();
        }
    }
}
