using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LYFramework;
using LYFramework.Log;
using LYUnity.Resource;
using LYUnity.Utility.Unity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LYUnity.UI
{
    /// <summary>
    /// UIComponent 的无状态方法和 Entity 生命周期处理器。
    /// </summary>
    public class UISystem : SystemBase, ISystemAwake<UIComponent>, ISystemDispose<UIComponent>, ISystemUpdate<UIComponent>
    {
        public void Awake(UIComponent self)
        {
            self.State = UIState.Created;
            self.Path = null;
            self.Layer = 0;
            self.Depth = 0;
            self.IsVisible = false;
            self.UserData = null;
            self.Resource = null;
            self.LogicComponent = null;
            self.GameObject = null;
            
            self.Lifecycle = self.Owner?.Parent?.GetComponent<UIManagerComponent>()?.Lifecycle;
        }

        public void Dispose(UIComponent self)
        {
            var uiEntity = self.Owner?.Parent;
            var manager = uiEntity?.GetComponent<UIManagerComponent>();
            if (manager == null)
            {
                LYLogger.Warning("UIManagerComponent is Null.");
                return;
            }
            
            List<Exception> exceptions = null;
            
            try
            {
                self.Lifecycle.Close(self.LogicComponent);
                manager.OnUIClose(self);
            }
            catch (Exception exception)
            {
                exceptions = new List<Exception> { exception };
            }

            Object.Destroy(self.GameObject);
            
            self.LogicComponent = null;
            self.GameObject = null;
            self.Resource = null;
            self.UserData = null;
            self.IsVisible = false;
            self.State = UIState.Disposed;

            if (exceptions != null)
            {
                throw new AggregateException(
                    "UIComponent dispose lifecycle failed.",
                    exceptions);
            }
        }

        public void Update(UIComponent component)
        {
            if (component.IsVisible)
            {
                component.Lifecycle.Update(component.LogicComponent);
            }
        }
    }

    public static class UIComponentExtensions
    {
        public static void Configure(this UIComponent self, string path, int layer, object userData, Entity uiLogicComponent)
        {
            ThrowIfInvalid(self);
            
            if (self.State != UIState.Created)
            {
                throw new InvalidOperationException(
                    $"UIComponent cannot be configured in state: {self.State}");
            }

            self.Path = path;
            self.Layer = layer;
            self.Depth = layer;
            self.UserData = userData;
            self.LogicComponent = uiLogicComponent;
        }

        public static async ValueTask BeginLoading(this UIComponent self)
        {
            ThrowIfInvalid(self);

            if (self.State != UIState.Created)
            {
                throw new InvalidOperationException(
                    $"UIComponent cannot begin loading in state: {self.State}");
            }

            self.State = UIState.Loading;

            self.Resource = await self.Owner.Load<GameObject>(self.Path);
            if (!self.Resource)
            {
                return;
            }

            if (self.IsDisposed)
            {
                return;
            }

            self.GameObject = Object.Instantiate(self.Resource);
            self.IsVisible = true;
        }
        
        public static void SetOpen(this UIComponent self)
        {
            ThrowIfInvalid(self);

            if (self.State != UIState.Loading)
            {
                throw new InvalidOperationException(
                    $"UIComponent cannot open in state: {self.State}");
            }

            self.State = UIState.Open;
        }

        public static void SetDepth(this UIComponent self, int depth)
        {
            ThrowIfInvalid(self);

            if (self.Depth == depth)
            {
                return;
            }

            self.Depth = depth;
            
            var canvas = self.GameObject.GetComponent<Canvas>();
            if (!canvas)
            {
                LYLogger.Error("Canvas is Null.");
                return;
            }
            
            canvas.sortingOrder = depth;
        }

        public static void SetVisible(this UIComponent self, bool isVisible)
        {
            ThrowIfInvalid(self);

            if (self.IsVisible == isVisible)
            {
                return;
            }

            self.IsVisible = isVisible;
            
            self.GameObject.SetActiveEx(self.IsVisible);
        }

        private static void ThrowIfInvalid(UIComponent self)
        {
            if (self == null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            if (self.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UIComponent));
            }
        }
    }
}
