using UnityEngine;

namespace LYFramework.Log
{
    public class DefaultLogHelper : ILogHelper
    {
        public void Log(string message)
        {
            Debug.Log(message);
        }

        public void Info(string message)
        {
            Debug.Log($"[Info] {message}");
        }

        public void Warning(string message)
        {
            Debug.LogWarning(message);
        }

        public void Error(string message)
        {
            Debug.LogError(message);
        }
    }
}