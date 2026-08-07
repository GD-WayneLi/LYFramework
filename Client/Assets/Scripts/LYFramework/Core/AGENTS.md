# Core 模块规范

本文件继承 `Assets/Scripts/LYFramework/AGENTS.md`，适用于 `Core/` 及其 Controller、Entity、Interface、System、Utility 子目录。

## 模块职责

- Core 只负责对象生命周期、服务注册/查询、System/Controller 基础契约，以及 World、EntityDomain、Entity 的所有权和层级关系。
- 不在 Core 中加入 UI、网络协议、资源路径、业务事件或具体游戏状态。
- 新代码避免直接依赖 `UnityEngine`；日志或错误报告通过框架抽象完成。

## GameManager 与服务注册

- `GameManagerBase<T>` 持有并独占一个 `World`，自身不参与 Entity 树。销毁 GameManager 时必须先按确定顺序销毁服务，再销毁 World。
- 明确服务注册键是“契约类型”还是“实现类型”，注册与查询必须使用同一规则。
- 重复注册、初始化异常和部分初始化必须有确定结果与回滚，不得只记录日志后留下半初始化对象。
- 初始化顺序必须确定，销毁默认按初始化逆序执行。
- System、Utility、Controller 的生命周期接口需要保持一致语义；修改其中之一时检查所有实现类。
- System 可通过 `ISystemAwake<T>` 声明其关注的 Component。GameManager 在 System 初始化成功后缓存处理器；`AddComponent<T>()` 完成 Parent、Domain 和集合挂载后同步触发，回调异常必须回滚本次组件添加。
- 单例销毁后必须可以安全重建，且旧对象或异步回调不能访问新实例。

## World：所有权与快速索引

- `World` 是全部 Entity 的运行时容器、生命周期所有者和快速查询入口，不负责表达 Entity 层级。
- 当前模型中每个 World 固定拥有一个根 `EntityDomain`。根 EntityDomain 本身也是 Entity，也应纳入同一个 World 的管理和索引。
- World 应保存其范围内的全部存活 Entity：根 EntityDomain、普通 Child、Component，以及尚未挂入 EntityDomain 树的游离 Entity。
- 当前索引键为 `InstanceId`，查询语义是“查找某个仍存活的生命周期实例”。不得混用稳定 `Id` 和 `InstanceId` 作为同一字典的键。
- 存活 Entity 必须在所属 World 中恰好注册一次；销毁时必须恰好注销一次。重复注册、重复注销和 InstanceId 冲突必须有明确结果。
- `World.Add/Remove/Get` 只负责索引操作时，也必须校验 null、World 是否已销毁、Entity 是否已销毁、InstanceId 是否有效以及对象归属，不能允许调用方静默覆盖其他实例。
- World Dispose 必须销毁根 EntityDomain 以及索引中仍由 World 拥有的游离 Entity，并最终清空索引。递归销毁树时不得对同一 Entity 重复释放。
- 不要把 Parent/Child 查询建立在 World 索引遍历上；层级查询由 Entity 自身维护，World 只提供按 InstanceId 的直接定位。

## EntityDomain：逻辑隔离

- `EntityDomain` 继承自 `Entity`，用于表示逻辑隔离边界和 Entity 树根节点，不负责保存 World 的全量 Entity 索引。
- `EntityDomain` 保存所属逻辑域的 Component 生命周期回调入口；它只负责转发生命周期通知，System 处理器的注册、缓存和执行仍由 GameManager 管理。
- 当前一个 World 只有一个根 EntityDomain。若以后扩展多个逻辑域，World 仍是统一索引和生命周期边界，不能重新把全局索引分散回各个 EntityDomain。
- 普通 Entity 的 `Domain` 表示其当前所在的逻辑树；游离 Entity 的 Domain 可以为 null，但仍必须保留 World 归属和 World 索引。
- 挂接、移除或迁移子树时，Domain 归属必须递归更新到全部 Child 和 Component。
- `EntityDomain` 不得作为普通 Child 或 Component 随意挂接；新增多个或嵌套逻辑域前必须先定义清晰的所有权和迁移规则。
- 框架逻辑域统一使用 `EntityDomain` 命名，不要重新添加容易与 Unity Scene 混淆的 `LYFramework.Scene` 类型。

## Entity：层级结构

- `Entity` 只负责 Parent、Child、Component、Domain 引用和自身生命周期状态，不承担全局查找职责。
- 一个 Entity 最多有一个 Parent；Child 和 Component 两种关系互斥；同一 Parent 默认只能拥有一个相同运行时类型的 Component。
- Parent 拥有 Child 和 Component。默认移除会销毁目标；如果提供不销毁的游离操作，必须显式表达所有权仍归 World，并保持 World 索引不变。
- 禁止自引用和父子环。挂接或迁移前先完成全部校验，再修改旧 Parent、新 Parent、Domain 和 World 索引；中途失败必须恢复原状态。
- Parent、Child、Component 和所属 EntityDomain 必须处于同一个 World。禁止通过直接修改集合制造跨 World 层级。
- `Children` 和 `Components` 不得向调用方暴露可变字典；公开遍历应返回只读视图、快照或受控枚举。
- `IsDisposed`、`IsComponent`、Parent、Domain、Id 和 InstanceId 等维持不变量的状态只能由框架内部修改。
- 遍历 Child 或 Component 时，若递归 Dispose 或回调会修改原集合，必须先快照或使用安全的逐项移除流程。
- Dispose 必须幂等。销毁完成后需要清除 Parent、Domain、Component 标记和 InstanceId，并从 World 索引注销；部分销毁抛异常时不能让对象永久停留在半销毁状态。

## 标识与创建流程

- `Id` 表示 Entity 的稳定标识；`InstanceId` 表示当前生命周期实例，用于 World 快速查询和异步有效性检查。
- 存活实例的 InstanceId 必须非 0 且在 World 内唯一；0 只表示尚未完成生命周期初始化或已经销毁。
- Entity 创建必须通过受控流程完成：创建实例、分配标识、绑定 World、注册索引，然后根据用途挂接为 Child/Component 或保持游离。
- 若新增统一工厂或 `World.CreateEntity<T>()`，它应成为业务 Entity 的首选入口；不要让 Entity、EntityDomain 和业务代码分别维护独立的编号生成器或注册逻辑。
- 创建或挂接失败时，必须回滚已经生成的父子关系、组件标记、Domain 引用和 World 索引，不能遗留只有 Parent 引用但未进入集合的对象。

## 当前实现状态

- `GameManagerBase<T>` 已持有唯一 World，World 已持有唯一根 EntityDomain。
- `World` 已有基于 `InstanceId` 的 `Add/Remove/Get` 索引骨架，但根 EntityDomain 和新建 Entity 尚未自动注册，Entity 也尚未记录所属 World。
- 当前 Entity 创建流程尚未正确分配 `Id/InstanceId`，Child/Component 挂接与 World 注册尚未联动。
- 当前 World Dispose 只调用根 EntityDomain Dispose，尚未独立处理索引中仍存活的游离 Entity，也未清空索引。
- Entity 树的环检测、关系修改原子性、遍历期间修改安全和销毁后的完整状态清理仍需闭环；不得把当前骨架视为生产可用实现。
- 当前 Utility 没有统一 Init/Dispose 契约。修复应作为完整生命周期设计处理，不做局部补丁。

## 最低验证

- 服务注册、重复注册、缺失查询、初始化失败回滚、销毁顺序和重复 Dispose。
- World 根 EntityDomain 注册、Entity Add/Get/Remove、InstanceId 唯一性、重复注册、无效注销和 World Dispose 后查询。
- Entity AddChild/RemoveChild、AddComponent/RemoveComponent、关系互斥、环检测和操作失败回滚。
- Domain 归属递归传播、游离 Entity 保持 World 注册、跨 World 挂接拒绝，以及迁移过程的索引一致性。
- 父节点销毁、子节点主动销毁、Component 销毁、游离对象销毁、递归/重复 Dispose 和遍历期间修改安全。
