using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LYFramework;
using LYFramework.Log;
using LYFramework.Resource;
using LYUnity.Resource;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LYUnity.UI
{
    /// <summary>
    /// UIComponent 的无状态方法和 Entity 生命周期处理器。
    /// </summary>
    public class UISystem : SystemBase, ISystemAwake<UIComponent>, ISystemDispose<UIComponent>
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
        }

        public void Dispose(UIComponent self)
        {
            var uiEntity = self.Parent?.Parent;
            var manager = uiEntity?.GetComponent<UIManagerComponent>();
            if (manager == null)
            {
                LYLogger.Warning("UIManagerComponent is Null.");
                return;
            }
            
            List<Exception> exceptions = null;
            
            try
            {
                manager.OnUIClose(self);
            }
            catch (Exception exception)
            {
                exceptions = new List<Exception> { exception };
            }

            Object.Destroy(self.GameObject);
            
            var resLoader = self.Parent.GetComponent<ResourceLoaderComponent>();
            if (self.Resource != null && !resLoader.IsDisposed)
            {
                resLoader.Unload(self.Resource);
            }
            
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
    }

    public static class UIComponentExtensions
    {
        public static void Configure(this UIComponent self, string path, int layer, object userData)
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

            var resLoader = self.Parent.GetComponent<ResourceLoaderComponent>();

            self.Resource = await resLoader.Load(self.Path);

            if (self.IsDisposed && !resLoader.IsDisposed)
            {
                resLoader.Unload(self.Resource);
                return;
            }

            self.GameObject = Object.Instantiate(self.Resource as GameObject);
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
            // todo : 处理层级
        }

        public static void SetVisible(this UIComponent self, bool isVisible)
        {
            ThrowIfInvalid(self);

            if (self.IsVisible == isVisible)
            {
                return;
            }

            self.IsVisible = isVisible;
            
            // todo：处理显示
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

            if (!self.IsComponent || self.Parent == null)
            {
                throw new InvalidOperationException(
                    "UIComponent is not attached to a valid Entity.");
            }
        }
    }
}