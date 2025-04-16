
namespace EventSystem
{
    public struct TestEventData : IConsumableData
    {
        public TestEventData(int count, float value)
        {
            Count = count;
            Value = value;
            Consumed = false;
        }

        public int Count { get; }
        public float Value { get; }
        public bool Consumed { get; set; }
    }

    public class TestEventWithData : NonAllocEvent<TestEventData>
    {
        public TestEventWithData(int capacity) : base(capacity) { }
    }

    public class TestEventNoData : NonAllocEvent
    {
        public TestEventNoData(int capacity) : base(capacity) { }
    }
}