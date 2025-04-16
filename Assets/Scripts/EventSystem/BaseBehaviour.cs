using UnityEngine;

namespace EventSystem
{
    public abstract class BaseBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            this.ResolveEventListeners();
        }

        protected virtual void OnDestroy()
        {
            this.UnsubscribeEventListeners();
        }

        public void ResolveEventListeners()
        {
            EventListenerResolver.ResolveEventListeners(this);
        }

        public void UnsubscribeEventListeners()
        {
            EventListenerResolver.UnsubscribeEventListeners(this);
        }
    }
}