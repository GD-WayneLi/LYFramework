# LYUnity Client 层规范

## 简介

- 此目录用来实现Unity相关架构，不负责具体业务

## 分层职责

- `Model` 保存 Entity Component、接口骨架、枚举和轻量状态，不直接承载复杂流程或 Unity 生命周期入口。
- `System` 保存接入 GameManager 的行为和生命周期分发，通过 `ISystemAwake<T>`、`ISystemUpdate<T>`、`ISystemDispose<T>` 等接口处理 Component。
- Model 与 System 可以共同依赖 `LYFramework` 的 Core、Log、ReferencePool、Network 抽象，但不得依赖具体业务场景。
~~~~