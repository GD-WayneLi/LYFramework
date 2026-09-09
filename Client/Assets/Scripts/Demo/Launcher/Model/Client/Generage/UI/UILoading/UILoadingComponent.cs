using LYFramework;
using LYUnity.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Demo.Client.Model
{
    public partial class UILoadingComponent : Entity, IUILogicComponent
    {
        public GameObject GameObject => m_GameObject;
        
        public GameObject m_GameObject;
        
        public TextMeshProUGUI m_TitleText;
        public TextMeshProUGUI m_StatusText;
        public Slider m_ProgressSlider;

    }
}
