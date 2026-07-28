# Core 模块规范

本文件继承 `Assets/Scripts/LYFramework/AGENTS.md`，适用于 `Core/` 及其 Controller、Entity、Interface、System、Utility 子目录。

## 模块职责

- Core 只负责对象生命周期、服务注册/查询、System/Controller 基础契约和 Entity 树。
- 不在 Core 中加入 UI、网络协议、资源路径、业务事件或具体游戏状态。
- 新代码避免直接依赖 `UnityEngine`；日志或错误报告通过框架抽象完成。

## GameManager 与服务注册

- 明确注册键是“契约类型”还是“实现类型”，注册与查询必须使用同一规则。
- 重复注册、初始化异常和部分初始化必须有确定结果与回滚，不得只记录日志后留下半初始化对象。
- 初始化顺序必须确定，销毁默认按初始化逆序执行。
- System、Utility、Controller 的生命周期接口需要保持一致语义；修改其中之一时检查所有实现类。
- 单例销毁后必须可以安全重建，且旧对象或异步回调不能访问新实例。

## Entity 不变量

- 一个 Entity 最多有一个 Parent；Child 和 Component 两种关系互斥。
- 禁止自引用和父子环；Reparent 失败时原树、Scene 索引和所有权必须保持不变。
- Parent 拥有 Child/Component。默认移除会销毁，游离操作必须显式请求。
- Scene 索引必须与整棵树一致；Add、Remove、Reparent、Dispose 中途抛异常时也不能遗留脏索引。
- 遍历公开集合时考虑调用方修改树；不要直接暴露可变字典。
- Dispose 幂等，且递归销毁后清除 Parent、Scene、Component 标记和生命周期标识。

## 已知状态

- `GameManagerBase<T>` 持有独立的 `World`，自身不参与 Entity 树。`World` 仅管理 Scene，Scene 自身是 Entity 树根节点并维护完整 Id 索引。
- 当前 Utility 没有统一 Init/Dispose 契约。修复应作为完整生命周期设计处理，不做局部补丁。

## 最低验证

- 服务注册、重复注册、缺失查询、初始化失败回滚、销毁顺序和重复 Dispose。
- Entity Add/Remove/Reparent、环检测、Component 唯一性、跨 Scene 迁移、索引一致性。
- 父节点销毁、子节点主动销毁、游离后重新挂载，以及操作失败后的原子性。
