---
id: kd_69a7146e-2350-435f-bdd5-b721871e0149
injectMode: inherit
summary: 项目 UI Prefab、生成器与管理框架的结构索引。当前确认的 UI Prefab 位于 `Assets/Bundles/Prefabs/UI`，现有示例为 `Assets/Bundles/Prefabs/UI/UILogin.prefab`。
aiEditMode: inherit
---

- `Assets/Bundles/Prefabs/UI` 存放项目 UI Prefab，现有示例 `UILogin.prefab` 使用 Screen Space Overlay Canvas、CanvasScaler、GraphicRaycaster。
- UI 管理代码位于 `Assets/Scripts/LYUnity/Client/Model/UI` 与 `Assets/Scripts/LYUnity/Client/System/UI`；UI Prefab 由 UI 管理器运行时加载并实例化到 `UIRoot`，按 Canvas sortingOrder 管理深度。
- `Assets/Scripts/Editor/UITool/UIScriptGenerator.cs` 扫描名称中包含 `m_` 的节点，根据其 uGUI 组件生成 `<UIName>Component` 字段和 `<UIName>System.OnLoaded` 的 `Transform.Find` 绑定代码。
- 生成代码位于 `Assets/Scripts/Demo/Model/Client/Generage/UI` 与 `Assets/Scripts/Demo/System/Clinet/Generage/UI`；自定义 partial 代码位于对应的非 `Generage` 目录。生成目录中的文件可能被重新生成覆盖。
- UI Prefab 的制作、命名、绑定和 AI 读取流程维护在 `design/ui-prefab-rules.md`。
