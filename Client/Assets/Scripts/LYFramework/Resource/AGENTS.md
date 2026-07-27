# Resource 模块规范

本文件继承 `Assets/Scripts/LYFramework/AGENTS.md`，适用于资源加载与释放契约。

## 模块职责

- Resource 只定义跨项目资源服务契约和通用生命周期，不包含具体资源路径表、Prefab 类型或业务缓存规则。
- Resources、AssetBundle、Addressables 等 Unity 实现放在 `LYGame` 或独立适配程序集。
- Resource 可以实现 Core 的 Utility 契约，但不得依赖 UI、Network 或 Demo。

## 加载契约

- 明确同步/异步、成功/失败/取消、回调线程和回调次数；回调必须恰好完成一次。
- 优先使用可表达失败和取消的结果/句柄类型，不继续扩散无类型 `object`。
- 路径规范化、重复请求合并、缓存键和类型校验必须由接口或实现明确负责。
- 调用方关闭或销毁后，迟到结果不能泄漏资源或继续回调失效对象。

## 所有权与释放

- Load 返回的是实例、共享资产还是租约必须明确；Unload 对应哪个对象或句柄也必须明确。
- 重复 Unload、释放未加载对象和共享资源引用计数需要确定行为。
- 实例化对象和原始资产的销毁方式不可混用；Unity 对象释放必须在主线程执行。

## 已知状态

- 当前只有 `IResourceUtility.Load(string, Action<object>)` 与 `Unload(object)`，无法表达类型、失败、取消、进度或所有权。
- 当前没有具体资源 Utility 实现，也没有与 UI 加载流程接通。

## 最低验证

- 成功、失败、取消、重复请求、类型不匹配和迟到回调。
- 缓存命中、引用计数、重复 Unload、实例/资产分别释放。
- Unity 适配实现验证主线程回调和场景/Domain Reload 清理。
