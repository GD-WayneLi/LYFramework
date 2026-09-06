# LYUnity 层规范

## 简介

- 用来实现Unity相关架构，不负责具体业务
- 承载Unity、第三方包和当前项目相关适配，可引用 `LYFramework`，不得让 `LYFramework` 反向依赖本目录。
- 不可引用业务层

## 分层职责

- `Editor`文件夹为编辑器目录，`Runtime`文件夹为运行时目录