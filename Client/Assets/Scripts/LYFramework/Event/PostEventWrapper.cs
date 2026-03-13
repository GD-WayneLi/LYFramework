namespace LYFramework.Event
{
    public class PostEventWrapper
    {
        public object Sender;
        public IEvent Event;

        public PostEventWrapper(object sender, IEvent e)
        {
            Sender = sender;
            Event = e;
        }
    }
}