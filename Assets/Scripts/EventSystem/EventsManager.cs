
namespace EventSystem
{
    public static class EventsManager
    {
        public static TestEventWithData TestEvent = new TestEventWithData(10000);
        public static TestEventNoData TestEventNoData = new TestEventNoData(10000);
    }
}