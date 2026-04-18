namespace LYFramework.Network
{
    public interface IPacketHandler
    {
        int Id { get; }
        
        void HandlePacket(IPacket packet);
    }
}