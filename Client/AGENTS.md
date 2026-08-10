# AGENTS.md

## 最小入口规则

- 请使用全中文跟我沟通（代码除外）。
- 每次执行任何操作前，请先说明要做什么，以及为什么要这么做。
- 只修改沟本次通相关代码，不要自行修改其他代码，如果需要修改其他代码，请进行询问。
- 代码的方法定义不要换行

## 文档地位与适用范围

- 本文件是 `Client/` 目录及其所有子目录的唯一仓库协作规范源。
- `CLAUDE.md` 仅用于兼容 Claude Code，并通过 `@AGENTS.md` 导入本文件；不要把 `CLAUDE.md` 当作独立信息源，也不要在其中重复维护规则。
- 若更深层目录以后出现新的 `AGENTS.md`，以更深层文件对其作用域内的补充或覆盖为准。
- 开始工作前先检查 `git status`。工作区可能包含用户正在进行的重构；不要回退、覆盖或顺手整理与当前任务无关的改动。

## 项目概况

- Unity：`2022.3.62f1`。
- 语言：C# 9，Unity 生成工程当前目标为 .NET Framework 4.7.1 / Unity .NET 4.x 兼容级别。
- `Assets/Scripts/LYFramework/LYFramework.asmdef` 定义可复用框架程序集 `LYFramework`。
- `Assets/Scripts/LYGame/` 和 `Assets/Scripts/Demo/` 当前进入 Unity 默认程序集 `Assembly-CSharp`，用于游戏侧实现与示例。
- 框架目标是尽量以普通 C# 对象承载逻辑
- 当前仓库处于框架演进阶段。Core/Entity、Event、ReferencePool、Network、UI 等模块有基础骨架，但部分主流程尚未闭环，不能默认视为生产可用。

## 目录与职责

```text
Assets/Scripts/
├── LYFramework/              可复用框架程序集
│   ├── Core/                 GameManager、System、Controller、Entity 生命周期
│   ├── Event/                同步与延迟事件
│   ├── ReferencePool/        按运行时类型划分的对象池
│   ├── Network/              网络通道抽象、管理器与 Utility
│   ├── UI/                   UI 生命周期和栈管理骨架
│   ├── Log/                  日志门面与接口
│   └── Resource/             资源服务接口
├── LYGame/                   游戏侧适配器和具体实现
└── Demo/                     启动与实验代码，不是正式 API 示例
```

依赖方向必须保持为：

```text
Demo / LYGame  ──────>  LYFramework
```

- `LYFramework` 不得反向引用 `LYGame` 或 `Demo`。
- 通用接口、生命周期和跨项目可复用实现放入 `LYFramework`；Unity/项目特定适配放入 `LYGame`。
- `DefaultLogHelper`、`PacketDispatcher`、`TcpNetworkChannel` 属于游戏侧适配器，即使某些现有命名空间仍使用 `LYFramework.*`，新增代码也应按实际职责选择命名空间。
- 不要在新代码中继续使用遗留的 `QFramework.Demo` 命名空间；示例代码统一使用 `LYGame.Demo` 或项目约定的新命名空间。

## 当前架构约定

### Core 与生命周期

- `IGameManager` 是 System 和 Utility 的注册/查询入口。
- `RegisterSystem` 当前会立即调用 `ISystem.Init(IGameManager)`；System 销毁由 `GameManagerBase.Dispose()` 触发。
- System 可通过 `ISystemAwake<T>`、`ISystemUpdate<T>` 和 `ISystemDispose<T>` 接收 Component 生命周期。`EntityLifecycle` 负责处理器注册、分发和 Update 队列，GameManager 只持有并驱动它。调用方每帧显式调用 `IGameManager.Update()`；销毁 GameManager 时先销毁 World 中的 Component，再按注册逆序销毁 System。
- Awake 抛异常时，框架必须原子回滚刚创建的 Component 并直接保留原异常；回滚不得调用公开 `Dispose()`，也不得触发 `ISystemDispose<T>`。
- `IUtility` 当前只是标记接口，注册时不会自动初始化，`GameManagerBase.Dispose()` 也不会销毁 Utility。若统一生命周期，必须同时修改接口、注册流程、异常回滚、销毁顺序和测试，不要只改其中一处。
- `IController`/`ISystem` 通过 `IGetGameManager` 扩展方法获取依赖。避免在业务对象中绕过该边界新增更多全局单例。
- 初始化和销毁应幂等。注册失败、初始化抛异常或部分初始化时，必须定义清晰的回滚与所有权规则。

### Entity、EntityDomain 与 World

- `GameManagerBase<T>` 独占一个 `World`，但自身不是 `Entity`、`EntityDomain` 或 Entity 树节点。
- `World` 是 Entity 的运行时归属和快速查询边界。它应保存当前 World 内全部存活 Entity，包括根 EntityDomain、树内 Child、Component 和尚未挂入树的游离 Entity，并以 `InstanceId` 建立索引。
- `World` 的索引只用于生命周期管理和快速查找，不表达 Parent/Child 层级，也不替代 EntityDomain 的逻辑归属。
- 当前每个 `World` 固定拥有一个根 `EntityDomain`。`EntityDomain` 继承自 `Entity`，作为逻辑隔离边界、Entity 树根节点和 `EntityLifecycle` 转发入口，不维护 Entity 索引或 System 处理器。
- `Entity` 负责保持 Parent、Child 和 Component 层级。一个 Entity 最多只有一个 Parent；Child 与 Component 两种关系互斥；同一 Parent 当前只允许一个相同运行时类型的 Component。
- Parent 拥有 Child 和 Component。默认移除应连带 `Dispose()`；若以后支持游离操作，必须由显式 API 转移所有权，且 Entity 仍由原 World 管理，直到销毁或明确迁移到另一个 World。
- `Id` 表示稳定标识，`InstanceId` 表示当前生命周期实例。World 的快速查询键使用 `InstanceId`；存活实例的 `InstanceId` 必须非 0、在 World 内唯一，销毁后置为 0 并从索引移除。
- 创建、挂接、移除、迁移和销毁 Entity 时，必须同步维护 Parent/Child 或 Component 关系、Domain 归属以及 World 索引。任何一步失败都应回滚，不能留下半挂接对象或脏索引。
- 同一棵 Entity 树中的 Parent、Child、Component 和 EntityDomain 必须属于同一个 World。禁止跨 World 直接挂接；跨 World 迁移若以后支持，必须使用显式且原子的迁移流程。
- 改动 Entity 树时必须维护：无环、单一父节点、关系类型互斥、Domain 归属一致、World 索引一致、重复销毁安全、遍历期间修改安全。
- 当前 `World.Add/Remove/Get` 只是索引骨架，Entity 创建、`InstanceId` 分配、自动注册、销毁注销以及游离 Entity 清理尚未闭环；新增调用方不得假定这些流程已经自动完成。
- 框架逻辑域统一使用 `EntityDomain` 命名；不要重新引入名为 `LYFramework.Scene` 的框架类型。

### Event

- `Send<T>` 是当前帧同步派发；`Post<T>` 应在 `Update()` 中延迟派发。
- 延迟队列必须保留事件的实际类型。当前 `EventManager.Update()` 以 `IEvent` 静态类型再次调用泛型 `Send`，无法命中具体事件监听器，这是已知缺陷。
- 明确事件线程模型。若允许后台线程 Post，队列和 Dispose 必须线程安全；监听器默认只在 Unity 主线程执行。
- Listener 抛异常、派发中增删 Listener、Dispose 后 Post/Update 的行为都应有测试和一致策略。

### ReferencePool

- Acquire/Release 必须成对，Release 前调用 `Clear()`，对象离池后不能继续持有或异步使用。
- 当前实现不防重复 Release，`UsingCount` 可能为负，同一实例也可能被重复入栈；新增调用方不能依赖池替自己发现所有权错误。
- 静态池和 `Stack<T>` 当前不是线程安全的。若网络或任务线程访问，先统一线程模型或增加同步，不要局部假设安全。
- `SetCapacity` 当前只影响首次创建时的 Stack 初始容量，不会预热对象，也不会修改已有 Collector；命名或行为变更需要同步文档和测试。

### Network

- Socket 完成回调可能运行在非 Unity 主线程。它们只能写入线程安全队列或连接状态，Unity API 和业务 Handler 必须回到主线程执行。
- Packet 的发送方、通道、Dispatcher 和 ReferencePool 之间必须明确所有权：谁在成功、失败、关闭、反序列化异常时 Release，必须只有一个答案。
- 当前 `NetworkChannelBase.Connect()` 只重置状态，没有创建/连接 Socket；`TcpNetworkChannel` 也没有补充实现，首次接收同样没有启动。当前网络通道不是完整可连接实现。
- 修改网络代码时必须覆盖：分包/粘包、部分发送、0 字节远端关闭、重连、旧异步回调、序列化失败回滚、最大包长、异常后的状态恢复、Dispose 期间并发回调。
- 反序列化前必须限制 `PacketLength`，不能把远端长度直接用于无限制扩容 `MemoryStream`。
- 不要在持有内部锁时调用用户回调；不要使用 `lock(this)` 作为同步对象。

### UI

- UI 加载是潜在异步流程，必须显式表示 Created/Loading/Open/Closing/Closed 等状态，并处理“加载完成前关闭”和重复关闭。
- 当前 `UIBase.Load()` 不调用完成回调，`m_IsPrepared` 也从未置为 `true`，因此默认流程不会进入 `OnOpen()` 或 `OnUpdate()`；`SetUIVisible()` 还是空实现。当前 UI 模块属于未完成骨架。
- 若 UI 实现 `IController`，创建时必须注入 `IGameManager`，不能让 `GetSystem/GetUtility` 在未初始化状态下工作。
- 资源加载、实例化、层级/深度、可见性、关闭和资源卸载必须形成成对生命周期。

## 已知工程状态

- 当前没有项目自有的 EditMode/PlayMode 测试程序集，也没有覆盖框架代码的自动化测试。
- `Assets/Scenes/Launcher.unity` 当前只有基础 Camera/Light，没有挂接 `Launcher` 脚本。
- `ProjectSettings/EditorBuildSettings.asset` 当前没有构建场景。
- `Demo/Launcher.cs` 是类型判断实验代码，框架初始化和销毁调用已被注释；不要把它当作有效启动模板。
- Unity Editor 最近一次脚本编译成功，但 Unity 生成的 `LYFramework.csproj` 仍含已删除的 Model/ECS 文件条目。`.csproj`/`.sln` 是生成物，可能滞后于 AssetDatabase。

## 编辑规则

- 不要手工编辑 `Library/`、`Temp/`、`Logs/`、`obj/`、Unity 生成的 `*.csproj` 或解决方案文件来修复源码问题。
- `Assets/` 下新增、移动、删除资源时保持对应 `.meta` 同步；移动资源优先保留原 GUID，不复制或复用其他资源的 GUID。
- 所有文本文件使用 UTF-8。现有中文注释要按 UTF-8 读取，避免把编码显示问题写回源码。
- 保持现有 C# 风格：Allman 大括号、类型/成员 PascalCase、局部变量 camelCase、实例字段 `m_`、静态字段 `s_`；新代码避免 `a`、`b`、`testBase` 这类临时命名。
- 公共 API 使用 XML 注释说明线程、所有权、生命周期和异常，而不只是重复方法名。
- 参数错误优先使用 `ArgumentNullException`、`ArgumentOutOfRangeException`、`InvalidOperationException` 等准确异常；不要泛化为 `Exception`。
- 可释放资源实现 `IDisposable`，Dispose 应幂等，并释放事件订阅、Socket、Stream、队列中仍拥有的对象和静态引用。
- 迭代集合时若回调可能修改集合，先快照或采用延迟修改队列。
- 不进行任务范围外的大规模格式化、重命名或目录整理。

## 验证方式

- Unity Editor/Unity Test Runner 是权威编译与测试环境。只有在 Unity 已刷新工程文件后，`dotnet build` 才能作为快速检查；不要把生成工程中的陈旧文件条目误判为源码错误。
- 若 Unity 已打开，可检查 `%LOCALAPPDATA%/Unity/Editor/Editor.log` 中最近一次 `ScriptCompilation` 的退出码和 `error CS...`，但不要仅凭旧日志宣称验证通过。
- 新增测试时使用独立的 EditMode/PlayMode asmdef，并让测试程序集引用 `LYFramework`，不要把测试代码混入运行时程序集。
- 最低验证矩阵：

| 修改区域 | 最低验证 |
| --- | --- |
| Core/Entity | World 注册/查询/注销、InstanceId 唯一性、Add/Remove/Reparent、环检测、Component 唯一性、Domain 归属、递归/重复 Dispose |
| Event | Send/Post 类型一致、派发中增删监听、监听异常、Dispose、跨线程 Post（若支持） |
| ReferencePool | 首次 Acquire、重复复用、Clear、null/重复/错误 Release、Clear 全池 |
| Network | 本机 loopback、分段头/包体、部分发送、断线、重连、超长包、序列化/反序列化异常 |
| UI | 同步/异步加载、打开/关闭顺序、加载中关闭、层级深度、可见性、资源卸载 |

## 提交前检查

- 复查 `git diff --check` 和 `git status --short`，确认没有覆盖用户已有改动。
- 确认没有编辑 Unity 生成物，也没有遗漏 Assets 对应的 `.meta`。
- 说明实际执行过的验证；未执行或受环境限制的验证必须明确写出。
- 若改变公共 API、生命周期、线程模型、目录职责或构建方式，同步更新本文件；`CLAUDE.md` 始终只保留导入，不复制内容。
