using Demo.Client.Model;
using LYFramework;
using LYUnity.UI;
using UnityEngine.UI;
using TMPro;

namespace Demo.Client.System
{
    public partial class UILoadingSystem : SystemBase, IUILogicSystem<UILoadingComponent>
    {
        public void OnLoaded(UILoadingComponent self)
        {
            self.m_GameObject = self.Owner?.GetComponent<UIComponent>()?.GameObject;
            if (self.m_GameObject == null)
            {
                return;
            }
            self.m_TitleText = self.m_GameObject.transform.Find("Panel/m_TitleText").GetComponent<TextMeshProUGUI>();
            self.m_StatusText = self.m_GameObject.transform.Find("Panel/m_StatusText").GetComponent<TextMeshProUGUI>();
            self.m_ProgressSlider = self.m_GameObject.transform.Find("Panel/m_ProgressSlider").GetComponent<Slider>();

        }
    }
}
