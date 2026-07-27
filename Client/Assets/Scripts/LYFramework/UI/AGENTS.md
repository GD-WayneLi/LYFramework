# UI 模块规范

本文件继承 `Assets/Scripts/LYFramework/AGENTS.md`，适用于 UI 抽象、Controller、层级和 UIManager。

## 依赖边界

- UI 可以依赖 Core、Log 和 Resource 抽象，不得依赖 `LYGame` 的具体加载器、Prefab 或业务界面。
- Unity GameObject/Canvas 的具体实现放在 `LYGame` 或独立 Unity 适配程序集。
- UIController 使用 System/Utility 前必须完成 IGameManager 注入。

## 生命周期状态

- 使用明确状态表示 Created、Loading、Open、Closing、Closed/Disposed，禁止用多个松散布尔值拼接隐含状态。
- Load 成功回调只执行一次；加载失败、取消、加载中关闭和重复关闭都有确定结果。
- OnOpen 只在资源准备完成后执行，OnClose 只对已打开或明确进入关闭流程的 UI 执行。
- Dispose 幂等，并完成资源卸载、事件解绑、Controller 清理和用户数据释放。

## 栈、层级与可见性

- 明确一个 UI 类型是否允许多实例，以及 CloseUI/GetUI 对多实例选择第一个、最后一个还是指定实例。
- Stack 顺序、Layer、Depth 是不同概念。Depth 应由 LayerGroup 的起始深度和组内顺序计算。
- 打开、关闭或层级变化后统一重算可见性；被遮挡 UI 是否 Update 必须由策略明确。
- 不在遍历 UI 列表时直接执行可能修改同一列表的用户回调。

## 异步与资源所有权

- 加载回调必须回到 Unity 主线程。
- UIManager 负责的资源与 UI 实例必须成对释放；关闭后的迟到加载结果要立即卸载，不能重新打开 UI。
- 回调参数优先使用具体泛型类型或结果类型，避免只传 `UIBase` 和无法表达失败的回调。

## 已知状态

- 当前 `UIBase.Load()` 不调用完成回调，`m_IsPrepared` 从未设为 true，默认实现不会进入 OnOpen/OnUpdate。
- 当前 `SetUIVisible()` 为空，LayerGroup 未接入，UIController 也没有在 UIManager 中初始化。

## 最低验证

- 同步加载、异步加载、加载失败、加载中关闭、重复打开/关闭和 CloseAll。
- 多 Layer/多实例的 Depth、栈顺序、Pop/Get/Has/Close 语义。
- UIController 依赖注入、可见性变化、Update 策略和资源释放次数。
