# LYFramework 程序集规范

本文件继承仓库根目录 `AGENTS.md`，适用于 `Assets/Scripts/LYFramework/`。子模块存在更深层 `AGENTS.md` 时，还应遵循对应的局部规则。

## 职责与依赖边界

- 本目录承载可复用框架程序集 `LYFramework`，不得引用 `LYGame`、`Demo` 或默认程序集中的类型。
- 依赖方向保持从高层模块流向低层基础能力，禁止用静态单例或反射绕过程序集/模块边界。
- Core 是生命周期与对象关系基础；Log、ReferencePool 应保持低依赖；Network、UI、Resource 不得彼此形成循环依赖。
- Unity 或具体项目适配器放在 `LYGame`。框架层优先依赖接口，不直接依赖 Prefab、Scene、Addressables 配置或项目业务类型。

## 公共 API

- 新增公共类型前先确认它是跨项目可复用契约，而不是当前游戏的便捷封装。
- 公共 API 必须说明所有权、线程、生命周期、失败方式和回调所在上下文。
- 破坏性修改要同步更新所有调用方、示例、测试和对应模块的 `AGENTS.md`。
- 避免无约束的 `object`、全局可变状态和含糊的布尔返回值；优先使用明确类型、状态或结果对象。

## Unity 边界

- 不在后台线程调用 Unity API。
- 若代码可以不依赖 Unity，则不要为了日志、时间或调度直接引入 `UnityEngine`；通过接口在 `LYGame` 注入适配实现。
- 不手工修改 Unity 生成的工程文件。程序集边界以 asmdef 和 Unity 编译结果为准。

## 完成标准

- 相关模块的局部验证全部通过。
- 没有新增从 `LYFramework` 指向 `LYGame`/`Demo` 的依赖。
- 新增 Assets 文件具有唯一且配套的 `.meta`。
- 公共行为变化已经更新仓库根目录和对应模块文档。
