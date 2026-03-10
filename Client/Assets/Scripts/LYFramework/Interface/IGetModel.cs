namespace LYFramework
{
    public interface IGetModel : IGetGameManager
    {
    }
    
    public static class GetModelExtension
    {
        public static T GetModel<T>(this IGetModel self) where T : class, IModel
        {
            return self.GetGameManager().GetModel<T>();
        }
    }
}