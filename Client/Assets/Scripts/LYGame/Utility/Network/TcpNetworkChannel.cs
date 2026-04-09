using System.Net;
using LYFramework.Network;

namespace LYGame.Utility.Network
{
    public class TcpNetworkChannel : INetworkChannel
    {
        public bool IsConnected { get; }
        
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
        
        public bool Init(IPacketHelper packetHelper)
        {
            throw new System.NotImplementedException();
        }

        public void Connect(IPAddress ipAddress, int port)
        {
            throw new System.NotImplementedException();
        }

        public void Send<T>(T packet) where T : IPacket
        {
            throw new System.NotImplementedException();
        }

        public bool TryDequeuePacket(out IPacket packet)
        {
            throw new System.NotImplementedException();
        }

        public void Close()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }
    }
}