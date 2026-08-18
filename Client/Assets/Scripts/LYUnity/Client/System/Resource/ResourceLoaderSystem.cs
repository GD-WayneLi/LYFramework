using System.Threading.Tasks;
using System;
using LYUnity.Resource;

namespace LYFramework.Resource
{
    public class ResourceLoaderSystem : SystemBase, ISystemAwake<ResourceLoaderComponent>, ISystemDispose<ResourceLoaderComponent>
    {
        public void Awake(ResourceLoaderComponent component)
        {
            component.ResourceUtility = this.GetUtility<IResourceUtility>();
        }

        public void Dispose(ResourceLoaderComponent component)
        {
            foreach (var kv in component.AssetCache)
            {
                component.ResourceUtility.Unload(kv.Value);
            }
            
            component.AssetCache.Clear();
            component.AssetCache = null;
            component.LoadingTasks.Clear();
            component.LoadingTasks = null;
        }
    }

    public static class ResourceLoadExtensions
    {
        public static async ValueTask<object> Load(this ResourceLoaderComponent component, string path)
        {
            ThrowIfDisposed(component);

            if (component.AssetCache.TryGetValue(path, out var asset))
            {
                return asset;
            }

            if (component.LoadingTasks.TryGetValue(path, out var loadingTask))
            {
                asset = await loadingTask;
                return component.IsDisposed ? null : asset;
            }

            loadingTask = LoadAndCache(component, path);
            component.LoadingTasks.Add(path, loadingTask);

            try
            {
                asset = await loadingTask;
                return component.IsDisposed ? null : asset;
            }
            finally
            {
                component.LoadingTasks?.Remove(path);
            }
        }

        private static async Task<object> LoadAndCache(ResourceLoaderComponent component, string path)
        {
            var asset = await component.ResourceUtility.Load(path);

            if (component.IsDisposed)
            {
                if (asset != null)
                {
                    component.ResourceUtility.Unload(asset);
                }

                return null;
            }

            component.AssetCache.Add(path, asset);
            return asset;
        }

        private static void ThrowIfDisposed(ResourceLoaderComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (component.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ResourceLoaderComponent));
            }
        }

        public static async Task Unload(this ResourceLoaderComponent component, object asset)
        {
            await Task.Delay(5000);
            component.ResourceUtility.Unload(asset);
        }
    }
}
