using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace LYFramework.Network
{
    public abstract class NetworkChannelBase : INetworkChannel
    {
        public Action<NetworkChannelBase, NetworkErrorCode, string> NetworkChannelError;

        private const int DefaultBufferSize = 1024 * 4;

        private IPacketHelper m_PacketHelper;
        private IPacketHeader m_PacketHeader;

        private Socket m_Socket;
        private readonly Queue<IPacket> m_SendPacketQueue = new();
        private readonly Queue<IPacket> m_ReceivePacketQueue = new();

        private MemoryStream m_ReceiveStream;
        private MemoryStream m_SendStream;

        private SocketAsyncEventArgs m_ReceiveEventArgs;
        private SocketAsyncEventArgs m_SendEventArgs;

        private bool m_IsReceiving;

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

            m_IsReceiving = false;

            ResetReadStream();
            ResetReceiveState();
        }

        public void Send<T>(T packet) where T : IPacket
        {
            if (!IsConnected)
            {
                return;
            }

            lock (m_SendPacketQueue)
            {
                m_SendPacketQueue.Enqueue(packet);
            }
        }

        public void Close()
        {
            lock (this)
            {
                if (m_Socket == null)
                {
                    return;
                }

                var socket = m_Socket;
                m_Socket = null;

                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    socket.Close();
                }

                lock (m_SendPacketQueue)
                {
                    m_SendPacketQueue.Clear();
                }

                lock (m_ReceivePacketQueue)
                {
                    m_ReceivePacketQueue.Clear();
                }
            }
        }

        public void Update()
        {
            if (!IsConnected)
            {
                return;
            }

            ProcessSend();
            ProcessReceive();
            ProcessReceivePacket();
        }

        protected virtual bool ProcessSend()
        {
            if (m_SendStream.Length > 0 || m_SendPacketQueue.Count <= 0)
                return false;

            lock (m_SendPacketQueue)
            {
                while (m_SendPacketQueue.Count > 0)
                {
                    var packet = m_SendPacketQueue.Dequeue();
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

        protected virtual void ProcessReceive()
        {
            if (m_IsReceiving)
                return;

            m_IsReceiving = true;
            m_ReceiveEventArgs.SetBuffer(m_ReceiveStream.GetBuffer(), (int)m_ReceiveStream.Position,
                (int)(m_ReceiveStream.Length - m_ReceiveStream.Position));

            var ret = Socket.ReceiveAsync(m_ReceiveEventArgs);
            if (!ret)
            {
                OnReceiveCompleted(this, m_ReceiveEventArgs);
            }
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

            ResetReadStream();

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
            m_IsReceiving = false;
            if (!IsConnected)
            {
                return;
            }

            if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success)
            {
                if (NetworkChannelError != null)
                {
                    NetworkChannelError(this, NetworkErrorCode.ReceiveError, e.SocketError.ToString());
                    return;
                }

                throw new Exception(e.SocketError.ToString());
            }

            m_ReceiveStream.Position += e.BytesTransferred;
            if (m_ReceiveStream.Position < m_ReceiveStream.Length)
            {
                return;
            }

            m_ReceiveStream.Position = 0;
            if (m_PacketHeader == null)
            {
                m_PacketHeader = m_PacketHelper.DeserializeHeader(m_ReceiveStream);

                if (m_PacketHeader == null || m_PacketHeader.PacketLength < 0)
                    throw new Exception("DeserializeHeader failure.");
                
                m_ReceiveStream.Position = 0;
                m_ReceiveStream.SetLength(m_PacketHeader.PacketLength);
                
                if (m_PacketHeader.PacketLength <= 0)
                {
                    DeserializePacket();
                }
            }
            else
            {
                DeserializePacket();
            }
        }

        bool DeserializePacket()
        {
            var packet = m_PacketHelper.Deserialize(m_ReceiveStream, m_PacketHeader);
            if (packet != null)
            {
                lock (m_ReceivePacketQueue)
                {
                    m_ReceivePacketQueue.Enqueue(packet);
                }
            }
            ResetReceiveState();

            return true;
        }

        void ProcessReceivePacket()
        {
            lock (m_ReceivePacketQueue)
            {
                while (m_ReceivePacketQueue.Count > 0)
                {
                    var packet = m_ReceivePacketQueue.Dequeue();
                }
            }
        }
        
        void ResetReadStream()
        {
            m_SendStream.Position = 0;
            m_SendStream.SetLength(0);
        }

        void ResetReceiveState()
        {
            m_PacketHeader = null;
            m_ReceiveStream.Position = 0;
            m_ReceiveStream.SetLength(m_PacketHelper.HeaderLength);
        }
    }
}