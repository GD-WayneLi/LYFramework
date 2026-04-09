namespace LYFramework.Network
{
    public enum NetworkErrorCode
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// 序列化错误
        /// </summary>
        SerializeError,
        
        /// <summary>
        /// 发送失败
        /// </summary>
        SendError,
        
        /// <summary>
        /// 接收失败
        /// </summary>
        ReceiveError
    }
}