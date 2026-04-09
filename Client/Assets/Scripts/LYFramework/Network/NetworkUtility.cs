using System;
using System.Collections.Generic;

namespace LYFramework.Network
{
    public class NetworkUtility : INetworkUtility
    {
        readonly Dictionary<NetworkChannelType, INetworkChannel> m_NetworkChannels = new();

        public INetworkChannel CreateNetworkChannel<T>(NetworkChannelType channelType, IPacketHelper packetHelper) where T : INetworkChannel, new()
        {
            if (packetHelper == null)
            {
                throw new Exception($"packetHelper is null");
            }

            if (packetHelper.HeaderLength <= 0)
            {
                throw new Exception("packetHelper.HeaderLength need > 0");
            }
            
            if (HasNetworkChannel(channelType))
            {
                throw new Exception($"Network channel {channelType} already exists");
            }

            var channel = new T();
            if (!channel.Init(packetHelper))
            {
                throw new Exception($"Failed to initialize network channel {channelType}");
            }
            m_NetworkChannels.Add(channelType, channel);
            return channel;
        }

        public void DestroyNetworkChannel(NetworkChannelType channelType)
        {
            if (m_NetworkChannels.TryGetValue(channelType, out var channel))
            {
                channel.Dispose();
                m_NetworkChannels.Remove(channelType);
            }
        }

        public bool HasNetworkChannel(NetworkChannelType channelType)
        {
            return m_NetworkChannels.ContainsKey(channelType);
        }

        public INetworkChannel GetNetworkChannel(NetworkChannelType channelType)
        {
            m_NetworkChannels.TryGetValue(channelType, out var channel);
            return channel;
        }

        public void Update()
        {
            foreach (var kv in m_NetworkChannels)
            {
                kv.Value.Update();
            }
        }
    }
}