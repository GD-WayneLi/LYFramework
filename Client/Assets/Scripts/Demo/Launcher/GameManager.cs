using System.Threading.Tasks;
using Demo;
using LYFramework;
using LYFramework.Log;
using LYFramework.TaskEx;
using LYUnity.Resource;
using LYUnity.UI;
using UnityEngine;
using YooAsset;

namespace QFramework.Demo
{
    public class GameManager : GameManagerBase<GameManager>
    {
        EPlayMode m_PlayMode;
        GameObject m_Root;
        
        public void Init(GameObject root, EPlayMode playMode)
        {
            LYLogger.SetLogHelper(new DefaultLogHelper());

            m_Root = root;
            m_PlayMode = playMode;

            Init();
        }
        
        public override void Init()
        {
            RegisterSystems();
            InitAsync().Forget();
        }
        
        async ValueTask InitAsync()
        {
            var domain = Game.World.AddDomain();
            
            // 初始化UI组件
            var uiManagerEntity = domain.AddChild();
            var uiManager = uiManagerEntity.AddComponent<UIManagerComponent, GameObject>(m_Root);
            
            YooAssetUtility.Init();
            await YooAssetUtility.LoadBootPage(m_PlayMode, "Boot");
            
            
            
            await YooAssetUtility.Update(m_PlayMode, "DefaultPackage");
        }

        /// <summary>
        /// 注册框架system(不可热更)
        /// </summary>
        private void RegisterSystems()
        {
            RegisterSystem<ResourceLoaderSystem>();
            RegisterSystem<UIManagerSystem>();
        }
    }
}