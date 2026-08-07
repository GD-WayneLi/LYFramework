using System;
using System.Collections.Generic;
using System.Threading;
using LYFramework.Log;

namespace LYFramework
{
	[Flags]
	public enum EntityStatus : byte
	{
		None = 0,
		IsComponent = 1,
		IsDisposed = 1 << 1,
	}

    /// <summary>
    /// ECS 中所有业务对象与组件的基类。
    /// Entity 只能通过父子关系或组件关系挂接到一棵 Entity 树中。
    /// </summary>
    public class Entity : IDisposable
    {
        private static long s_InstanceIdGenerator;

        private Dictionary<long, Entity> m_Children;
		public Dictionary<long, Entity> Children
		{
			get { return m_Children ??= new(); }
		}

        private Dictionary<Type, Entity> m_Components;
		public Dictionary<Type, Entity> Components
		{
			get { return m_Components ??= new(); }
		}

		private EntityStatus m_Status;

        protected Entity()
        {
        }

        /// <summary>
        /// Entity 的稳定标识。
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// 当前对象生命周期标识。销毁后为 0，可用于异步操作的有效性检查。
        /// </summary>
        public long InstanceId { get; private set; }

		private Entity m_Parent;
		public Entity Parent
		{
			get => m_Parent;
			private set
			{
				if (value == null)
				{
					throw new ArgumentNullException(GetType().Name);
				}

				if (ReferenceEquals(value, this))
				{
					throw new InvalidOperationException("An entity cannot be its own child.");
				}

				if (value.Domain == null)
				{
                    throw new Exception($"Parent domain is null: {GetType().Name}, {value.GetType().Name}");
				}

				if (m_Parent != null)
				{
					if (m_Parent == value)
					{
						LYLogger.Error($"Set parent repeat: {GetType().Name}");
						return;
					}
					m_Parent.OnRemoveChild(this);
				}

				m_Parent = value;
				m_Parent.OnAddChild(this);
				ChangeDomain(m_Parent.Domain);
			}
		}

		private Entity ComponentParent
		{
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(GetType().Name);
				}

				if (ReferenceEquals(value, this))
				{
					throw new InvalidOperationException("An entity cannot be its own child.");
				}

				if (value.Domain == null)
				{
                    throw new Exception($"Parent domain is null: {GetType().Name}, {value.GetType().Name}");
				}

				if (m_Parent != null)
				{
					if (m_Parent == value)
					{
						LYLogger.Error($"Set parent repeat: {GetType().Name}");
						return;
					}
					m_Parent.OnRemoveComponent(this);
				}

				m_Parent = value;
				m_Parent.OnAddComponent(this);
				ChangeDomain(m_Parent.Domain);
			}
		}

		/// <summary>
		/// Entity 当前所属的逻辑域。游离 Entity 的 Domain 为 null。
		/// </summary>
		public EntityDomain Domain { get; protected set; }

        public bool IsDisposed
		{
			get
			{
				return (m_Status & EntityStatus.IsDisposed) > 0;
			}
			private set
			{
				if (value)
				{
					m_Status |= EntityStatus.IsDisposed;
				}
				else
				{
					m_Status &= ~EntityStatus.IsDisposed;
				}
			}
		}

		public bool IsComponent
		{
			get
			{
				return (m_Status & EntityStatus.IsComponent) > 0;
			}
			private set
			{
				if (value)
				{
					m_Status |= EntityStatus.IsComponent;
				}
				else
				{
					m_Status &= ~EntityStatus.IsComponent;
				}
			}
		}

		public int ChildCount => m_Children?.Count ?? 0;
        public int ComponentCount => m_Components?.Count ?? 0;

		static T Create<T>() where T : Entity
		{
			var entity = Activator.CreateInstance(typeof(T)) as T;
			if (entity == null)
			{
				throw new InvalidOperationException($"Unable to create entity: {typeof(T).FullName}");
			}

			entity.InstanceId = Interlocked.Increment(ref s_InstanceIdGenerator);
			entity.m_Status = EntityStatus.None;

            return entity;
		}

        public T AddChild<T>() where T : Entity
        {
            ThrowIfDisposed();

			var child = Create<T>();
			child.Parent = this;

            return child;
        }

        public Entity GetChild(long id)
        {
            if (m_Children != null && m_Children.TryGetValue(id, out var child))
            {
                return child;
            }

            return null;
        }

        public bool TryGetChild(long id, out Entity child)
        {
            if (m_Children != null)
            {
                return m_Children.TryGetValue(id, out child);
            }

            child = null;
            return false;
        }

        /// <summary>
        /// 从当前父节点移除。dispose 为 false 时，Entity 会成为未归属 EntityDomain 的游离对象。
        /// </summary>
        public bool RemoveChild(Entity child)
        {
            ThrowIfDisposed();

            if (child == null || child.IsComponent || !ReferenceEquals(child.Parent, this))
            {
                return false;
            }

            child.Dispose();
            return true;
        }

        public T AddComponent<T>() where T : Entity, new()
        {
            ThrowIfDisposed();

			if (HasComponent<T>())
			{
				throw new InvalidOperationException($"Component already exists: {typeof(T).Name}");
			}

			var component = Create<T>();
			component.IsComponent = true;

			try
			{
				component.ComponentParent = this;
				component.Domain.OnComponentAwake(component);
			}
			catch
			{
				component.Dispose();
				throw;
			}

            return component;
        }

        public T GetComponent<T>() where T : Entity
        {
            if (m_Components != null && m_Components.TryGetValue(typeof(T), out var component))
            {
                return (T)component;
            }

            return null;
        }

        public bool TryGetComponent<T>(out T component) where T : Entity
        {
            component = GetComponent<T>();
            return component != null;
        }

        public bool HasComponent<T>() where T : Entity
        {
            return m_Components != null && m_Components.ContainsKey(typeof(T));
        }

        public bool RemoveComponent<T>() where T : Entity
        {
            ThrowIfDisposed();

            if (m_Components == null || !m_Components.TryGetValue(typeof(T), out var component))
            {
                return false;
            }

			component.Dispose();

            return true;
        }

        /// <summary>
        /// 递归销毁所有组件和子节点，并从父节点及 World 索引中移除。
        /// </summary>
        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            InstanceId = 0;

            if (m_Components != null)
            {
                foreach (var component in m_Components.Values)
                {
                    component.Dispose();
                }

                m_Components.Clear();
				m_Components = null;
            }

            if (m_Children != null)
            {
                foreach (var child in m_Children.Values)
                {
                    child.Dispose();
                }

                m_Children.Clear();
				m_Children = null;
            }

            DetachFromParent();
        }

        private void DetachFromParent()
        {
            if (Parent == null)
            {
                return;
            }

            if (IsComponent)
            {
                Parent.m_Components?.Remove(GetType());
            }
            else
            {
				Parent.m_Children?.Remove(InstanceId);
            }

            m_Parent = null;
			Domain = null;
        }

        private void ChangeDomain(EntityDomain newDomain)
        {
            if (ReferenceEquals(Domain, newDomain))
            {
                return;
            }

            SetDomainRecursively(newDomain);
        }

        private void SetDomainRecursively(EntityDomain domain)
        {
			if (Domain == null)
			{
				// todo:设置InstanceId
			}

            Domain = domain;

            if (m_Components != null)
            {
                foreach (var component in m_Components.Values)
                {
                    component.SetDomainRecursively(domain);
                }
            }

            if (m_Children != null)
            {
                foreach (var child in m_Children.Values)
                {
                    child.SetDomainRecursively(domain);
                }
            }
        }

		private void OnAddChild(Entity entity)
		{
			Children.Add(entity.InstanceId, entity);
		}

		private void OnRemoveChild(Entity entity)
		{
			if (m_Children == null)
				return;

			m_Children.Remove(entity.InstanceId);
		}

		private void OnAddComponent(Entity entity)
		{
			Components.Add(entity.GetType(), entity);
		}

		private void OnRemoveComponent(Entity entity)
		{
			if (m_Components == null)
				return;

			m_Components.Remove(entity.GetType());
		}

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
