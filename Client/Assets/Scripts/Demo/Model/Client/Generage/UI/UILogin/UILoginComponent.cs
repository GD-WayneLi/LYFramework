using LYFramework;
using LYUnity.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Demo.Client.Model
{
    public partial class UILoginComponent : Entity, IUILogicComponent
    {
        public GameObject GameObject => m_GameObject;
        
        public GameObject m_GameObject;
        
        public TMP_InputField m_AccountInput;
        public Button m_LoginButton;

    }
}
