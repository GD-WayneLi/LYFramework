using System;
using System.Collections.Generic;
using LYFramework.Log;

namespace LYFramework
{
    /// <summary>
    /// 游戏的全局管理器，负责服务注册并持有管理 Scene 的 World。
    /// </summary>
    public abstract class GameManagerBase<T> : IGameManager, IDisposable where T : GameManagerBase<T>, new()
    {
        private static T _gameManager;
        private bool m_IsDisposed;

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

        private readonly Dictionary<Type, IUtility> m_Utilities = new();
        private readonly Dictionary<Type, ISystem> m_Systems = new();

        /// <summary>
        /// 获取由当前 GameManager 独占的 World。销毁 GameManager 时会一并销毁其中的全部 Scene。
        /// </summary>
        public World World { get; } = new();

        public bool IsDisposed => m_IsDisposed;

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

            instance.Init(this);
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
