using System;
using System.Collections.Generic;
using UnityEngine;

namespace LYFramework
{
    public abstract class GameManagerBase<T> : IGameManager where T : GameManagerBase<T>, new()
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

        private Dictionary<Type, IModel> m_Models = new Dictionary<Type, IModel>();
        private Dictionary<Type, IUtility> m_Utilities = new Dictionary<Type, IUtility>();
        private Dictionary<Type, ISystem> m_Systems = new Dictionary<Type, ISystem>();

        public abstract void Init();

        public virtual void Dispose()
        {
            foreach (var system in m_Systems)
            {
                system.Value.Dispose();
            }

            foreach (var model in m_Models)
            {
                model.Value.Dispose();
            }
            
            _gameManager = null;
        }

        public void RegisterModel<TModel>(TModel instance = default) where TModel : IModel
        {
            var type = typeof(TModel);
            if (instance == null)
            {
                if (!type.IsClass)
                {
                    Debug.LogError($"this type not class: {type.Name}");
                    return;
                }

                instance = Activator.CreateInstance<TModel>();
            }

            if (!m_Models.TryAdd(type, instance))
            {
                Debug.LogError($"this type is already add: {type.Name}");
                return;
            }

            instance.Init(this);
        }

        public void RegisterUtility<TUtility>(TUtility instance = default) where TUtility : IUtility
        {
            var type = typeof(TUtility);
            if (instance == null)
            {
                if (!type.IsClass)
                {
                    Debug.LogError($"this type not class: {type.Name}");
                    return;
                }

                instance = Activator.CreateInstance<TUtility>();
            }
            
            if (!m_Utilities.TryAdd(type, instance))
            {
                Debug.LogError($"this type is already add: {type.Name}");
            }
        }

        public void RegisterSystem<TSystem>(TSystem instance = default) where TSystem : ISystem
        {
            var type = typeof(TSystem);
            if (instance == null)
            {
                if (!type.IsClass)
                {
                    Debug.LogError($"this type not class: {type.Name}");
                    return;
                }

                instance = Activator.CreateInstance<TSystem>();
            }

            if (!m_Systems.TryAdd(type, instance))
            {
                Debug.LogError($"this type is already add: {type.Name}");
            }
            
            instance.Init(this);
        }

        public TModel GetModel<TModel>() where TModel : class, IModel
        {
            return m_Models.GetValueOrDefault(typeof(TModel)) as TModel;
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