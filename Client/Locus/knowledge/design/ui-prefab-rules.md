---
id: kd_cf943365-ed4d-43d4-9bc2-91b115898aa1
injectMode: inherit
summary: 项目 UI Prefab 的统一目录、层级、m_ 控件命名、脚本生成、业务逻辑和 AI 自动拼界面流程规范。
aiEditMode: inherit
---

# UI Prefab 制作与自动绑定规则

## 1. 适用范围

本规则适用于项目中所有由 UI 管理器运行时加载的 uGUI 界面 Prefab，目标是让 AI 和编辑器生成器能够稳定读取层级、识别控件并生成绑定代码。

## 2. Prefab 位置与命名

- UI Prefab 统一放在 `Assets/Bundles/Prefabs/UI/`。
- Prefab 根节点名必须与 Prefab 文件名一致，例如 `UIShop.prefab` 的根节点命名为 `UIShop`。
- UI 逻辑类使用同名后缀：`UIShopComponent`、`UIShopSystem`。
- UI 不直接放入场景，运行时由 UI 管理器实例化并挂载到 `UIRoot`。

## 3. 根节点规范

根节点通常包含：

- `Canvas`

Canvas 使用 `Screen Space - Overlay`。参考分辨率统一为 `900 x 1600`，除非界面有明确的特殊需求。

推荐的通用层级：

```text
UIName
├─ Background
└─ Panel
   ├─ Header
   ├─ Content
   └─ Footer
```

层级名称可以按界面实际需求调整，但应保持语义清晰、层级稳定。

## 4. 控件绑定命名

需要由代码访问的 GameObject 必须使用 `m_` 前缀，例如：

```text
m_Title
m_CloseButton
m_NameInput
m_ConfirmButton
m_ItemList
```

当前 `UIScriptGenerator` 会扫描名称中包含 `m_` 的节点，并根据节点上的 uGUI 组件生成字段和 `Transform.Find` 绑定代码。因此：

- 需要绑定的节点必须命名为 `m_语义名称`。
- 不需要绑定的纯装饰节点不要使用 `m_`。
- `m_` 节点名称必须唯一。
- 生成绑定代码后，不要随意修改 `m_` 节点名称或其父级层级；如果必须修改，应重新生成 UI 脚本。
- `m_` 节点应直接挂载目标组件，减少依赖隐藏子节点的查找逻辑。

推荐后缀与组件：

| 命名后缀 | 目标组件 |
|---|---|
| `Button` | `Button` |
| `Input` | `TMP_InputField` 或 `InputField` |
| `Text` | `TMP_Text` 或 `TextMeshProUGUI` |
| `Image` | `Image` |
| `Toggle` | `Toggle` |
| `Slider` | `Slider` |
| `ScrollView` | `ScrollRect` |
| `List` | 列表容器 `Transform` 或 `GameObject` |

例如：`m_LoginButton`、`m_AccountInput`、`m_TitleText`。

## 5. 业务逻辑规范

- `OnLoaded` 只负责控件引用初始化。
- 按钮监听、数据刷新和交互逻辑写在非生成目录下的自定义 partial System 中。
- 需要监听的事件在 `OnOpen` 注册，在 `OnClose` 注销。
- 不要修改 `Assets/Scripts/Demo/**/Generage/` 下的生成文件；重新生成时这些文件会被覆盖。
- 现有代码生成器生成的目录拼写 `Generage`、`Clinet` 保持不变。
- UI 打开、关闭和层级管理遵循现有 `UIManagerComponent`、`UIManagerSystem`、`UISystem` 流程，不新增独立的 UI 基类体系。

## 6. AI 自动拼界面流程

AI 创建或修改 UI 时按以下顺序执行：

1. 确认目标 Prefab 路径、根节点和现有层级。
2. 读取 Prefab hierarchy，确认目标节点的完整路径。
3. 读取目标节点组件，确认实际绑定类型、布局组件和现有序列化配置。
4. 先复用项目已有节点、组件、材质和字体，避免无必要地重建资源。
5. 新增可交互或需要逻辑访问的节点时使用唯一的 `m_` 名称。
6. 通过 Unity API 修改 Prefab，不直接编辑 Unity YAML。
7. 需要代码绑定时使用 `Assets/生成UI脚本` 生成脚本。
8. 只在自定义 partial 文件中补充业务逻辑，并重新编译检查。
9. 检查所有 `Transform.Find` 路径、组件类型和 Prefab 引用是否仍然有效。

## 7. 交付检查清单

- Prefab 位于规定目录，根节点和文件名一致。
- 根节点 Canvas 配置正确。
- 所有需要绑定的节点都以 `m_` 开头且名称唯一。
- `m_` 节点直接挂载预期组件。
- 生成脚本中的层级路径与 Prefab 实际层级一致。
- 业务代码没有写入生成目录。
- 打开界面使用现有 UI 管理流程。
