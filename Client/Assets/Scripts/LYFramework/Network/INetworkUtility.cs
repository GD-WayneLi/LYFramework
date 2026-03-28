namespace LYFramework.Network
{
    public interface INetworkUtility : IUtility
    {
        public INetworkChannel CreateNetworkChannel<T>(NetworkChannelType channelType, IPacketHelper packetHelper) where T : INetworkChannel, new();
        void DestroyNetworkChannel(NetworkChannelType channelType);

        bool HasNetworkChannel(NetworkChannelType channelType);
        INetworkChannel GetNetworkChannel(NetworkChannelType channelType);
        
        void Update();
    }
}