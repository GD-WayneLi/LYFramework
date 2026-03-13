using System.Collections.Generic;

namespace LYFramework.ReferencePool
{
    public class ReferenceCollector<T> : IReferenceCollector where T : IReference, new()
    {
        private readonly Stack<T> m_Stack;

        public int Count => m_Stack.Count;
        public int UsingCount { get; private set; } = 0;

        public ReferenceCollector()
        {
            m_Stack = new Stack<T>();
        }

        public ReferenceCollector(int count)
        {
            m_Stack = new Stack<T>(count);
        }
        
        /// <summary>
        /// 泛型方法Acquire的声明，用于获取指定类型的资源
        /// </summary>
        /// <typeparam name="T">泛型类型参数，表示要获取的资源类型</typeparam>
        public T Acquire()
        {
            UsingCount++;

            if (!m_Stack.TryPop(out var obj))
                obj = new T();
            
            return obj;
        }

        /// <summary>
        /// 释放指定对象的资源
        /// </summary>
        /// <typeparam name="T">泛型类型，表示要释放的对象类型</typeparam>
        /// <param name="obj">需要释放资源的对象</param>
        public void Release(T obj)
        {
            if (obj == null)
            {
                return;
            }
            
            UsingCount--;
            
            obj.Clear();
            m_Stack.Push(obj);
        }
        
        IReference IReferenceCollector.Acquire()
        {
            return Acquire();
        }
        
        void IReferenceCollector.Release(IReference obj)
        {
            if (obj is T t)
            {
                Release(t);
            }
        }

        public void Dispose()
        {
            UsingCount = 0;
            m_Stack.Clear();
        }
    }
}