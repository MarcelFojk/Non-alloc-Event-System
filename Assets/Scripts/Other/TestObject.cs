using EventSystem;
using UnityEngine;

namespace Other
{
    public class TestObject : BaseBehaviour
    {
        [EventListener(typeof(TestEventWithData))]
        private void Method1(ref TestEventData data)
        {
            Debug.Log("Event with data");
        }

        [EventListener(typeof(TestEventNoData))]
        private void Method2()
        {
            Debug.Log("Event without data");
        }

        private void Method3()
        {

        }

        private void Method4()
        {

        }

        private void Method5()
        {

        }

        private void Method6()
        {

        }

        private void Method7()
        {

        }

        private void Method8()
        {

        }

        private void Method9()
        {

        }

        private void Method10()
        {

        }
    }
}