using System.Collections.Generic;
using LYFramework;
using LYFramework.Resource;

namespace LYUnity.UI
{
    /// <summary>
    /// 挂载在 UIManager Entity 上的管理器数据。
    /// </summary>
    public sealed class UIManagerComponent : Entity
    {
        internal readonly List<Entity> UIStack = new(50);
        internal readonly Dictionary<int, ILayerGroup> LayerGroups = new();

        internal UILifecycle Lifecycle { get; set; }
        
        internal bool IsClosingAll { get; set; }

        public int Count => UIStack.Count;

        public IReadOnlyList<Entity> OpenedUIEntities => UIStack;
    }
}
