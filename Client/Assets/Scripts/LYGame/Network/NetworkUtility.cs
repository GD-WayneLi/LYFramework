using System;
using System.Collections.Generic;
using System.Net;
using LYFramework.Network;

namespace LYGame.Network
{
    public class NetworkUtility : INetworkUtility
    {
        readonly Dictionary<NetworkChannelType, ChannelContext> m_ChannelContexts = new();

        public void RegisterChannel(NetworkChannelType channelType, IPacketHelper packetHelper,
            IPacketDispatcher dispatcher = null)
        {
            if (packetHelper == null)
            {
                throw new ArgumentNullException(nameof(packetHelper));
            }

            if (packetHelper.HeaderLength <= 0)
            {
                throw new ArgumentException("packetHelper.HeaderLength need > 0", nameof(packetHelper));
            }

            if (HasChannel(channelType))
            {
                throw new InvalidOperationException($"Network channel {channelType} already exists");
            }

            var channel = NetworkChannelFactory.Create(channelType, packetHelper);
            m_ChannelContexts.Add(channelType, new ChannelContext(channel, dispatcher ?? new PacketDispatcher()));
        }

        public void UnregisterChannel(NetworkChannelType channelType)
        {
            if (!m_ChannelContexts.TryGetValue(channelType, out var channelContext))
            {
                return;
            }

            channelContext.Channel.Dispose();
            m_ChannelContexts.Remove(channelType);
        }

        public bool HasChannel(NetworkChannelType channelType)
        {
            return m_ChannelContexts.ContainsKey(channelType);
        }

        public void Connect(NetworkChannelType channelType, IPAddress ipAddress, int port)
        {
            GetRequiredContext(channelType).Channel.Connect(ipAddress, port);
        }

        public void Disconnect(NetworkChannelType channelType)
        {
            GetRequiredContext(channelType).Channel.Close();
        }

        public void Send<T>(NetworkChannelType channelType, T packet) where T : IPacket
        {
            GetRequiredContext(channelType).Channel.Send(packet);
        }

        public void AddHandler(NetworkChannelType channelType, int packetId, Action<IPacket> handler)
        {
            GetRequiredContext(channelType).Dispatcher.AddHandler(packetId, handler);
        }

        public void RemoveHandler(NetworkChannelType channelType, int packetId, Action<IPacket> handler)
        {
            GetRequiredContext(channelType).Dispatcher.RemoveHandler(packetId, handler);
        }

        public void Update()
        {
            foreach (var channelContext in m_ChannelContexts.Values)
            {
                channelContext.Channel.Update();
                channelContext.Dispatcher.Dispatch(channelContext.Channel);
            }
        }

        ChannelContext GetRequiredContext(NetworkChannelType channelType)
        {
            if (m_ChannelContexts.TryGetValue(channelType, out var channelContext))
            {
                return channelContext;
            }

            throw new InvalidOperationException($"Network channel {channelType} is not registered");
        }

        sealed class ChannelContext
        {
            public ChannelContext(INetworkChannel channel, IPacketDispatcher dispatcher)
            {
                Channel = channel ?? throw new ArgumentNullException(nameof(channel));
                Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            }

            public INetworkChannel Channel { get; }
            public IPacketDispatcher Dispatcher { get; }
        }
    }
}
