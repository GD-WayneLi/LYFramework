namespace LYFramework
{
    public interface IGetUtility : IGetGameManager
    {
    }

    public static class GetUtilityExtension
    {
        public static T GetUtility<T>(this IGetUtility self) where T : class, IUtility
        {
            return self.GetGameManager().GetUtility<T>();
        }
    }
}