using System;

namespace LYFramework.ReferencePool
{
    public interface IReferenceCollector : IDisposable
    {
        /// <summary>
        /// 泛型方法Acquire的声明，用于获取指定类型的资源
        /// </summary>
        IReference Acquire();

        /// <summary>
        /// 释放指定对象的资源
        /// </summary>
        /// <param name="obj">需要释放资源的对象</param>
        void Release(IReference obj);
    }
}