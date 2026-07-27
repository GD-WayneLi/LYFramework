using System;
using System.Collections.Generic;
using LYFramework.Log;
using UnityEngine;

namespace LYFramework
{
    /// <summary>
    /// 游戏的全局管理器，同时也是 Entity World 的根 Scene。
    /// </summary>
    public abstract class GameManagerBase<T> : Entity, IGameManager where T : GameManagerBase<T>, new()
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

        private readonly Dictionary<Type, IUtility> m_Utilities = new();
        private readonly Dictionary<Type, ISystem> m_Systems = new();

        /// <summary>
        /// GameManager 本身就是 World 根节点。通过接口访问时无需了解具体的 GameManager 类型。
        /// </summary>
        public Entity World => this;

        public abstract void Init();

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            foreach (var system in m_Systems)
            {
                system.Value.Dispose();
            }

            m_Systems.Clear();
            m_Utilities.Clear();

            // 递归销毁挂载在 World 下的全部 Component 和 Child Entity，
            // 并清理 Scene 的 Entity 索引。
            base.Dispose();

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
