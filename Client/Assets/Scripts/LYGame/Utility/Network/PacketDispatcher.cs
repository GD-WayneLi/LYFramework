using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LYFramework.Log;
using LYFramework.Network;
using LYFramework.ReferencePool;

namespace LYGame.Utility.Network
{
    public class PacketDispatcher : IPacketDispatcher
    {
        readonly Dictionary<int, Action<IPacket>> m_PacketHandlers = new();
        readonly ConcurrentQueue<IPacket> m_PacketQueue = new();
        
        public void AddHandler(IPacketHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (m_PacketHandlers.ContainsKey(handler.Id))
            {
                m_PacketHandlers[handler.Id] += handler.HandlePacket;
                return;
            }

            m_PacketHandlers.Add(handler.Id, handler.HandlePacket);
        }

        public void RemoveHandler(IPacketHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!m_PacketHandlers.ContainsKey(handler.Id))
            {
                return;
            }

            m_PacketHandlers[handler.Id] -= handler.HandlePacket;
            if (m_PacketHandlers[handler.Id] == null)
            {
                m_PacketHandlers.Remove(handler.Id);
            }
        }

        public void Dispatch(IPacket packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            m_PacketQueue.Enqueue(packet);
        }

        public void Update()
        {
            while (m_PacketQueue.TryDequeue(out var packet))
            {
                if (!m_PacketHandlers.TryGetValue(packet.Id, out var handler))
                {
                    ReferencePool.Release(packet);
                    continue;
                }
                try
                {
                    handler?.Invoke(packet);
                }
                catch (Exception e)
                {
                    LYLogger.Error($"消息处理异常, packetId = {packet.Id}, {e}");
                }
                
                ReferencePool.Release(packet);
            }
        }

        public void Dispose()
        {
            foreach (var packet in m_PacketQueue)
            {
                ReferencePool.Release(packet);
            }
            
            m_PacketQueue.Clear();
            m_PacketHandlers.Clear();
        }
    }
}
