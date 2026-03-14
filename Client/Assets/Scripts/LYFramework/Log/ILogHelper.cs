namespace LYFramework.Log
{
    public interface ILogHelper
    {
        void Log(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
    }
}