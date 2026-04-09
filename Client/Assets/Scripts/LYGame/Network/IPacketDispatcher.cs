using System;
using LYFramework.Network;

namespace LYGame.Network
{
    public interface IPacketDispatcher
    {
        void AddHandler(int packetId, Action<IPacket> handler);
        void RemoveHandler(int packetId, Action<IPacket> handler);
        void Dispatch(INetworkChannel channel);
    }
}
