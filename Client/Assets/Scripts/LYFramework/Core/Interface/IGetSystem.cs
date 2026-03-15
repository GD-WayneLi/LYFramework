namespace LYFramework
{
    public interface IGetSystem : IGetGameManager
    {
    }

    public static class GetSystemExtension
    {
        public static T GetSystem<T>(this IGetSystem self) where T : class, ISystem
        {
            return self.GetGameManager().GetSystem<T>();
        }
    }
}