using System;
using System.Net;

namespace LYFramework.Network
{
    public interface INetworkChannel : IDisposable
    {
        /// <summary>
        /// 频道名
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 获取是否已连接。
        /// </summary>
        bool IsConnected
        {
            get;
        }

        bool Init(string name, IPacketHelper packetHelper, IPacketDispatcher dispatcher);
        
        void AddHandler(IPacketHandler handler);
        void RemoveHandler(IPacketHandler handler);
        
        void Connect(IPAddress ipAddress, int port);
        void Send<T>(T packet) where T : IPacket;
        
        void Close();
        void Update();
    }
}
