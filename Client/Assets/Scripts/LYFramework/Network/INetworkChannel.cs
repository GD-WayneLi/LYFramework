using System;
using System.Net;

namespace LYFramework.Network
{
    public interface INetworkChannel : IDisposable
    {
        bool Init(IPacketHelper packetHelper);
        void Connect(IPAddress ipAddress, int port);
        void Send<T>(T packet) where T : IPacket;
        bool TryDequeuePacket(out IPacket packet);
        void Close();
        void Update();
    }
}
