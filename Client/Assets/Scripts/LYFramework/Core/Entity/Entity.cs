using System;
using System.Collections.Generic;
using System.Threading;

namespace LYFramework
{
    /// <summary>
    /// ECS 中所有业务对象与组件的基类。
    /// Entity 只能通过父子关系或组件关系挂接到一棵 Entity 树中。
    /// </summary>
    public class Entity : IDisposable
    {
        private static long s_IdGenerator;
        private static long s_InstanceIdGenerator;

        private Dictionary<long, Entity> m_Children;
        private Dictionary<Type, Entity> m_Components;
        private bool m_IsDisposed;
        private bool m_IsComponent;

        public Entity()
        {
            Id = Interlocked.Increment(ref s_IdGenerator);
            InstanceId = Interlocked.Increment(ref s_InstanceIdGenerator);
        }

        /// <summary>
        /// Entity 的稳定标识。当前由框架自动生成，后续可由 EntityFactory 扩展业务 Id。
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// 当前对象生命周期标识。销毁后为 0，可用于异步操作的有效性检查。
        /// </summary>
        public long InstanceId { get; private set; }

        public Entity Parent { get; private set; }

        /// <summary>
        /// Entity 当前所属的顶层 Scene。游离 Entity 的 Scene 为 null。
        /// </summary>
        public Scene Scene { get; protected set; }

        public bool IsDisposed => m_IsDisposed;
        public bool IsComponent => m_IsComponent;
        public int ChildCount => m_Children?.Count ?? 0;
        public int ComponentCount => m_Components?.Count ?? 0;

        public IEnumerable<Entity> Children
        {
            get
            {
                if (m_Children == null)
                {
                    return Array.Empty<Entity>();
                }

                return m_Children.Values;
            }
        }

        public IEnumerable<Entity> Components
        {
            get
            {
                if (m_Components == null)
                {
                    return Array.Empty<Entity>();
                }

                return m_Components.Values;
            }
        }

        public T AddChild<T>() where T : Entity, new()
        {
            return AddChild(new T());
        }

        /// <summary>
        /// 添加子节点。若 child 已有父节点，会将其迁移到当前节点。
        /// </summary>
        public T AddChild<T>(T child) where T : Entity
        {
            ThrowIfDisposed();

            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            child.ThrowIfDisposed();

            if (child is Scene)
            {
                throw new InvalidOperationException("Scene cannot be added as a child entity.");
            }

            if (child.m_IsComponent)
            {
                throw new InvalidOperationException($"Component '{child.GetType().Name}' cannot be added as a child entity.");
            }

            if (ReferenceEquals(child, this))
            {
                throw new InvalidOperationException("An entity cannot be its own child.");
            }

            for (Entity current = this; current != null; current = current.Parent)
            {
                if (ReferenceEquals(current, child))
                {
                    throw new InvalidOperationException("Adding this child would create a circular entity hierarchy.");
                }
            }

            if (ReferenceEquals(child.Parent, this))
            {
                return child;
            }

            var oldScene = child.Scene;
            child.DetachFromParent();

            m_Children ??= new Dictionary<long, Entity>();
            if (!m_Children.TryAdd(child.Id, child))
            {
                throw new InvalidOperationException($"Child entity id '{child.Id}' already exists on '{GetType().Name}'.");
            }

            child.Parent = this;
            if (!ReferenceEquals(oldScene, Scene))
            {
                child.ChangeScene(Scene);
            }

            return child;
        }

        public void Reparent(Entity newParent)
        {
            if (newParent == null)
            {
                throw new ArgumentNullException(nameof(newParent));
            }

            if (m_IsComponent)
            {
                throw new InvalidOperationException("A component cannot be reparented as a child entity.");
            }

            newParent.AddChild(this);
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
        /// 从当前父节点移除。dispose 为 false 时，Entity 会成为未归属 Scene 的游离对象。
        /// </summary>
        public bool RemoveChild(Entity child)
        {
            ThrowIfDisposed();

            if (child == null || child.m_IsComponent || !ReferenceEquals(child.Parent, this))
            {
                return false;
            }

            child.Dispose();
            return true;
        }

        public T AddComponent<T>() where T : Entity, new()
        {
            return AddComponent(new T());
        }

        /// <summary>
        /// 添加组件。一个 Entity 默认只能拥有一个相同运行时类型的组件。
        /// </summary>
        public T AddComponent<T>(T component) where T : Entity
        {
            ThrowIfDisposed();

            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            component.ThrowIfDisposed();

            if (component is Scene)
            {
                throw new InvalidOperationException("Scene cannot be added as a component.");
            }

            if (component.Parent != null || component.Scene != null)
            {
                throw new InvalidOperationException("Only a detached entity can be added as a component.");
            }

            var componentType = component.GetType();
            m_Components ??= new Dictionary<Type, Entity>();
            if (!m_Components.TryAdd(componentType, component))
            {
                throw new InvalidOperationException($"Component '{componentType.Name}' already exists on '{GetType().Name}'.");
            }

            component.m_IsComponent = true;
            component.Parent = this;
            component.ChangeScene(Scene);
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

        public bool RemoveComponent<T>(bool dispose = true) where T : Entity
        {
            ThrowIfDisposed();

            if (m_Components == null || !m_Components.TryGetValue(typeof(T), out var component))
            {
                return false;
            }

            if (dispose)
            {
                component.Dispose();
            }
            else
            {
                component.DetachFromParent();
                component.ChangeScene(null);
                component.m_IsComponent = false;
            }

            return true;
        }

        /// <summary>
        /// 递归销毁所有组件和子节点，并从父节点及 Scene 索引中移除。
        /// </summary>
        public virtual void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;

            if (m_Components != null)
            {
                var components = new List<Entity>(m_Components.Values);
                foreach (var component in components)
                {
                    component.Dispose();
                }

                m_Components.Clear();
            }

            if (m_Children != null)
            {
                var children = new List<Entity>(m_Children.Values);
                foreach (var child in children)
                {
                    child.Dispose();
                }

                m_Children.Clear();
            }

            DetachFromParent();
            ChangeScene(null);

            m_IsComponent = false;
            InstanceId = 0;
        }

        private void DetachFromParent()
        {
            if (Parent == null)
            {
                return;
            }

            if (m_IsComponent)
            {
                Parent.m_Components?.Remove(GetType());
            }
            else
            {
                Parent.m_Children?.Remove(Id);
            }

            Parent = null;
        }

        private void ChangeScene(Scene newScene)
        {
            if (ReferenceEquals(Scene, newScene))
            {
                return;
            }

            Scene?.UnregisterTree(this);
            SetSceneRecursively(newScene);
            newScene?.RegisterTree(this);
        }

        private void SetSceneRecursively(Scene scene)
        {
            Scene = scene;

            if (m_Components != null)
            {
                foreach (var component in m_Components.Values)
                {
                    component.SetSceneRecursively(scene);
                }
            }

            if (m_Children != null)
            {
                foreach (var child in m_Children.Values)
                {
                    child.SetSceneRecursively(scene);
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
