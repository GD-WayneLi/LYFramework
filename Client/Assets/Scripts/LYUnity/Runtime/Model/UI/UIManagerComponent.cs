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
        public readonly List<Entity> UIStack = new(50);
        public readonly Dictionary<int, ILayerGroup> LayerGroups = new();

        public UIEvent Event { get; set; }

        public GameObject UIRootSrc;
        public GameObject UIRoot;
        public Transform UIParent;
        
        public int Count => UIStack.Count;

        public IReadOnlyList<Entity> OpenedUIEntities => UIStack;
    }
}
