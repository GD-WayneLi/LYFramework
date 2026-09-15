using System;
using Demo.Client.Model;
using LYFramework;
using LYUnity.UI;
using UnityEngine;

namespace Demo.Client.System
{
    public partial class UILoadingSystem : IUIOpen<UILoadingComponent>, IUIUpdate<UILoadingComponent>, IUIClose<UILoadingComponent>
    {
        private const float ProgressSmoothTime = 0.2f;

        public void OnOpen(UILoadingComponent self)
        {
            self.m_CurrentProgress = 0f;
            self.m_TargetProgress = 0f;
            self.m_ProgressVelocity = 0f;

            if (self.m_ProgressSlider != null)
            {
                self.m_ProgressSlider.value = 0f;
            }

            if (self.m_StatusText != null)
            {
                self.m_StatusText.text = string.Empty;
            }

            UnregisterListener(self);
            if (Game.EventManager == null)
            {
                return;
            }

            self.m_EventHandler = (_, loadingEvent) => OnLoadingEvent(self, loadingEvent);
            Game.EventManager.AddListener(self.m_EventHandler);
        }

        public void Update(UILoadingComponent self)
        {
            if (self.m_ProgressSlider == null)
            {
                return;
            }

            SmoothProgress(self);
        }

        public void OnClose(UILoadingComponent self)
        {
            UnregisterListener(self);
        }

        private static void OnLoadingEvent(UILoadingComponent self, UILoadingEvent loadingEvent)
        {
            if (self == null || self.IsDisposed)
            {
                return;
            }

            var targetProgress = Mathf.Clamp01(loadingEvent.Progress);
            if (float.IsNaN(targetProgress))
            {
                targetProgress = 0f;
            }

            self.m_TargetProgress = targetProgress;
            if (self.m_StatusText != null)
            {
                self.m_StatusText.text = loadingEvent.Description ?? string.Empty;
            }
        }

        private static void SmoothProgress(UILoadingComponent self)
        {
            var targetProgress = self.m_TargetProgress;
            var progress = Mathf.SmoothDamp(self.m_ProgressSlider.value, targetProgress, ref self.m_ProgressVelocity, ProgressSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            if (Mathf.Abs(progress - targetProgress) <= 0.001f)
            {
                progress = targetProgress;
                self.m_ProgressVelocity = 0f;
            }

            self.m_CurrentProgress = progress;
            self.m_ProgressSlider.value = progress;
        }

        private static void UnregisterListener(UILoadingComponent self)
        {
            if (self.m_EventHandler == null)
            {
                return;
            }

            Game.EventManager?.RemoveListener(self.m_EventHandler);
            self.m_EventHandler = null;
        }
    }
}
