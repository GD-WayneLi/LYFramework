using System;
using LYFramework.Log;
using LYFramework.Resource;

namespace LYFramework.UI
{
    /// <summary>
    /// UIManager 的公共方法契约。
    /// </summary>
    public interface IUIManagerSystem : ISystem
    {
        void RefreshUILifecycle(UIManagerComponent self);

        void RegisterLayerGroup(UIManagerComponent self, ILayerGroup layerGroup);

        void OpenUI<T>(UIManagerComponent self, string path, int layer, object data = null) where T : Entity, new();

        void CloseUI<T>(UIManagerComponent self) where T : Entity;

        void CloseUI(UIManagerComponent self, Entity entityOrComponent);

        void PopUI(UIManagerComponent self);

        void CloseUIAll(UIManagerComponent self);

        T GetUI<T>(UIManagerComponent self) where T : Entity;

        bool HasUI<T>(UIManagerComponent self) where T : Entity;

        void HandleLoadCompleted(UIManagerComponent self, Entity uiEntity, int loadVersion, object resource, IResourceUtility resourceUtility);

        void HandleUIComponentDisposed(UIManagerComponent self, Entity uiEntity, int layer);
    }

    /// <summary>
    /// UIManagerComponent 的无状态方法和 Entity 生命周期处理器。
    /// </summary>
    public class UIManagerSystem : SystemBase, IUIManagerSystem, ISystemAwake<UIManagerComponent>, ISystemDispose<UIManagerComponent>
    {
        public void Awake(UIManagerComponent self)
        {
            var gameManager = ((IGetGameManager)this).GetGameManager();
            if (gameManager == null)
            {
                throw new InvalidOperationException("UIManagerSystem has not been registered in a GameManager.");
            }

            var lifecycle = CreateLifecycle(gameManager);
            self.GameManager = gameManager;
            self.ResourceUtility = gameManager.GetUtility<IResourceUtility>();
            self.IsClosingAll = false;
            self.Lifecycle = lifecycle;
        }

        public void Dispose(UIManagerComponent self)
        {
            CloseUIAllInternal(self);

            var lifecycle = self.Lifecycle;
            self.Lifecycle = null;
            lifecycle?.Dispose();
            self.LayerGroups.Clear();
            self.ResourceUtility = null;
            self.GameManager = null;
        }

        public void RefreshUILifecycle(UIManagerComponent self)
        {
            ThrowIfUnavailable(self);

            var lifecycle = CreateLifecycle(self.GameManager);
            var previousLifecycle = self.Lifecycle;
            self.Lifecycle = lifecycle;
            previousLifecycle.Dispose();
        }

        public void RegisterLayerGroup(UIManagerComponent self, ILayerGroup layerGroup)
        {
            ThrowIfUnavailable(self);

            if (layerGroup == null)
            {
                throw new ArgumentNullException(nameof(layerGroup));
            }

            if (!self.LayerGroups.TryAdd(layerGroup.Layer, layerGroup))
            {
                throw new InvalidOperationException($"UI layer is already registered: {layerGroup.Layer}");
            }
        }

        public void OpenUI<T>(UIManagerComponent self, string path, int layer, object data = null) where T : Entity, new()
        {
            ThrowIfUnavailable(self);

            if (self.IsClosingAll)
            {
                throw new InvalidOperationException("A UI cannot be opened while CloseUIAll is running.");
            }

            var dataComponentType = typeof(T);
            if (dataComponentType == typeof(Entity) || dataComponentType == typeof(UIComponent) || dataComponentType == typeof(UIManagerComponent))
            {
                throw new InvalidOperationException($"Invalid concrete UI component type: {dataComponentType.FullName}");
            }

            var uiSystem = GetUISystem(self);
            Entity uiEntity = null;

            try
            {
                uiEntity = self.Parent.AddChild<Entity>();
                var uiComponent = uiEntity.AddComponent<UIComponent>();
                var dataComponent = uiEntity.AddComponent<T>();
                uiSystem.Configure(uiComponent, path, layer, data, dataComponent);

                self.UIStack.Add(uiEntity);

                if (IsManagedUI(self, uiEntity))
                {
                    BeginLoad(self, uiEntity, uiComponent, uiSystem);
                }
            }
            catch (Exception openException)
            {
                self.UIStack.Remove(uiEntity);

                try
                {
                    uiEntity?.Dispose();
                }
                catch (Exception disposeException)
                {
                    throw new AggregateException("UI open and rollback both failed.", openException, disposeException);
                }

                throw;
            }
        }

        public void CloseUI<T>(UIManagerComponent self) where T : Entity
        {
            ThrowIfUnavailable(self);

            for (var i = 0; i < self.UIStack.Count; i++)
            {
                var uiEntity = self.UIStack[i];
                if (uiEntity.GetComponent<T>() != null)
                {
                    CloseUIInternal(self, uiEntity);
                    return;
                }
            }
        }

        public void CloseUI(UIManagerComponent self, Entity entityOrComponent)
        {
            ThrowIfUnavailable(self);

            if (entityOrComponent == null)
            {
                return;
            }

            CloseUIInternal(self, entityOrComponent.GetUIEntity());
        }

        public void PopUI(UIManagerComponent self)
        {
            ThrowIfUnavailable(self);

            var count = self.UIStack.Count;
            if (count <= 0)
            {
                LYLogger.Warning("UIStack is empty, but PopUI was called.");
                return;
            }

            CloseUIInternal(self, self.UIStack[count - 1]);
        }

        public void CloseUIAll(UIManagerComponent self)
        {
            ThrowIfUnavailable(self);
            CloseUIAllInternal(self);
        }

        public T GetUI<T>(UIManagerComponent self) where T : Entity
        {
            ThrowIfUnavailable(self);

            foreach (var uiEntity in self.UIStack)
            {
                var component = uiEntity.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        public bool HasUI<T>(UIManagerComponent self) where T : Entity
        {
            return GetUI<T>(self) != null;
        }

        public void HandleLoadCompleted(UIManagerComponent self, Entity uiEntity, int loadVersion, object resource, IResourceUtility resourceUtility)
        {
            if (!IsManagedUI(self, uiEntity))
            {
                Unload(resourceUtility, resource);
                return;
            }

            var uiComponent = uiEntity.GetComponent<UIComponent>();
            if (uiComponent == null || uiComponent.State != UIState.Loading || uiComponent.LoadVersion != loadVersion)
            {
                Unload(resourceUtility, resource);
                return;
            }

            var dataComponent = uiComponent.DataComponent;
            if (dataComponent == null || dataComponent.IsDisposed)
            {
                Unload(resourceUtility, resource);
                CloseUIInternal(self, uiEntity);
                return;
            }

            var uiSystem = GetUISystem(self);
            var lifecycle = self.Lifecycle;

            try
            {
                uiSystem.SetOpen(uiComponent, resource);
                lifecycle.Loaded(dataComponent, resource);

                if (!IsManagedOpenUI(self, uiEntity, uiComponent))
                {
                    return;
                }

                SortUIDepth(self, uiComponent.Layer, uiSystem);
                SetUIVisible(self, uiSystem);
                uiSystem.MarkOpened(uiComponent);
                lifecycle.Open(dataComponent);
            }
            catch
            {
                if (IsManagedUI(self, uiEntity))
                {
                    CloseUIInternal(self, uiEntity);
                }

                throw;
            }
        }

        public void HandleUIComponentDisposed(UIManagerComponent self, Entity uiEntity, int layer)
        {
            if (self == null)
            {
                return;
            }

            self.UIStack.Remove(uiEntity);

            if (self.IsDisposed || self.Parent == null || self.Parent.IsDisposed)
            {
                return;
            }

            var uiSystem = self.GameManager?.GetSystem<IUISystem>();
            if (uiSystem == null || self.Lifecycle == null)
            {
                return;
            }

            SortUIDepth(self, layer, uiSystem);
            SetUIVisible(self, uiSystem);
        }

        private static UILifecycle CreateLifecycle(IGameManager gameManager)
        {
            var lifecycle = new UILifecycle();
            try
            {
                lifecycle.RegisterSystems(gameManager.GetSystems());
                return lifecycle;
            }
            catch
            {
                lifecycle.Dispose();
                throw;
            }
        }

        private static void CloseUIAllInternal(UIManagerComponent self)
        {
            if (self == null || self.IsClosingAll)
            {
                return;
            }

            self.IsClosingAll = true;
            try
            {
                var uiEntities = self.UIStack.ToArray();
                foreach (var uiEntity in uiEntities)
                {
                    try
                    {
                        CloseUIInternal(self, uiEntity);
                    }
                    catch (Exception exception)
                    {
                        LYLogger.Error(exception.ToString());
                    }
                }
            }
            finally
            {
                self.IsClosingAll = false;
            }
        }

        private static void BeginLoad(UIManagerComponent self, Entity uiEntity, UIComponent uiComponent, IUISystem uiSystem)
        {
            var resourceUtility = self.ResourceUtility;
            var loadVersion = uiSystem.BeginLoading(uiComponent);

            if (resourceUtility == null || string.IsNullOrEmpty(uiComponent.Path))
            {
                DispatchLoadCompleted(self, uiEntity, loadVersion, null, resourceUtility);
                return;
            }

            try
            {
                resourceUtility.Load(uiComponent.Path, resource => DispatchLoadCompleted(self, uiEntity, loadVersion, resource, resourceUtility));
            }
            catch
            {
                CloseUIInternal(self, uiEntity);
                throw;
            }
        }

        private static void DispatchLoadCompleted(UIManagerComponent self, Entity uiEntity, int loadVersion, object resource, IResourceUtility resourceUtility)
        {
            var currentSystem = self?.GameManager?.GetSystem<IUIManagerSystem>();
            if (currentSystem == null)
            {
                Unload(resourceUtility, resource);
                return;
            }

            currentSystem.HandleLoadCompleted(self, uiEntity, loadVersion, resource, resourceUtility);
        }

        private static void CloseUIInternal(UIManagerComponent self, Entity uiEntity)
        {
            if (!IsOwnedUI(self, uiEntity))
            {
                return;
            }

            var uiComponent = uiEntity.GetComponent<UIComponent>();
            if (uiComponent == null)
            {
                self.UIStack.Remove(uiEntity);
                uiEntity.Dispose();
                return;
            }

            var uiSystem = GetUISystem(self);
            if (!uiSystem.BeginClosing(uiComponent))
            {
                return;
            }

            uiEntity.Dispose();
        }

        private static void SortUIDepth(UIManagerComponent self, int layer, IUISystem uiSystem)
        {
            var startDepth = layer;
            if (self.LayerGroups.TryGetValue(layer, out var layerGroup))
            {
                startDepth = layerGroup.StartDepth;
            }

            var uiEntities = self.UIStack.ToArray();
            foreach (var uiEntity in uiEntities)
            {
                if (!IsManagedUI(self, uiEntity))
                {
                    continue;
                }

                var uiComponent = uiEntity.GetComponent<UIComponent>();
                if (uiComponent == null || uiComponent.Layer != layer)
                {
                    continue;
                }

                if (uiSystem.SetDepth(uiComponent, startDepth))
                {
                    var dataComponent = uiComponent.DataComponent;
                    if (dataComponent != null && !dataComponent.IsDisposed)
                    {
                        self.Lifecycle.DepthChanged(dataComponent, startDepth);
                    }
                }

                startDepth += 100;
            }
        }

        private static void SetUIVisible(UIManagerComponent self, IUISystem uiSystem)
        {
            var hideLowerUI = false;
            var uiEntities = self.UIStack.ToArray();

            for (var i = uiEntities.Length - 1; i >= 0; i--)
            {
                var uiEntity = uiEntities[i];
                if (!IsManagedUI(self, uiEntity))
                {
                    continue;
                }

                var uiComponent = uiEntity.GetComponent<UIComponent>();
                var dataComponent = uiComponent?.DataComponent;
                if (uiComponent == null || uiComponent.State != UIState.Open || dataComponent == null || dataComponent.IsDisposed)
                {
                    continue;
                }

                var isVisible = !hideLowerUI;
                var hidesLowerUI = self.Lifecycle.GetHideLowerUI(dataComponent.GetType());
                if (uiSystem.SetVisible(uiComponent, isVisible))
                {
                    self.Lifecycle.VisibilityChanged(dataComponent, isVisible);
                }

                if (isVisible && hidesLowerUI && IsManagedOpenUI(self, uiEntity, uiComponent))
                {
                    hideLowerUI = true;
                }
            }
        }

        private static IUISystem GetUISystem(UIManagerComponent self)
        {
            var uiSystem = self.GameManager?.GetSystem<IUISystem>();
            if (uiSystem == null)
            {
                throw new InvalidOperationException("IUISystem must be registered before opening a UI.");
            }

            return uiSystem;
        }

        private static bool IsManagedUI(UIManagerComponent self, Entity uiEntity)
        {
            return self != null && !self.IsDisposed && IsOwnedUI(self, uiEntity);
        }

        private static bool IsOwnedUI(UIManagerComponent self, Entity uiEntity)
        {
            return self != null && uiEntity != null && !uiEntity.IsDisposed && self.UIStack.Contains(uiEntity);
        }

        private static bool IsManagedOpenUI(UIManagerComponent self, Entity uiEntity, UIComponent uiComponent)
        {
            return IsManagedUI(self, uiEntity) && uiComponent.State == UIState.Open;
        }

        private static void Unload(IResourceUtility resourceUtility, object resource)
        {
            if (resourceUtility != null && resource != null)
            {
                resourceUtility.Unload(resource);
            }
        }

        private static void ThrowIfUnavailable(UIManagerComponent self)
        {
            if (self == null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            if (self.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UIManagerComponent));
            }

            if (!self.IsComponent || self.Parent == null || self.Parent.IsDisposed)
            {
                throw new InvalidOperationException("UIManagerComponent is not attached to a valid Entity.");
            }

            if (self.GameManager == null || self.Lifecycle == null)
            {
                throw new InvalidOperationException("UIManagerComponent did not receive its Awake lifecycle.");
            }
        }
    }
}
