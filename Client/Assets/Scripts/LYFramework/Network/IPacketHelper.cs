using System.IO;

namespace LYFramework.Network
{
    public interface IPacketHelper
    {
        /// <summary>
        /// 消息头长度
        /// </summary>
        int HeaderLength { get; }
        int DeserializeHeader(byte[] data);
        
        bool Serialize<T>(T packet, MemoryStream stream) where T : IPacket;
        IPacket Deserialize(byte[] data); 
    }
}