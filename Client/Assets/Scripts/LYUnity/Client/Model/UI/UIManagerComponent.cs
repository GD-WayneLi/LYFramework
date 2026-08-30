using System.Collections.Generic;
using LYFramework;
using UnityEngine;

namespace LYUnity.UI
{
    /// <summary>
    /// 挂载在 UIManager Entity 上的管理器数据。
    /// </summary>
    public sealed class UIManagerComponent : Entity
    {
        internal readonly List<Entity> UIStack = new(50);
        internal readonly Dictionary<int, ILayerGroup> LayerGroups = new();

        internal UIEvent Event { get; set; }

        public GameObject UIRoot;
        
        public int Count => UIStack.Count;

        public IReadOnlyList<Entity> OpenedUIEntities => UIStack;
    }
}
