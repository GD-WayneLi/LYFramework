using System;

namespace LYFramework.Event
{
    public interface IEventWrapper
    {
        void Trigger(object sender, object e);
    }
}