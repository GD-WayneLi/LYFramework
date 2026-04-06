using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using CodiceApp.EventTracking;

namespace LYFramework.Network
{
    public abstract class NetworkChannelBase : INetworkChannel
    {
        public Action<NetworkChannelBase, NetworkErrorCode, string> NetworkChannelError;

        private const int DefaultBufferSize = 1024 * 4;
        
        private IPacketHelper m_PacketHelper;
        private Socket m_Socket;
        private Queue<IPacket> m_PacketQueue;

        private MemoryStream m_ReceiveStream;
        private MemoryStream m_SendStream;

        private SocketAsyncEventArgs m_ReceiveEventArgs;
        private SocketAsyncEventArgs m_SendEventArgs;

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
            m_PacketQueue = new Queue<IPacket>();

            m_ReceiveStream = new MemoryStream(DefaultBufferSize);
            m_SendStream = new MemoryStream(DefaultBufferSize);

            m_ReceiveEventArgs = new SocketAsyncEventArgs();
            m_SendEventArgs = new SocketAsyncEventArgs();

            m_ReceiveEventArgs.Completed += OnReceiveCompleted;
            m_SendEventArgs.Completed += OnSendCompleted;

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
            if (!IsConnected)
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
            if (!IsConnected)
            {
                return;
            }

            ProcessSend();
            ProcessReceive();
        }

        protected virtual bool ProcessSend()
        {
            if (m_SendStream.Length > 0)
                return false;

            while (m_PacketQueue.Count > 0)
            {
                var packet = m_PacketQueue.Dequeue();
                var result = m_PacketHelper.Serialize(packet, m_SendStream);
                if (!result)
                {
                    var errorStr = "Serialized packet failure.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SerializeError, errorStr);
                        return false;
                    }

                    throw new Exception(errorStr);
                }
            }

            m_SendStream.Position = 0;
            m_SendEventArgs.SetBuffer(m_SendStream.GetBuffer(), 0, (int)m_SendStream.Length);
            var ret = Socket.SendAsync(m_SendEventArgs);
            if (!ret)
            {
                OnSendCompleted(this, m_SendEventArgs);
            }

            return true;
        }

        public void ProcessReceive()
        {
        }

        public void Dispose()
        {
            Close();
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (!IsConnected)
            {
                return;
            }
            
            ResetStream(m_SendStream);

            if (e.SocketError != SocketError.Success)
            {
                if (NetworkChannelError != null)
                {
                    NetworkChannelError(this, NetworkErrorCode.SendError, e.SocketError.ToString());
                    return;
                }

                throw new Exception(e.SocketError.ToString());
            }
        }

        void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
        }

        void ResetStream(MemoryStream stream)
        {
            stream.Position = 0;
            stream.SetLength(0);
        }
    }
}