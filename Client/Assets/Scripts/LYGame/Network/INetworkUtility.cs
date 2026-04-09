using System;
using System.Net;
using LYFramework;
using LYFramework.Network;

namespace LYGame.Network
{
    public interface INetworkUtility : IUtility
    {
        void RegisterChannel(NetworkChannelType channelType, IPacketHelper packetHelper, IPacketDispatcher dispatcher = null);
        void UnregisterChannel(NetworkChannelType channelType);
        bool HasChannel(NetworkChannelType channelType);
        void Connect(NetworkChannelType channelType, IPAddress ipAddress, int port);
        void Disconnect(NetworkChannelType channelType);
        void Send<T>(NetworkChannelType channelType, T packet) where T : IPacket;
        void AddHandler(NetworkChannelType channelType, int packetId, Action<IPacket> handler);
        void RemoveHandler(NetworkChannelType channelType, int packetId, Action<IPacket> handler);
        void Update();
    }
}
