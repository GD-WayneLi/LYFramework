using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace LYFramework.Network
{
    public abstract class NetworkChannelBase : INetworkChannel
    {
        private IPacketHelper m_PacketHelper;
        private Socket m_Socket;
        private Queue<IPacket> m_PacketQueue;
        
        public Socket Socket => m_Socket;

        public bool IsConnected
        {
            get
            {
                if (m_Socket == null)
                    return false;

                return m_Socket.Connected;
            }
        }
        
        public bool Init(IPacketHelper packetHelper)
        {
            m_PacketHelper = packetHelper;
            return true;
        }

        public virtual void Connect(IPAddress ipAddress, int port)
        {
            if (m_Socket != null)
            {
                Close();
                m_Socket = null;
            }
            
            
        }

        public void Send<T>(T packet) where T : IPacket
        {
            if (m_Socket == null)
            {
                return;
            }
            m_PacketQueue.Enqueue(packet);
        }

        public void Close()
        {
            if (m_Socket == null)
            {
                return;
            }
            
            m_Socket.Shutdown(SocketShutdown.Both);
            m_Socket.Close();
            m_Socket = null;
        }

        public void Update()
        {
            if (m_Socket == null)
            {
                return;
            }

            ProcessSend();
            ProcessReceive();
        }

        public void ProcessSend()
        {
            
        }

        public void ProcessReceive()
        {
            
        }
        
        public void Dispose()
        {
            Close();
        }
    }
}