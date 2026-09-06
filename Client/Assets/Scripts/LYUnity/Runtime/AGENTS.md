# LYUnity/Runtime 层规范

## 分层职责

- `Model` 保存 Entity Component、接口骨架、枚举和轻量状态，不直接承载复杂流程或 Unity 生命周期入口。
- `HotUpdate` 保存接入 GameManager 的行为和生命周期分发，通过 `ISystemAwake<T>`、`ISystemUpdate<T>`、`ISystemDispose<T>` 等接口处理 Component。
- `Model` 与 `System` 可以共同依赖 `LYFramework` ，但不得依赖具体业务场景。
- `Utility` 不在esc架构的其他工具
