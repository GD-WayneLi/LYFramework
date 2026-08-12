using System;

namespace LYFramework.UI
{
    public enum UIState : byte
    {
        Created = 0,
        Loading = 1,
        Open = 2,
        Closing = 3,
        Closed = 4,
        Disposed = 5,
    }

    /// <summary>
    /// 挂载在 UI Entity 上的通用 UI 数据。
    /// </summary>
    public sealed class UIComponent : Entity
    {
        public UIState State { get; internal set; }

        public string Path { get; internal set; }

        public int Layer { get; internal set; }

        public int Depth { get; internal set; }

        public bool IsVisible { get; internal set; }

        public object UserData { get; internal set; }

        public object Resource { get; internal set; }

        public Type DataComponentType => DataComponent?.GetType();

        internal Entity DataComponent { get; set; }

        internal int LoadVersion { get; set; }

        internal bool WasOpened { get; set; }
    }
}
