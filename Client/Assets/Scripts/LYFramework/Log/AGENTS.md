# Log 模块规范

本文件继承 `Assets/Scripts/LYFramework/AGENTS.md`，适用于日志接口和框架日志门面。

## 模块边界

- Log 是低层能力，不得依赖 Core、Event、Network、UI、Resource 或具体游戏代码。
- `LYFramework` 中保留日志契约和与引擎无关的门面；Unity `Debug`、文件、远端上报等实现放在 `LYGame`。
- 其他模块只依赖 `ILogHelper`/`LYLogger`，不要直接散落 `UnityEngine.Debug`。

## 行为约定

- 未设置 Helper 时的行为要明确：静默、降级或开发环境报错只能选择一种一致策略。
- 日志系统本身不应让业务流程因 Helper 异常而崩溃；若需要传播异常，应由显式配置控制。
- 支持后台线程调用时，Helper 的线程安全与 Unity 主线程限制必须在契约中说明。
- 日志包含模块、错误码和必要上下文，避免记录 Token、账号凭据或完整敏感包体。

## 生命周期

- Helper 替换、清空和 Unity Domain Reload 后的静态状态必须可预测。
- 若 Helper 持有文件、线程或网络资源，应实现并由拥有方负责 Dispose；静态门面不隐式接管所有权，除非 API 明确说明。

## 已知状态

- 当前 `LYLogger` 在 Helper 为空时静默丢弃日志。
- `DefaultLogHelper` 位于 `LYGame` 路径但使用 `LYFramework.Log` 命名空间，新增实现应按实际层级命名。

## 最低验证

- Helper 未设置、替换、清空，各日志级别正确转发。
- Helper 抛异常和多线程写入的策略测试。
- Unity 适配器单独验证主线程要求和格式输出。
