using System;
using System.Collections.Generic;

namespace LYFramework
{
    /// <summary>
    /// Entity 树的顶层管理节点，同时维护 Scene 内全部 Entity 的 Id 索引。
    /// </summary>
    public class Scene : Entity
    {
        private readonly Dictionary<long, Entity> m_Entities = new();

        public Scene()
        {
            Scene = this;
        }

        /// <summary>
        /// 获取当前 Scene 所属的 World。未加入 World 时为 null。
        /// </summary>
        public World World { get; private set; }

        public int EntityCount => m_Entities.Count;

        public Entity GetEntity(long id)
        {
            m_Entities.TryGetValue(id, out var entity);
            return entity;
        }

        public T GetEntity<T>(long id) where T : Entity
        {
            return GetEntity(id) as T;
        }

        public bool TryGetEntity(long id, out Entity entity)
        {
            return m_Entities.TryGetValue(id, out entity);
        }

        internal void AttachWorld(World world)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            if (World != null)
            {
                throw new InvalidOperationException("The Scene already belongs to a World.");
            }

            World = world;
        }

        internal void DetachWorld(World world)
        {
            if (ReferenceEquals(World, world))
            {
                World = null;
            }
        }

        internal void RegisterTree(Entity root)
        {
            Register(root);

            foreach (var component in root.Components)
            {
                RegisterTree(component);
            }

            foreach (var child in root.Children)
            {
                RegisterTree(child);
            }
        }

        internal void UnregisterTree(Entity root)
        {
            foreach (var component in root.Components)
            {
                UnregisterTree(component);
            }

            foreach (var child in root.Children)
            {
                UnregisterTree(child);
            }

            if (m_Entities.TryGetValue(root.Id, out var entity) && ReferenceEquals(entity, root))
            {
                m_Entities.Remove(root.Id);
            }
        }

        private void Register(Entity entity)
        {
            if (m_Entities.TryGetValue(entity.Id, out var current))
            {
                if (!ReferenceEquals(current, entity))
                {
                    throw new InvalidOperationException($"Entity id '{entity.Id}' already exists in the Scene.");
                }

                return;
            }

            m_Entities.Add(entity.Id, entity);
        }
    }
}
