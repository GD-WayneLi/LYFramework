using System.IO;

namespace LYFramework.Network
{
    public interface IPacketHelper
    {
        /// <summary>
        /// 消息头长度
        /// </summary>
        int HeaderLength { get; }
        IPacketHeader DeserializeHeader(MemoryStream stream);
        
        bool Serialize<T>(T packet, MemoryStream stream) where T : IPacket;
        IPacket Deserialize(MemoryStream stream, IPacketHeader packetHeader); 
    }
}