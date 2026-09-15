using System;
using LYFramework.Event;

namespace Demo.Client.Model
{
    public readonly struct UILoadingEvent : IEvent
    {
        public float Progress { get; }
        public string Description { get; }

        public UILoadingEvent(float progress, string description)
        {
            Progress = progress;
            Description = description;
        }
    }

    public partial class UILoadingComponent
    {
        public float m_CurrentProgress;
        public float m_TargetProgress;
        public float m_ProgressVelocity;
        public EventHandler<UILoadingEvent> m_EventHandler;
    }
}
