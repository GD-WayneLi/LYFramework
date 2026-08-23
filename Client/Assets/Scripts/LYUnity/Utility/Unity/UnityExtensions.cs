using LYFramework.Log;
using UnityEngine;

namespace LYUnity.Utility.Unity
{
    public static class UnityExtensions
    {
        public static void SetActiveEx(this GameObject go, bool active)
        {
            if (!go)
                return;

            if (go.activeSelf == active)
                return;
            
            go.SetActive(active);
        }

        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (!go)
            {
                LYLogger.Error("GameObject is null");
                return null;
            }
            
            if (!go.TryGetComponent(out T component))
            {
                component = go.AddComponent<T>();
            }
            
            return component;
        }
    }
}