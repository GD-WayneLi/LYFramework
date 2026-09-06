using System.Threading.Tasks;
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
            
            Init().Forget();
        }

        async ValueTask Init()
        {
            YooAssetUtility.Init();
            await YooAssetUtility.LoadBootPage(m_PlayMode, "Boot");
            YooAssetUtility.Update(m_PlayMode, "DefaultPackage").Forget();
        }
        
        private void OnDestroy()
        {
        }
    }
}