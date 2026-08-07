using System;

namespace LYFramework
{
    /// <summary>
    /// Entity 树的顶层管理节点，作为逻辑隔离使用
    /// </summary>
    public class EntityDomain : Entity
    {
        private Action<Entity> m_ComponentAwakeHandler;

        public EntityDomain()
        {
            Domain = this;
        }

		internal void SetComponentAwakeHandler(Action<Entity> componentAwakeHandler)
		{
			m_ComponentAwakeHandler = componentAwakeHandler;
		}

		internal void OnComponentAwake(Entity component)
		{
			m_ComponentAwakeHandler?.Invoke(component);
		}

		public override void Dispose()
		{
			m_ComponentAwakeHandler = null;
			base.Dispose();
		}
	}
}
