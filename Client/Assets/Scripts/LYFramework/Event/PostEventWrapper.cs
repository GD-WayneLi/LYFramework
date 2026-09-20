namespace LYFramework.Event
{
    public class PostEventWrapper
    {
        public object Sender;
        public IEvent Event;
        public System.Type EventType;

        public PostEventWrapper(object sender, IEvent e, System.Type eventType)
        {
            Sender = sender;
            Event = e;
            EventType = eventType;
        }
    }
}
