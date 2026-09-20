using LYFramework.Event;

namespace Demo.Client.System
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
}