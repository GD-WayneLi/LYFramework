using Demo.Client.System.UI;
using LYFramework;
using LYUnity.UI;
using UnityEngine;

namespace QFramework.Demo
{
    public class GameManager : GameManagerBase<GameManager>
    {
        GameObject m_Root;
        
        public void Init(GameObject root)
        {
            m_Root = root;
        }
        
        public override void Init()
        {
            var domain = Game.World.AddDomain();
            
            // 初始化UI组件
            var uiManagerEntity = domain.AddChild();
            var uiManager = uiManagerEntity.AddComponent<UIManagerComponent, GameObject>(m_Root);
            uiManager.RegisterLayerGroup(new LayerGroup());
        }
    }
}