using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LYFramework;
using LYFramework.Resource;
using LYUnity.Resource;

namespace LYUnity.UI
{
    /// <summary>
    /// UIComponent 的公共方法契约。
    /// </summary>
    public interface IUISystem : ISystem
    {
        void Configure(UIComponent self, string path, int layer, object userData, Entity dataComponent);

        ValueTask BeginLoading(UIComponent self);
        
        void SetOpen(UIComponent self);

        bool SetDepth(UIComponent self, int depth);

        bool SetVisible(UIComponent self, bool isVisible);
    }

    /// <summary>
    /// UIComponent 的无状态方法和 Entity 生命周期处理器。
    /// </summary>
    public class UISystem : SystemBase, IUISystem, ISystemAwake<UIComponent>, ISystemDispose<UIComponent>
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
            self.LoadVersion = 1;
            self.WasOpened = false;
        }

        public void Configure(UIComponent self, string path, int layer, object userData, Entity dataComponent)
        {
            ThrowIfInvalid(self);

            if (dataComponent == null)
            {
                throw new ArgumentNullException(nameof(dataComponent));
            }

            if (dataComponent.IsDisposed || !dataComponent.IsComponent || !ReferenceEquals(dataComponent.Parent, self.Parent))
            {
                throw new InvalidOperationException("The concrete UI Component must be attached to the same UI Entity as UIComponent.");
            }

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

        public async ValueTask BeginLoading(UIComponent self)
        {
            ThrowIfInvalid(self);

            if (self.State != UIState.Created)
            {
                throw new InvalidOperationException(
                    $"UIComponent cannot begin loading in state: {self.State}");
            }

            self.State = UIState.Loading;
            var resLoader = self.Parent.GetComponent<ResourceLoaderComponent>();
            
            var manager = self.GetUIManagerComponent();

            self.Resource = await manager.ResourceUtility.Load(self.Path);
            if (self.IsDisposed)
            {
                manager.ResourceUtility.Unload(self.Resource);
            }
        }

        public void SetOpen(UIComponent self)
        {
            ThrowIfInvalid(self);

            if (self.State != UIState.Loading)
            {
                throw new InvalidOperationException(
                    $"UIComponent cannot open in state: {self.State}");
            }

            self.State = UIState.Open;
        }

        public bool SetDepth(UIComponent self, int depth)
        {
            ThrowIfInvalid(self);

            if (self.Depth == depth)
            {
                return false;
            }

            self.Depth = depth;
            return true;
        }

        public bool SetVisible(UIComponent self, bool isVisible)
        {
            ThrowIfInvalid(self);

            if (self.IsVisible == isVisible)
            {
                return false;
            }

            self.IsVisible = isVisible;
            return true;
        }
        
        public void Dispose(UIComponent self)
        {
            var uiEntity = self.Parent;
            var manager = self.GetUIManagerComponent();
            List<Exception> exceptions = null;

            self.LoadVersion++;
            self.State = UIState.Closing;
            manager?.UIStack.Remove(uiEntity);

            try
            {
                if (self.WasOpened && manager?.Lifecycle != null && self.DataComponent != null)
                {
                    manager.Lifecycle.Close(self.DataComponent);
                }
            }
            catch (Exception exception)
            {
                exceptions = new List<Exception> { exception };
            }

            try
            {
                if (self.Resource != null)
                {
                    manager?.ResourceUtility?.Unload(self.Resource);
                }
            }
            catch (Exception exception)
            {
                (exceptions ??= new List<Exception>()).Add(exception);
            }
            finally
            {
                self.Resource = null;
                self.UserData = null;
                self.IsVisible = false;
                self.State = UIState.Disposed;
            }

            if (exceptions != null)
            {
                throw new AggregateException(
                    "UIComponent dispose lifecycle failed.",
                    exceptions);
            }
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

    public static class UIEntityExtensions
    {
        public static Entity GetUIEntity(this Entity self)
        {
            if (self == null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            return self.IsComponent ? self.Parent : self;
        }

        public static UIComponent GetUIComponent(this Entity self)
        {
            return self.GetUIEntity()?.GetComponent<UIComponent>();
        }

        public static UIManagerComponent GetUIManagerComponent(this Entity self)
        {
            var uiEntity = self.GetUIEntity();
            return uiEntity?.Parent?.GetComponent<UIManagerComponent>();
        }
    }
}
