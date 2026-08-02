using System;
using System.Collections.Generic;

namespace LYFramework
{
    /// <summary>
    /// Entity 树的顶层管理节点，作为逻辑隔离使用
    /// </summary>
    public class EntityDomain : Entity
    {
        public EntityDomain()
        {
            Domain = this;
        }
	}
}
