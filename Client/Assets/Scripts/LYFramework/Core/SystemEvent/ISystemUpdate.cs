namespace LYFramework
{
    /// <summary>
    /// 接收指定存活 Component 的逐帧同步生命周期回调。
    /// </summary>
    public interface ISystemUpdate<T> where T : Entity
    {
        void Update(T component);
    }
    
    internal sealed class SystemUpdateInvoker<T> : ISystemEventInvoker where T : Entity
    {
        private readonly ISystemUpdate<T> m_System;

        public SystemUpdateInvoker(object system)
        {
            m_System = (ISystemUpdate<T>)system;
        }

        public void Invoke(Entity component)
        {
            m_System.Update((T)component);
        }
    }
}
