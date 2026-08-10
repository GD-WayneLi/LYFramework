namespace LYFramework
{
    /// <summary>
    /// 接收指定 Component 完成挂载后的同步生命周期回调。
    /// 回调发生在 AddComponent 的调用线程中，此时 Component 已拥有 Parent 和 Domain。
    /// </summary>
    public interface ISystemAwake<T> where T : Entity
    {
        /// <summary>
        /// 初始化刚完成挂载的 Component。抛出异常将使本次添加操作回滚，
        /// 但不会触发 Component 的 Dispose 生命周期。
        /// </summary>
        void Awake(T component);
    }

    internal interface ISystemAwakeInvoker
    {
        void Invoke(Entity component);
    }

    internal sealed class SystemAwakeInvoker<T> : ISystemAwakeInvoker where T : Entity
    {
        private readonly ISystemAwake<T> m_System;

        public SystemAwakeInvoker(object system)
        {
            m_System = (ISystemAwake<T>)system;
        }

        public void Invoke(Entity component)
        {
            m_System.Awake((T)component);
        }
    }
}
