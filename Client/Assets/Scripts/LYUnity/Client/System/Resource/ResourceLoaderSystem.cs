using System;
using System.Threading.Tasks;
using LYFramework;
using LYFramework.TaskEx;
using YooAsset;

namespace LYUnity.Resource
{
    public class ResourceLoaderSystem : SystemBase, ISystemAwake<ResourceLoaderComponent>, ISystemDispose<ResourceLoaderComponent>
    {
        public void Awake(ResourceLoaderComponent component)
        {
            component.Package = YooAssets.GetPackage("");
        }

        public void Dispose(ResourceLoaderComponent component)
        {
            foreach (var kv in component.AssetCache)
            {
                component.Unload(kv.Value);
            }
            
            component.AssetCache.Clear();
            component.AssetCache = null;
        }
    }

    public static class ResourceLoadExtensions
    {
        public static async ValueTask<T> Load<T>(this ResourceLoaderComponent component, string path) where T :  UnityEngine.Object
        {
            ThrowIfDisposed(component);

            if (!component.AssetCache.TryGetValue(path, out var handler))
            {
                handler = component.Package.LoadAssetAsync<T>(path);
                component.AssetCache.Add(path, handler);
            }

            await handler;
            return handler.GetAssetObject<T>();
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

        public static void Unload(this ResourceLoaderComponent component, AssetHandle asset)
        {
            if (!asset.IsValid)
            {
                return;
            }
            UnloadInternal(asset).Forget();
        }
        
        static async Task UnloadInternal(AssetHandle asset)
        {
            await Task.Delay(5000);
            asset.Release();
        }
    }
}
