using System;
using LYFramework;
using UnityEngine;

namespace LYUnity.UI
{
    public enum UIState : byte
    {
        Created = 0,
        Loading = 1,
        Open = 2,
        Disposed = 3
    }

    /// <summary>
    /// 挂载在 UI Entity 上的通用 UI 数据。
    /// </summary>
    public sealed class UIComponent : Entity
    {
        internal UILifecycle Lifecycle { get; set; }
        
        public UIState State { get; internal set; }

        public string Path { get; internal set; }

        public int Layer { get; internal set; }

        public int Depth { get; internal set; }

        public bool IsVisible { get; internal set; }

        public Entity LogicComponent { get; internal set; }
        
        public object UserData { get; internal set; }

        public object Resource { get; internal set; }
        
        public GameObject GameObject { get; internal set; }
    }
}
