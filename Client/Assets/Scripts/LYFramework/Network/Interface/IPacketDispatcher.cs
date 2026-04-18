using System;

namespace LYFramework.Network
{
    public interface IPacketDispatcher : IDisposable
    {
        void AddHandler(IPacketHandler handler);
        void RemoveHandler(IPacketHandler handler);
        
        /// <summary>
        /// 消息派发，需要保证线程安全
        /// </summary>
        /// <param name="packet"></param>
        void Dispatch(IPacket packet);
        
        void Update();
    }
}
