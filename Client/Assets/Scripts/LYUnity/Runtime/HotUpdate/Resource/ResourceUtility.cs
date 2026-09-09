using System.Threading.Tasks;
using LYFramework;
using LYFramework.Log;

namespace LYUnity.Resource
{
    public static class ResourceUtility
    {
        public static async ValueTask<T> Load<T>(this Entity entity, string path, string packageName = "DefaultPackage")  where T : UnityEngine.Object
        {
            if (entity.IsComponent || entity.IsDisposed)
            {
                LYLogger.Error("Entity is not valid.");
                return null;
            }
            
            if (!entity.TryGetComponent<ResourceLoaderComponent>(out var component))
            {
                component = entity.AddComponent<ResourceLoaderComponent, string>(packageName);
            }

            var obj = await component.Load<T>(path);
            return obj;
        }
    }
}