namespace LYFramework.Resource
{
    public interface IResourceUtility : IUtility
    {
        void Load(string path, System.Action<object> callback);
        void Unload(object obj);
    }
}