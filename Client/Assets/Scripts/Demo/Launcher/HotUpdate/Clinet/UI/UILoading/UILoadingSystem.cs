using System;
using Demo.Client.Model;
using LYFramework;
using LYUnity.UI;
using UnityEngine;

namespace Demo.Client.System
{
    public partial class UILoadingSystem : IUIOpen<UILoadingComponent>, IUIUpdate<UILoadingComponent>
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

            if (Game.EventManager == null)
            {
                return;
            }

            Game.EventManager.AddListener<UILoadingComponent, UILoadingEvent>(self, OnLoadingEvent);
        }

        public void Update(UILoadingComponent self)
        {
            if (self.m_ProgressSlider == null)
            {
                return;
            }

            SmoothProgress(self);
        }

        private static void OnLoadingEvent(UILoadingComponent self, object sender, UILoadingEvent loadingEvent)
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
    }
}
