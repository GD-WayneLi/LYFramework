using System.Collections.Generic;
using LYFramework;
using YooAsset;

namespace LYUnity.Resource
{
    public class ResourceLoaderComponent : Entity
    {
        public ResourcePackage Package;
        public Dictionary<string, AssetHandle> AssetCache = new();
    }
}