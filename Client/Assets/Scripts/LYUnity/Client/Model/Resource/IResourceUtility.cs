using System.Threading.Tasks;

namespace LYFramework.Resource
{
    public interface IResourceUtility : IUtility
    {
        ValueTask<object> Load(string path);
        void Unload(object obj);
    }
}