# Demo 区域规范

本文件继承仓库根目录 `AGENTS.md`，适用于 `Assets/Scripts/Demo/`。

## 目标

- Demo 用于展示框架推荐用法和提供最小可运行烟雾测试，不存放临时语言实验或生产业务逻辑。
- Demo 可以引用 `LYFramework` 和 `LYUnity`，其他运行时代码不得依赖 Demo。
- 每个示例应聚焦一个主题，名称表达用途，并能从场景或测试入口实际运行。

## 启动职责

- Bootstrap/Launcher 明确负责初始化、每帧 Update 和 Dispose，并避免重复初始化。
- 退出 Play Mode、销毁场景对象和应用退出时都能安全清理。
- 需要场景对象、资源或服务器时，在注释或 README 中写明前置条件。

## 代码质量

- 新示例使用 `LYUnity.Demo.*` 或项目约定的新命名空间，不继续添加 `QFramework.Demo`。
- 不使用 `a`、`b`、`testBase` 等实验命名，不保留大段注释代码。
- 示例应展示错误处理和正确释放，不只展示成功路径。

## 已知状态

- 当前 Launcher 主要是泛型类型判断实验，GameManager Init/Dispose 被注释。
- Launcher Scene 没有挂载 Launcher，EditorBuildSettings 也没有构建场景，因此当前 Demo 不是可运行启动模板。

## 最低验证

- 从干净 Play Mode 启动，无 Console Error，退出后无未释放 Socket、事件监听或静态单例。
- 示例场景引用和构建场景配置有效。
- 若示例对应框架缺陷，优先写自动化测试，不用人工 Demo 代替回归测试。
