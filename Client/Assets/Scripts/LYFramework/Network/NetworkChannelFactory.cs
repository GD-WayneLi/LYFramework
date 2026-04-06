using System;

namespace LYFramework.Network
{
    public static class NetworkChannelFactory
    {
        public static INetworkChannel Create(NetworkChannelType networkChannelType, IPacketHelper packetHelper)
        {
            switch (networkChannelType)
            {
                case NetworkChannelType.TCP:
                    break;
            }

            throw new Exception($"no this network channel type:{networkChannelType}");
        }
    }
}