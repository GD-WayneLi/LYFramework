using System;
using System.Net;
using LYFramework.Log;
using LYFramework.Network;

namespace LYGame.Utility.Network
{
    public class GameNetworkUtility<T> : IGameNetworkUtility where T : INetworkChannel, new()
    {
        private bool m_IsInit = false;
        private NetworkManager m_NetworkManager;
        private IPacketHelper m_PacketHelper;
        private IPacketDispatcher m_PacketDispatcher;
        
        private INetworkChannel m_NetworkChannel;
        
        public void Init(IPacketHelper packetHelper, IPacketDispatcher packetDispatcher)
        {
            if (packetHelper == null)
            {
                throw new ArgumentNullException(nameof(packetHelper), "Packet helper cannot be null");
            }
            
            if (packetDispatcher == null)
            {
                throw new ArgumentNullException(nameof(packetDispatcher), "Packet dispatcher cannot be null");
            }
            
            m_NetworkManager ??= new NetworkManager();
            m_PacketHelper = packetHelper;
            m_PacketDispatcher = packetDispatcher;

            m_IsInit = true;
        }

        public void Connect(NetworkChannelType channelType, IPAddress ipAddress, int port)
        {
            CheckNetworkValid();

            m_NetworkChannel ??= m_NetworkManager.CreateNetworkChannel<T>(channelType, m_PacketHelper);

            m_NetworkChannel.Connect(ipAddress, port);
        }

        public void Disconnect(NetworkChannelType channelType)
        {
            CheckNetworkValid();
            
            m_NetworkChannel?.Close();
        }

        public void Send<TPacket>(TPacket packet) where TPacket : IPacket
        {
            CheckNetworkValid();

            if (m_NetworkChannel == null)
            {
                LYLogger.Error($"m_NetworkChannel does not exist, you need to call Connect first.");
                return;
            }

            m_NetworkChannel.Send(packet);
        }

        public void AddHandler(NetworkChannelType channelType, int packetId, Action<IPacket> handler)
        {
            m_PacketDispatcher.AddHandler(packetId, handler);
        }

        public void RemoveHandler(NetworkChannelType channelType, int packetId, Action<IPacket> handler)
        {
            m_PacketDispatcher.RemoveHandler(packetId, handler);
        }

        public void Update()
        {
            if (m_NetworkChannel == null || !m_NetworkChannel.IsConnected)
                return;
            
            m_NetworkManager.Update();
            
            m_PacketDispatcher.Dispatch(m_NetworkChannel);
        }

        private void CheckNetworkValid()
        {
            if (!m_IsInit)
            {
                throw new Exception("Network utility is not initialized.");
            }
        }
    }
}