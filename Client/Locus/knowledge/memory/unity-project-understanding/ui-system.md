---
id: kd_69a7146e-2350-435f-bdd5-b721871e0149
type: memory
path: unity-project-understanding/ui-system.md
title: ui-system
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
inheritAiConfig: true
createdAt: 1787912409438
updatedAt: 1787912409439
---

# ui-system

## Summary
项目 UI 目录、管理框架与 LoginView Prefab 的结构索引。

<!-- locus:body:start -->
- `Assets/Prefabs/UI` 存放项目 UI Prefab。
- UI 管理代码位于 `Assets/Scripts/LYUnity/Client/Model/UI` 与 `Assets/Scripts/LYUnity/Client/System/UI`；`UISystem` 加载并实例化根节点带 Canvas 的 UI Prefab，按 Canvas sortingOrder 管理深度。
- `Assets/Prefabs/UI/LoginView.prefab` 是 uGUI 登录界面：Screen Space Overlay Canvas、1920×1080 参考分辨率、账号 InputField（32 字符）与 LoginButton。Prefab 本身不包含业务登录回调，按钮事件由后续 UI 逻辑绑定。
<!-- locus:body:end -->
