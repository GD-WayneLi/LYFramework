using System;
using System.Collections.Generic;

namespace LYFramework.Network
{
    public class NetworkManager
    {
        readonly Dictionary<string, INetworkChannel> m_NetworkChannels = new();

        public INetworkChannel CreateNetworkChannel<T>(string channelName, IPacketHelper packetHelper, IPacketDispatcher dispatcher) where T : INetworkChannel, new()
        {
            if (packetHelper == null)
            {
                throw new Exception($"packetHelper is null");
            }

            if (packetHelper.HeaderLength <= 0)
            {
                throw new Exception("packetHelper.HeaderLength need > 0");
            }

            if (dispatcher == null)
            {
                throw new Exception($"dispatcher is null");
            }
            
            if (HasNetworkChannel(channelName))
            {
                throw new Exception($"Network channel {channelName} already exists");
            }

            var channel = new T();
            if (!channel.Init(channelName, packetHelper, dispatcher))
            {
                throw new Exception($"Failed to initialize network channel {channelName}");
            }
            m_NetworkChannels.Add(channelName, channel);
            return channel;
        }

        public void DestroyNetworkChannel(string channelName)
        {
            if (m_NetworkChannels.TryGetValue(channelName, out var channel))
            {
                channel.Dispose();
                m_NetworkChannels.Remove(channelName);
            }
        }

        public bool HasNetworkChannel(string channelName)
        {
            return m_NetworkChannels.ContainsKey(channelName);
        }

        public INetworkChannel GetNetworkChannel(string channelName)
        {
            m_NetworkChannels.TryGetValue(channelName, out var channel);
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