using System;
using System.Net;
using LYFramework;
using LYFramework.Network;

namespace LYGame.Utility.Network
{
    public interface IGameNetworkUtility : IUtility
    {
        void Init(IPacketHelper packetHelper, IPacketDispatcher packetDispatcher);
        void Connect(NetworkChannelType channelType, IPAddress ipAddress, int port);
        void Disconnect(NetworkChannelType channelType);
        void Send<T>(T packet) where T : IPacket;
        void AddHandler(NetworkChannelType channelType, int packetId, Action<IPacket> handler);
        void RemoveHandler(NetworkChannelType channelType, int packetId, Action<IPacket> handler);
        void Update();
    }
}
