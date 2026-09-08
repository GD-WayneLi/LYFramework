using System;
using System.Threading.Tasks;
using LYFramework;
using LYFramework.Log;
using LYUnity.Utility.Unity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LYUnity.UI
{
    /// <summary>
    /// UIManagerComponent 的无状态方法和 Entity 生命周期处理器。
    /// </summary>
    public class UIManagerSystem : SystemBase, ISystemAwake<UIManagerComponent, GameObject>, ISystemDispose<UIManagerComponent>
    {
        public void Awake(UIManagerComponent self, GameObject gameObject)
        {
            var UIRoot = Resources.Load<GameObject>("UIRoot");
            self.UIRootSrc = UIRoot;
            self.UIRoot = Object.Instantiate(UIRoot, gameObject.transform);
            self.UIParent = UIRoot.transform.Find("Canvas/SafeArena");
            RefreshUILifecycle(self);
        }

        public void Dispose(UIManagerComponent self)
        {
            var lifecycle = self.Event;
            self.Event = null;
            lifecycle?.Dispose();
            self.LayerGroups.Clear();
            
            Object.DestroyImmediate(self.UIRoot);
            Resources.UnloadAsset(self.UIRootSrc);
        }

        public void RefreshUILifecycle(UIManagerComponent self)
        {
            var gameManager = ((IGetGameManager)this).GetGameManager();
            if (gameManager == null)
            {
                throw new InvalidOperationException("UIManagerSystem has not been registered in a GameManager.");
            }

            var lifecycle = CreateUIEvent(gameManager);
            var previousLifecycle = self.Event;
            self.Event = lifecycle;

            previousLifecycle?.Dispose();
        }

        private static UIEvent CreateUIEvent(IGameManager gameManager)
        {
            var lifecycle = new UIEvent();
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
    }

    public static class UIManagerExtensions
    {
        public static void RegisterLayerGroup(this UIManagerComponent self, ILayerGroup layerGroup)
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

        public static async ValueTask OpenUI<T>(this UIManagerComponent self, string path, int layer, object data = null) where T : Entity, IUILogicComponent, new()
        {
            ThrowIfUnavailable(self);

            Entity uiEntity = self.Owner.AddChild();
            self.UIStack.Add(uiEntity);

            var uiComponent = uiEntity.AddComponent<UIComponent>();
            var uiLogicComponent = uiEntity.AddComponent<T>();
            
            uiComponent.Configure(path, layer, data, uiLogicComponent);


            await uiComponent.BeginLoading();

            if (uiComponent.IsDisposed)
            {
                return;
            }
            
            var lifecycle = self.Event;

            lifecycle.Loaded(uiLogicComponent);
            
            self.SortUIDepth(uiComponent.Layer);
            self.SetUIVisible();

            uiComponent.SetOpen();
            lifecycle.Open(uiLogicComponent);
        }

        public static void CloseUI<T>(this UIManagerComponent self) where T : Entity, IUILogicComponent
        {
            ThrowIfUnavailable(self);

            for (var i = self.UIStack.Count - 1; i >= 0; i--)
            {
                var uiEntity = self.UIStack[i];
                if (uiEntity.GetComponent<T>() != null)
                {
                    self.CloseUIInternal(uiEntity);
                    return;
                }
            }
        }

        public static void CloseUI(this UIManagerComponent self, IUILogicComponent component)
        {
            ThrowIfUnavailable(self);

            if (component == null)
            {
                return;
            }

            if (component is Entity entity)
            {
                self.CloseUIInternal(entity.Parent);
            }
        }

        public static void PopUI(this UIManagerComponent self)
        {
            ThrowIfUnavailable(self);

            var count = self.UIStack.Count;
            if (count <= 0)
            {
                LYLogger.Warning("UIStack is empty, but PopUI was called.");
                return;
            }

            self.CloseUIInternal(self.UIStack[count - 1]);
        }

        public static void CloseUIAll(this UIManagerComponent self)
        {
            ThrowIfUnavailable(self);
            
            if (self == null)
            {
                return;
            }

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

        public static T GetUI<T>(this UIManagerComponent self) where T : Entity, IUILogicComponent
        {
            ThrowIfUnavailable(self);

            for (var i = self.UIStack.Count - 1; i >= 0; i--)
            {
                var uiEntity = self.UIStack[i];
                var component = uiEntity.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        public static bool HasUI<T>(this UIManagerComponent self) where T : Entity, IUILogicComponent
        {
            return GetUI<T>(self) != null;
        }

        internal static void OnUIClose(this UIManagerComponent self, UIComponent uiComponent)
        {
            self.UIStack.Remove(uiComponent.Parent);

            self.SortUIDepth(uiComponent.Layer);
            self.SetUIVisible();
        }
        
        private static void CloseUIInternal(this UIManagerComponent self, Entity uiEntity)
        {
            if (!IsOwnedUI(self, uiEntity))
            {
                return;
            }

            self.RemoveChild(uiEntity);
        }

        private static void SortUIDepth(this UIManagerComponent self, int layer)
        {
            var startDepth = layer;
            if (self.LayerGroups.TryGetValue(layer, out var layerGroup))
            {
                startDepth = layerGroup.StartDepth;
            }

            var uiEntities = self.UIStack.ToArray();
            foreach (var uiEntity in uiEntities)
            {
                var uiComponent = uiEntity.GetComponent<UIComponent>();
                if (uiComponent == null || uiComponent.Layer != layer)
                {
                    continue;
                }

                uiComponent.SetDepth(startDepth);

                startDepth += 100;
            }
        }

        private static void SetUIVisible(this UIManagerComponent self)
        {
            var hideLowerUI = false;
            var uiEntities = self.UIStack;

            for (var i = uiEntities.Count - 1; i >= 0; i--)
            {
                var uiEntity = uiEntities[i];

                var uiComponent = uiEntity.GetComponent<UIComponent>();
                if (uiComponent == null || uiComponent.State != UIState.Open)
                {
                    continue;
                }

                var isVisible = !hideLowerUI;
                uiComponent.SetVisible(isVisible);

                if (isVisible)
                {
                    hideLowerUI = true;
                }
            }
        }
        
        private static bool IsOwnedUI(this UIManagerComponent self, Entity uiEntity)
        {
            return self != null && uiEntity != null && !uiEntity.IsDisposed && self.UIStack.Contains(uiEntity);
        }

        private static void ThrowIfUnavailable(this UIManagerComponent self)
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

            if (self.Event == null)
            {
                throw new InvalidOperationException("UIManagerComponent did not receive its Awake lifecycle.");
            }
        }
    }
}