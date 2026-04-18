using LYFramework.ReferencePool;

namespace LYFramework.Network
{
    public interface IPacket : IReference
    {
        int Id { get; }
    }
}