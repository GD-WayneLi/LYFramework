using System;
using System.Net;
using LYFramework.Log;

namespace LYFramework.Network
{
    public class NetworkUtility : IUtility
    {
        private bool m_IsInit = false;
        private NetworkManager m_NetworkManager;
        
        public event Action<string, object> OnChannelConnected;
        public event Action<string> OnChannelClosed;
        public event Action<string, NetworkErrorCode, string> OnChannelError;
        
        public void Init()
        {
            m_NetworkManager ??= new NetworkManager();

            m_IsInit = true;
        }

        public void CreateChannel<T, TPH, TPD>(string channelName)
            where T : NetworkChannelBase, new() where TPH : IPacketHelper, new() where TPD : IPacketDispatcher, new()
        {
            CheckValid();
            
            var channel = m_NetworkManager.GetNetworkChannel(channelName);
            if (channel == null)
            {
                channel = m_NetworkManager.CreateNetworkChannel<T>(channelName, new TPH(), new TPD());

                if (channel is NetworkChannelBase networkChannel)
                {
                    networkChannel.NetworkChannelConnected = OnNetworkConnected;
                    networkChannel.NetworkChannelClosed = OnNetworkClosed;
                    networkChannel.NetworkChannelError = OnNetworkError;
                }
            }
        }

        public void DestroyChannel(string channelName)
        {
            CheckValid();

            m_NetworkManager.DestroyNetworkChannel(channelName);
        }
        
        public void Connect(string channelName, IPAddress ipAddress, int port) 
        {
            CheckValid();

            var channel = m_NetworkManager.GetNetworkChannel(channelName);
            if (channel == null)
            {
                LYLogger.Error($"please create channel {channelName} first");
                return;
            }
            
            channel.Connect(ipAddress, port);
        }

        public void Disconnect(string channelName)
        {
            CheckValid();
            var channel = m_NetworkManager.GetNetworkChannel(channelName);
            if (channel == null)
                return;
            
            channel.Close();
        }

        public bool IsConnected(string channelName)
        {
            CheckValid();
            var channel = m_NetworkManager.GetNetworkChannel(channelName);
            if (channel == null)
            {
                return false;
            }
            
            return channel.IsConnected;
        }

        public void Send<TPacket>(string channelName, TPacket packet) where TPacket : IPacket
        {
            CheckValid();
            var channel = m_NetworkManager.GetNetworkChannel(channelName);
            if (channel == null)
            {
                LYLogger.Error($"channel {channelName} not found");
                return;
            }

            channel.Send(packet);
        }

        public void AddHandler(string channelName, IPacketHandler handler)
        {
            CheckValid();
            
            var channel = m_NetworkManager.GetNetworkChannel(channelName);
            if (channel == null)
            {
                LYLogger.Error($"channel {channelName} not found");
                return;
            }
            channel.AddHandler(handler);
        }

        public void RemoveHandler(string channelName, IPacketHandler handler)
        {
            CheckValid();
            
            var channel = m_NetworkManager.GetNetworkChannel(channelName);
            if (channel == null)
            {
                LYLogger.Error($"channel {channelName} not found");
                return;
            }

            channel.RemoveHandler(handler);
        }

        public void Update()
        {
            if (!m_IsInit)
                return;
            
            m_NetworkManager.Update();
        }

        private void CheckValid()
        {
            if (!m_IsInit)
            {
                throw new Exception("Network utility is not initialized.");
            }
        }

        void OnNetworkConnected(NetworkChannelBase channel, object param)
        {
            OnChannelConnected?.Invoke(channel.Name, param);
        }
        
        void OnNetworkClosed(NetworkChannelBase channel)
        {
            OnChannelClosed?.Invoke(channel.Name);
        }
        
        void OnNetworkError(NetworkChannelBase channel, NetworkErrorCode errorCode, string errorMsg)
        {
            OnChannelError?.Invoke(channel.Name, errorCode, errorMsg);
        }
    }
}