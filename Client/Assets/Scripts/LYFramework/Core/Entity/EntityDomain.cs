namespace LYFramework
{
    /// <summary>
    /// Entity 树的顶层管理节点，作为逻辑隔离使用
    /// </summary>
    public class EntityDomain : Entity
    {
        internal EntityDomain()
        {
            Domain = this;
        }
    }
}
