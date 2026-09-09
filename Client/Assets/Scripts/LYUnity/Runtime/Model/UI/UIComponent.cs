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
        public UIEvent Event;
        
        public UIState State;

        public string PackageName;
        
        public string Path;

        public int Layer;

        public int Depth;

        public bool IsVisible;

        public Entity LogicComponent;
        
        public object UserData;

        public GameObject Resource;
        
        public GameObject GameObject;
    }
}
