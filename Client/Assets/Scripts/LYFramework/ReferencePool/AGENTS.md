# ReferencePool 模块规范

本文件继承 `Assets/Scripts/LYFramework/AGENTS.md`，适用于全局对象池和类型 Collector。

## 所有权

- Acquire 表示调用方取得对象的唯一租约；Release 后调用方不得继续读取、写入或缓存该对象。
- 每次成功 Acquire 必须且只能有一次 Release；Release 必须在重新入池前调用 `Clear()`。
- 不接受 null、错误类型、未由该池创建的对象或重复 Release，行为应在开发环境尽早暴露。
- Collector Clear/Dispose 时对仍在使用的对象采取什么策略必须明确，不能静默重置计数后让后续 Release 失败。

## 容量与性能

- 区分“Stack 初始容量”“预热数量”“最大缓存数量”，API 名称必须准确。
- 对象超过最大缓存时应丢弃而不是无限增长；必要时暴露 Acquire、Release、Using、Unused 等只读统计。
- 热路径避免不必要的反射、装箱和临时分配。

## 线程模型

- 当前默认按主线程使用。若模块被网络线程调用，要么在上层切回主线程，要么完整实现线程安全 Collector。
- 不只给静态 Dictionary 加锁；类型表、Stack、计数和 Clear 必须处于同一线程安全设计中。

## 已知状态

- 当前实现允许重复 Release，可能让 `UsingCount` 为负并把同一实例多次压栈。
- `Release(null)` 会在读取运行时类型时抛出空引用；`SetCapacity` 只设置首次创建时的容器容量，并不预热。

## 最低验证

- 首次创建、复用、Clear 调用、不同类型隔离和全池 Clear。
- null、重复、错误来源、错误类型 Release，以及对象构造或 Clear 抛异常。
- 若支持多线程，增加 Acquire/Release/Clear 并发压力测试。
