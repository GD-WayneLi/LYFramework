# AGENTS.md

## 最小入口规则

- 请使用全中文跟我沟通，代码和命令标识除外。
- 每次执行任何操作前，先说明要做什么，以及为什么要这么做。
- 开始工作前先检查 `git status`。工作区可能包含用户正在进行的重构；不要回退、覆盖或顺手整理与当前任务无关的改动。
- 只修改本次沟通相关代码或文档。若发现必须修改其他范围，先说明原因并询问。
- 所有 C# 方法声明必须写在单个物理行内，包括访问修饰符、返回类型、方法名、泛型参数、完整参数列表和泛型约束。
- 上述方法声明规则同时适用于接口方法、实现方法、构造函数和本地函数；不得因为参数较多、存在泛型约束或行宽过长而换行。
- 方法声明后的 `{` 仍遵循 Allman 风格，单独占行；XML 注释和 Attribute 可以单独占行。
- 本规则仅限制方法声明，不限制方法调用、类型继承列表和表达式的换行。

## 项目概况

- Unity：`2022.3.62f1`。
- 语言：C# 9，Unity 生成工程当前目标为 .NET Framework 4.7.1 / Unity .NET 4.x 兼容级别。
- `Assets/Scripts/LYFramework/LYFramework.asmdef` 定义可复用框架程序集 `LYFramework`。
- `Assets/Scripts/LYUnity/LYUnity.asmdef` 定义 Unity 与项目适配程序集 `LYUnity`，可引用 `LYFramework`、Unity 包和必要第三方包。

## 架构简介

- `Client/Assets/Scripts/LYFramework/Core` 是架构的核心区域
- 本架构以GameManager为整体管理脚本，Entity为基础单位，world为Entity管理根节点，Scene作为逻辑区域隔离区的树状管理结构
- Entity为生命周期管理节点，xxxComponent继承Entity作为Entity的组件，储存具体功能数据，Entity与Component不可直接new，必须使用AddChild/AddComponent进行创建。
- 数据与方法分离，生命周期方法定义在xxxSystem继承对应生命周期，实现对应接口，xxxExtensions作为Component的扩展方法，实现具体功能。

## 目录与职责

```text
Assets/Scripts/
├── LYFramework/              可复用框架程序集
│   ├── Core/                 GameManager、System、Controller、Entity 生命周期
│   ├── Event/                同步与延迟事件
│   ├── ReferencePool/        按运行时类型划分的对象池
│   ├── Network/              网络通道抽象、管理器与 Utility
│   └── Log/                  日志门面与接口
├── LYUnity/                  Unity 与当前项目适配程序集
│   ├── Client/Model/         UI、Resource 等运行时数据 Component 与接口骨架
│   ├── Client/System/        UI、Resource 等 System 与生命周期实现
│   ├── Log/                  Unity 日志适配
│   └── Utility/              Unity、Network 等具体适配器
└── Demo/                     启动与实验代码，不是正式 API 示例
```

依赖方向必须保持为：

```text
Demo  ──────>  LYUnity  ──────>  LYFramework
  └──────────────────────────>  LYFramework
```

- `LYFramework` 不得反向引用 `LYUnity`、`Demo` 或默认程序集中的类型。
- 通用接口、生命周期和跨项目可复用实现优先放入 `LYFramework`；Unity、YooAsset、项目配置、Prefab、Scene、Packet/UI 绑定等适配放入 `LYUnity`。