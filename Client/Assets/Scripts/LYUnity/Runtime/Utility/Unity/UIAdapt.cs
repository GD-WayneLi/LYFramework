using UnityEngine;

namespace LYUnity.Utility.Unity
{
    public class UIAdapt : MonoBehaviour
    {
        [SerializeField] private RectTransform m_CanvasTrans;
        [SerializeField] private RectTransform m_SafeAreaTrans;
        [SerializeField] private Camera m_UICamera;

        private void Start()
        {
            if (m_UICamera != null && m_CanvasTrans != null)
                m_UICamera.orthographicSize = m_CanvasTrans.rect.height / 2;

            ApplySafeArea();
        }
        
        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;

            if (m_SafeAreaTrans == null || Screen.width <= 0 || Screen.height <= 0)
                return;

            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            m_SafeAreaTrans.anchorMin = safeArea.position / screenSize;
            m_SafeAreaTrans.anchorMax = (safeArea.position + safeArea.size) / screenSize;
            m_SafeAreaTrans.offsetMin = Vector2.zero;
            m_SafeAreaTrans.offsetMax = Vector2.zero;
        }
    }
}
