# Network 模块规范

本文件继承 `Assets/Scripts/LYFramework/AGENTS.md`，适用于网络通道、包协议接口、NetworkManager 和 NetworkUtility。

## 允许依赖

- 可以依赖 Core 的 Utility 契约、Log 和 ReferencePool。
- 不得依赖 UI、Demo、具体游戏 Packet、场景对象或 Unity 组件。
- Packet 序列化和分发的具体项目实现优先放在 `LYUnity`。

## 连接状态与线程

- 使用显式状态机表示 Disconnected、Connecting、Connected、Closing、Disposed；不要只依赖 `Socket.Connected`。
- Socket 完成回调视为后台线程。回调只更新线程安全状态或队列，业务 Handler 和 Unity API 在主线程 Update 中执行。
- 每次连接具有独立代次或取消标识，旧 Socket 的迟到回调不能污染重连后的新状态。
- Close/Dispose 幂等，且在释放资源前停止新发送、取消接收并屏蔽后续回调。

## 数据帧与安全

- HeaderLength 和 PacketLength 必须验证上下限；最大包长由配置或协议常量明确给出。
- 正确处理包头/包体分段、连续多个包、0 长包、部分发送、远端 0 字节关闭和 SocketError。
- 任何序列化/反序列化错误都必须把发送或接收状态恢复到可继续、可关闭的确定状态。
- 不要根据不可信远端长度无限扩容 MemoryStream。

## Packet 所有权

- Send 入队后 Packet 归谁所有必须写入 API 注释；成功、失败、断线和 Dispose 路径必须恰好释放一次。
- Dispatcher 必须在没有 Handler、Handler 抛异常和 Dispose 清队列时正确释放收到的 Packet。
- 用户 Handler 若需异步持有 Packet，必须复制数据或显式转移所有权，不能在自动 Release 后继续使用。

## 回调与锁

- 不使用 `lock(this)`；使用私有同步对象或无锁状态设计。
- 不在内部锁中调用用户委托、日志适配器或 PacketHandler。
- Connection/Error/Closed 回调的线程、顺序和重复触发规则必须固定。