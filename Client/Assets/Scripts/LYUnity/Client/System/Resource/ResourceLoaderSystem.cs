using System.Threading.Tasks;
using LYUnity.Resource;

namespace LYFramework.Resource
{
    public class ResourceLoaderSystem : SystemBase, ISystemAwake<ResourceLoaderComponent>, ISystemUpdate<ResourceLoaderComponent>, ISystemDispose<ResourceLoaderComponent>
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

            foreach (var obj in component.UnloadAssets)
            {
                component.ResourceUtility.Unload(obj);
            }
            
            component.AssetCache.Clear();
            component.AssetCache = null;
        }
        
        public void Update(ResourceLoaderComponent component)
        {
            
        }
    }

    public static class ResourceLoadExtensions
    {
        public static async ValueTask<object> Load(this ResourceLoaderComponent component, string path)
        {
            if (component.AssetCache.TryGetValue(path, out var asset))
            {
                return asset;
            }
            
            asset = await component.ResourceUtility.Load(path);
            component.AssetCache.Add(path, asset);
            
            return asset;
        }

        public static void Unload(this ResourceLoaderComponent component, object asset)
        {
            
        }
    }
}