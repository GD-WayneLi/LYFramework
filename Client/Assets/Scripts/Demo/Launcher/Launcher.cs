using LYFramework.Log;
using LYFramework.TaskEx;
using UnityEngine;
using YooAsset;

namespace Demo
{
    public class Launcher : MonoBehaviour
    {
        [SerializeField] EPlayMode m_PlayMode = EPlayMode.EditorSimulateMode;

        void Start()
        {
            LYLogger.SetLogHelper(new DefaultLogHelper());

            // todo:加载loading界面
            
            YooAssetUtility.Init(m_PlayMode, "DefaultPackage").Forget();
        }

        private void OnDestroy()
        {
        }
    }
}