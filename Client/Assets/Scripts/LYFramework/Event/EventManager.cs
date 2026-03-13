using System;
using System.Collections.Generic;

namespace LYFramework.Event
{
    public class EventManager : IEventManager
    {
        private Dictionary<Type, IEventWrapper> m_EventDic = new();
        private List<PostEventWrapper> m_EventList = new();
        
        public void AddListener<T>(EventHandler<T> listener) where T : IEvent
        {
            if (listener == null)
            {
                // todo:添加报错
                return;
            }
            var type = typeof(T);
            if (!m_EventDic.TryGetValue(type, out var wrapper))
            {
                wrapper = new EventWrapper<T>();
                m_EventDic[type] = wrapper;
            }
            
            ((EventWrapper<T>)wrapper).Add(listener);
        }

        public void RemoveListener<T>(EventHandler<T> listener) where T : IEvent
        {
            if (listener == null)
            {
                // todo:添加报错
                return;
            }
            var type = typeof(T);
            if (!m_EventDic.TryGetValue(type, out var wrapper))
            {
                return;
            }
            
            ((EventWrapper<T>)wrapper).Remove(listener);
        }

        
        public void Send<T>(object sender, T e) where T : IEvent
        {
            if (m_EventDic.TryGetValue(typeof(T), out var wrapper))
            {
                wrapper.Trigger(sender, e);
            }
        }

        public void Post<T>(object sender, T e) where T : IEvent
        {
            m_EventList.Add(new PostEventWrapper(sender, e));
        }

        public void Dispose()
        {
            m_EventDic.Clear();
        }

        void IEventManager.Update()
        {
            if (m_EventList.Count > 0)
            {
                for (int i = m_EventList.Count; i >= 0; i--)
                {
                    var e = m_EventList[i];
                    Send(e.Sender, e.Event);
                    m_EventList.RemoveAt(i);
                }
            }
        }
    }
}
