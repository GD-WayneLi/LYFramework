using System;

namespace LYFramework.Network
{
    public static class NetworkChannelFactory
    {
        public static INetworkChannel Create(NetworkChannelType networkChannelType, IPacketHelper packetHelper)
        {
            switch (networkChannelType)
            {
                
            }

            throw new Exception($"no this network channel type:{networkChannelType}");
        }
    }
}