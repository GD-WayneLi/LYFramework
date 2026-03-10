using System;
using System.Collections.Generic;

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
        
        public void RegisterModel<TModel>() where TModel : IModel, new()
        {
            var model = new TModel();
            m_Models.Add(typeof(TModel), model);
            
            model.Init(this);
        }

        public void RegisterUtility<TUtility>() where TUtility : IUtility, new()
        {
            m_Utilities.Add(typeof(TUtility), new TUtility());
        }

        public void RegisterSystem<TSystem>() where TSystem : ISystem, new()
        {
            var system = new TSystem();
            m_Systems.Add(typeof(TSystem), system);
            
            system.Init(this);
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