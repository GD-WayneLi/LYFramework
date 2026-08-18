using System.Collections.Generic;
using System.Threading.Tasks;
using LYFramework;
using LYFramework.Resource;

namespace LYUnity.Resource
{
    public class ResourceLoaderComponent : Entity
    {
        public IResourceUtility ResourceUtility;
        public Dictionary<string, object> AssetCache = new();
        public Dictionary<string, Task<object>> LoadingTasks = new();
    }
}
