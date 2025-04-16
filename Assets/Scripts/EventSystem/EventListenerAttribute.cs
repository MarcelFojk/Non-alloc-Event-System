using System;

namespace EventSystem
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EventListenerAttribute : Attribute
    {
        public Type EventType { get; }

        public EventListenerAttribute(Type eventType)
        {
            if (!eventType.IsSubclassOf(typeof(NonAllocEventBase)) && !eventType.IsSubclassOf(typeof(NonAllocEvent)))
            {
                throw new ArgumentException($"Event type {eventType.Name} must inherit from NonAllocEventBase or be NonAllocEvent.");
            }
            EventType = eventType;
        }
    }
}