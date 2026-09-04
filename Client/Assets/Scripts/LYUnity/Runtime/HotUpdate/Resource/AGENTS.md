# Resource 模块规范

## 模块职责

- ResourceLoaderComponent组件实现资源加载和卸载
- 需要加载资源的Entity添加此组件，调用`Load`方法加载资源。
- `ResourceUtility`中添加了Entity的扩展方法，此方法可以便捷的添加`ResourceLoaderComponent`组件并加载资源
- 在组件移除时会卸载所有通过此组件加载的资源，无需手动卸载