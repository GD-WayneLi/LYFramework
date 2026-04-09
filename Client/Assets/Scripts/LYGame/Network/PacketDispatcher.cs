using System;
using System.Collections.Generic;
using LYFramework.Network;

namespace LYGame.Network
{
    public class PacketDispatcher : IPacketDispatcher
    {
        readonly Dictionary<int, Action<IPacket>> m_PacketHandlers = new();

        public void AddHandler(int packetId, Action<IPacket> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (m_PacketHandlers.TryGetValue(packetId, out var existingHandler))
            {
                m_PacketHandlers[packetId] = existingHandler + handler;
                return;
            }

            m_PacketHandlers.Add(packetId, handler);
        }

        public void RemoveHandler(int packetId, Action<IPacket> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!m_PacketHandlers.TryGetValue(packetId, out var existingHandler))
            {
                return;
            }

            existingHandler -= handler;
            if (existingHandler == null)
            {
                m_PacketHandlers.Remove(packetId);
                return;
            }

            m_PacketHandlers[packetId] = existingHandler;
        }

        public void Dispatch(INetworkChannel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            while (channel.TryDequeuePacket(out var packet))
            {
                if (m_PacketHandlers.TryGetValue(packet.Id, out var handler))
                {
                    handler(packet);
                }
            }
        }
    }
}
