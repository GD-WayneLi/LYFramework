namespace LYFramework
{
    /// <summary>
    /// Entity 树的顶层管理节点，作为逻辑隔离使用
    /// </summary>
    public class EntityDomain : Entity
    {
        private EntityLifecycle m_EntityLifecycle;

        public EntityDomain()
        {
            Domain = this;
        }

		internal void SetEntityLifecycle(EntityLifecycle entityLifecycle)
		{
			m_EntityLifecycle = entityLifecycle;
		}

		internal void OnComponentAwake(Entity component)
		{
			m_EntityLifecycle?.Awake(component);
		}

		internal void OnComponentDispose(Entity component)
		{
			m_EntityLifecycle?.DisposeComponent(component);
		}

		internal void OnComponentRollback(Entity component)
		{
			m_EntityLifecycle?.Forget(component);
		}

		public override void Dispose()
		{
			try
			{
				base.Dispose();
			}
			finally
			{
				m_EntityLifecycle = null;
			}
		}
	}
}
