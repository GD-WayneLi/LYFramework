using System;
using System.Collections.Generic;

namespace LYFramework
{
    public sealed class World : IDisposable
    {
		private Dictionary<long, Entity> m_Entitys = new();

        private bool m_IsDisposed;

        public World() : this(new EntityDomain())
        {
        }

        public World(EntityDomain domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException(nameof(domain));
            }

            if (domain.IsDisposed)
            {
                throw new ObjectDisposedException(domain.GetType().Name);
            }

            Domain = domain;
        }

        /// <summary>
        /// 获取当前 World 唯一的 EntityDomain 根节点。
        /// </summary>
        public EntityDomain Domain { get; }

        /// <summary>
        /// 销毁 EntityDomain 根节点及当前 World 仍管理的全部游离 Entity。重复调用安全。
        /// </summary>
        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            Domain.Dispose();
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
    }
}
