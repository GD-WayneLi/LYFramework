using System.Threading.Tasks;
using Demo.Client.Model;
using Demo.Model.Client;
using LYFramework;
using LYFramework.TaskEx;
using LYUnity.UI;
using UnityEngine;

namespace QFramework.Demo
{
    public partial class GameManager : GameManagerBase<GameManager>
    {
        GameObject m_Root;
        
        public void Init(GameObject root)
        {
            m_Root = root;

            Init();
        }
        
        public override void Init()
        {
            InitAsync().Forget();
        }

        async ValueTask InitAsync()
        {
            var domain = Game.World.AddDomain();
            
            // 初始化UI组件
            var uiManagerEntity = domain.AddChild();
            var uiManager = uiManagerEntity.AddComponent<UIManagerComponent, GameObject>(m_Root);
        }
        
    }
}