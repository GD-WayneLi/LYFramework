using System.Collections.Generic;
using LYFramework;
using LYFramework.Resource;

namespace LYUnity.Resource
{
    public class ResourceLoaderComponent : Entity
    {
        public IResourceUtility ResourceUtility;
        public Dictionary<string, object> AssetCache = new();
        public List<object> UnloadAssets = new();
    }
}