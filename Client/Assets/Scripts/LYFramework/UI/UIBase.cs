using System;

namespace LYFramework.UI
{
    public abstract class UIBase
    {
        protected object m_UserData;
        
        private bool m_IsPrepared;
        
        public abstract int Layer { get; set; }
        
        public abstract int Depth { get; set; }
        
        public abstract bool IsVisible { get; set; }

        protected internal abstract void OnOpen();

        protected internal abstract void OnClose();

        protected abstract void OnUpdate();
        
        protected internal virtual void Load(string path, Action<UIBase> callback, object userData)
        {
            m_UserData = userData;
        }
        
        protected internal virtual void Update()
        {
            if (m_IsPrepared && IsVisible)
            {
                OnUpdate();
            }
        }

        protected internal virtual void Dispose()
        {
            OnClose();
        }
    }
}