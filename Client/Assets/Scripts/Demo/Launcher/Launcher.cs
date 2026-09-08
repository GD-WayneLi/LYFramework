using QFramework.Demo;
using UnityEngine;
using YooAsset;

namespace Demo
{
    public class Launcher : MonoBehaviour
    {
        [SerializeField] EPlayMode m_PlayMode = EPlayMode.EditorSimulateMode;

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            GameManager.Instance.Init(gameObject, m_PlayMode);
        }

        private void Update()
        {
            GameManager.Instance.Update();
        }

        private void OnDestroy()
        {
            GameManager.Instance.Dispose();
        }
    }
}