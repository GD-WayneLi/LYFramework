using Demo.Client.Model;
using LYFramework;
using LYUnity.UI;
using UnityEngine.UI;
using TMPro;

namespace Demo.Client.System
{
    public partial class UILoginSystem : SystemBase, IUILogicSystem<UILoginComponent>
    {
        public void OnLoaded(UILoginComponent self)
        {
            self.m_GameObject = self.Owner?.GetComponent<UIComponent>()?.GameObject;
            if (self.m_GameObject == null)
            {
                return;
            }
            self.m_AccountInput = self.m_GameObject.transform.Find("Backdrop/Panel/m_AccountInput").GetComponent<TMP_InputField>();
            self.m_LoginButton = self.m_GameObject.transform.Find("Backdrop/Panel/m_LoginButton").GetComponent<Button>();

        }
    }
}
