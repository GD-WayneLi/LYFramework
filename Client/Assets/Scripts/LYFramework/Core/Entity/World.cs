using System;
using System.Collections.Generic;

namespace LYFramework
{
    /// <summary>
    /// Entity管理类
    /// </summary>
    public sealed class World : IDisposable
    {
        private readonly Dictionary<long, Entity> m_Entitys = new();

        private readonly List<EntityDomain> m_EntityDomains = new();
        
        public bool IsDisposed {get; private set;}
        
        private EntityEvent m_EntityEvent;

        internal World(EntityEvent entityEvent)
        {
            m_EntityEvent = entityEvent;
        }

        /// <summary>
        /// 销毁 EntityDomain 根节点及当前 World 仍管理的全部游离 Entity。重复调用安全。
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            foreach (var entityDomain in m_EntityDomains)
            {
                entityDomain.Dispose();
            }
            
            m_EntityDomains.Clear();
            m_Entitys.Clear();
        }

        public EntityDomain AddDomain()
        {
            var domain = new EntityDomain();
            
            m_EntityDomains.Add(domain);
            Add(domain);
            
            return domain;
        }
        
        public void Add(Entity entity)
        {
            m_Entitys.Add(entity.InstanceId, entity);
        }

        public void Remove(long instanceId)
        {
            m_Entitys.Remove(instanceId);
        }

        public Entity Get(long instanceId)
        {
            m_Entitys.TryGetValue(instanceId, out var entity);
            return entity;
        }
        
        internal void OnComponentAwake(Entity component)
        {
            m_EntityEvent?.Awake(component);
        }

        internal void OnComponentDispose(Entity component)
        {
            m_EntityEvent?.DisposeComponent(component);
        }
    }
}