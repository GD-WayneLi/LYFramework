# UI 模块规范

## 数据与 Entity 关系

- UIManager 和每个 UI 实例都使用基础 `Entity` 作为生命周期主体，不新增仅用于标识的 Entity 子类。
- `UIManagerComponent` 必须挂载在 UIManager 所属 Entity 上，只保存 UI 栈、`UILifecycle`、层级配置和依赖引用。
- 每个 UI Entity 必须同时挂载一个通用 `UIComponent` 和一个具体 `UIXxxComponent`；二者是兄弟 Component，具体 Component 不继承 `UIComponent`。
- UIManager 的栈保存 UI Entity，不保存具体 Component。关闭 UI 时销毁整个 UI Entity，由 Entity 统一销毁其 Component。

## System 与生命周期

- `UIManagerSystem` 和 `UISystem` 是无状态、可替换的实例 System，不得保存特定 Entity 或 Component。
- 具体 `UIXxxSystem` 必须注册到 GameManager，并通过 `IUILoaded<T>`、`IUIOpen<T>`、`IUIUpdate<T>`、`IUIClose<T>`、`IUIDepthChanged<T>`、`IUIVisibilityChanged<T>` 或 `IUIVisibilityPolicy<T>` 声明所处理的具体 UI Component。
- 所有公共方法显式接收目标 Component；运行时数据只存放在 Component 中。
- `UIManagerSystem` 通过 `ISystemAwake<UIManagerComponent>` 和 `ISystemDispose<UIManagerComponent>` 接入 Core 生命周期。
- `UISystem` 通过 `ISystemAwake<UIComponent>`、`ISystemUpdate<UIComponent>` 和 `ISystemDispose<UIComponent>` 接入 Core 生命周期。
- 必须先注册 `IUIManagerSystem`、`IUISystem` 和全部具体 `UIXxxSystem`，再创建 `UIManagerComponent`。其 Awake 生命周期从 GameManager 的 System 快照构建独立 `UILifecycle`；UI 更新统一由 `IGameManager.Update()` 驱动，不额外维护手工 Update 列表。
- GameManager 销毁时先销毁 World，因此 UI Component Dispose 期间对应 System 仍然有效；不得改变这一依赖顺序。

## UI 生命周期状态

- 使用 `Created`、`Loading`、`Open`、`Closing`、`Closed`、`Disposed` 表示状态，禁止用松散布尔值组合隐含状态。
- `OnOpen` 只在资源加载成功后执行；只有已经进入 Open 流程的 UI 才执行 `OnClose`。
- Close、Entity Dispose、Manager Component Dispose 和 World Dispose 必须汇入同一资源释放路径，并保持幂等。
- 加载中的 UI 被关闭后，迟到结果必须立即卸载，禁止重新打开已销毁 UI。
- `UILifecycle` 按具体 Component 类型保存 UI 专属 Loaded/Open/Close/Update/Depth/Visible Invoker；不得重新引入委托式 `UITypeDefinition`。
- GameManager 中的具体 UI System 发生替换后，使用 `RefreshUILifecycle` 从最新 System 快照原子重建 Invoker。

## 栈、层级与可见性

- Stack 表示打开顺序，Layer 表示显示层，Depth 表示 Layer 内最终渲染深度，三者不得混用。
- `CloseUI<T>` 和 `GetUI<T>` 默认选择最早打开的同类型实例；`PopUI` 关闭最后打开的实例。
- Depth 使用 `LayerGroup.StartDepth + 组内序号 * 100`；没有 LayerGroup 时以 Layer 值作为起始深度。
- 打开、关闭和销毁 UI 后统一重算 Depth 与可见性。`HideLowerUI` 从栈顶向下遮挡较低 UI。
- 遍历 UI 栈并调用用户回调时必须使用快照或受控修改流程。

## 资源与线程

- UI 通用 System 可以依赖 Core、Log 和 Resource 抽象；Prefab、Canvas 和业务 UI 绑定应通过 Component 字段或适配器进入，不在通用生命周期里硬编码。
- Unity GameObject、Canvas、Addressables、YooAsset 等适配实现保留在 LYUnity 或独立 Unity 适配程序集。
- 资源加载完成回调必须回到 Unity 主线程。UI Entity 销毁后的迟到结果由发起加载时捕获的 ResourceUtility 卸载。
- 资源获取和释放必须严格成对；OnClose、Unload 或出栈回调抛异常时仍须继续完成其余清理。