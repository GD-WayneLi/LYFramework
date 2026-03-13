using System;
using System.Collections.Generic;

namespace LYFramework.ReferencePool
{
    public static class ReferencePool
    {
        private static Dictionary<Type, IReferenceCollector> m_Pool = new();
        
        /// <summary>
        /// 设置泛型集合的容量
        /// </summary>
        /// <typeparam name="T">集合中元素的类型</typeparam>
        /// <param name="count">要设置的新容量值</param>
        public static void SetCapacity<T>(int count) where T : class, IReference, new()
        {
            var type = typeof(T);
            if (!m_Pool.TryGetValue(type, out var collector))
            {
                collector = new ReferenceCollector<T>(count);
                m_Pool[type] = collector;
            }
        }

        /// <summary>
        /// 泛型方法Acquire，用于获取指定类型的资源
        /// </summary>
        /// <typeparam name="T">泛型类型参数，表示要获取的资源类型</typeparam>
        public static T Acquire<T>() where T : class, IReference, new()
        {
            var type = typeof(T);
            if (!m_Pool.TryGetValue(type, out var collector))
            {
                collector = new ReferenceCollector<T>();
                m_Pool[type] = collector;
            }

            return (T)collector.Acquire();
        }

        /// <summary>
        /// 释放指定对象的资源
        /// </summary>
        /// <typeparam name="T">泛型类型，表示要释放的对象类型</typeparam>
        /// <param name="obj">需要释放资源的对象</param>
        public static void Release<T>(T obj) where T : class, IReference, new()
        {
            var type = typeof(T);
            if (!m_Pool.TryGetValue(type, out var collector))
            {
                throw new Exception($"this type not Acquire:{type}");
            }

            collector.Release(obj);
        }
        
        public static void Clear()
        {
            foreach (var kv in m_Pool)
            {
                kv.Value.Dispose();
            }
            
            m_Pool.Clear();
        }
    }
}