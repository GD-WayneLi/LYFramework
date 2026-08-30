namespace LYFramework
{
    /// <summary>
    /// 接收指定 Component 开始销毁时的同步生命周期回调。
    /// 回调执行时 Component 已标记为 IsDisposed，但仍保有 Parent、Domain 和 InstanceId。
    /// </summary>
    public interface ISystemDispose<T> where T : Entity
    {
        void Dispose(T component);
    }
    
    internal sealed class SystemDisposeInvoker<T> : ISystemEventInvoker where T : Entity
    {
        private readonly ISystemDispose<T> m_System;

        public SystemDisposeInvoker(object system)
        {
            m_System = (ISystemDispose<T>)system;
        }

        public void Invoke(Entity component)
        {
            m_System.Dispose((T)component);
        }
    }
}
