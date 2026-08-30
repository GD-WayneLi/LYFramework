using QFramework.Demo;
using UnityEngine;

namespace Demo
{
    public class Launcher : MonoBehaviour
    {
        void Start()
        {
            GameManager.Instance.Init();
        }

        private void OnDestroy()
        {
        }
    }
}