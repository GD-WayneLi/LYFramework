# LYGame 实现层规范

本文件继承仓库根目录 `AGENTS.md`，适用于 `Assets/Scripts/LYGame/`。

## 职责

- 本目录实现 `LYFramework` 定义的接口，并承载 Unity、平台和当前项目特定适配。
- 可以引用 `LYFramework`，不得让 `LYFramework` 反向依赖本目录。
- 可复用的抽象或无项目依赖算法应下沉到框架；业务配置、资源路径、具体 Packet/UI/日志适配留在本层。

## 命名与程序集

- 新代码使用 `LYGame.*` 命名空间，不因实现了框架接口就放入 `LYFramework.*` 命名空间。
- 若新增 asmdef，只引用必要程序集，并保持 Runtime 与 Editor 代码分离。
- 一个适配器只负责一种后端，不在同一类型中混合 Resources、Addressables、编辑器模拟等多套实现。

## Unity 与线程

- 所有 Unity API 调用在主线程完成；网络、文件等后台结果通过队列或调度器切回主线程。
- MonoBehaviour 负责接入 Unity 生命周期，但业务状态和可测试逻辑尽量保留在普通 C# 类型中。
- OnDestroy/OnApplicationQuit 中的清理应幂等，不依赖不确定的脚本销毁顺序。

## 当前实现注意点

- `DefaultLogHelper` 应被视为 Unity 日志适配器。
- `PacketDispatcher` 负责把后台收到的 Packet 转到主线程，并必须在所有路径正确 Release。
- `TcpNetworkChannel` 当前是空子类，不能被当成完成的 TCP 实现。
- `Utility/UI/TestUI.cs` 是注释骨架，不作为有效示例。

## 最低验证

- 适配器契约测试与 Unity 主线程行为测试。
- 初始化/销毁、Domain Reload、应用退出和异常路径资源清理。
- 网络适配使用 loopback，资源/UI 适配使用可控的测试替身或测试资产。
