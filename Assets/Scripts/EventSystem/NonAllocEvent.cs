using System;

namespace EventSystem
{
    public interface IConsumableData
    {
        bool Consumed { get; set; }
    }

    public abstract class NonAllocEventBase { }
    public delegate void RefAction<T>(ref T data);

    public abstract class NonAllocEvent<T> : NonAllocEventBase where T : struct, IConsumableData
    {
        private readonly RefAction<T>[] listeners;
        private int listenerCount = 0;

        public NonAllocEvent(int capacity)
        {
            listeners = new RefAction<T>[capacity];
        }

        public void Register(RefAction<T> listener)
        {
            if (listenerCount < listeners.Length && !IsRegistered(listener))
            {
                listeners[listenerCount++] = listener;
            }
        }

        public void Unregister(RefAction<T> listener)
        {
            for (int i = 0; i < listenerCount; i++)
            {
                if (listeners[i] == listener)
                {
                    for (int j = i; j < listenerCount - 1; j++)
                    {
                        listeners[j] = listeners[j + 1];
                    }
                    listeners[listenerCount - 1] = null;
                    listenerCount--;
                    break;
                }
            }
        }

        public void Invoke(T data)
        {
            for (int i = listenerCount - 1; i >= 0; i--)
            {
                listeners[i]?.Invoke(ref data);
                if (data.Consumed == true)
                    break;
            }
        }

        private bool IsRegistered(RefAction<T> listener)
        {
            for (int i = 0; i < listenerCount; i++)
            {
                if (listeners[i] == listener) return true;
            }
            return false;
        }
    }

    public abstract class NonAllocEvent
    {
        private readonly Action[] listeners;
        private int listenerCount = 0;

        public NonAllocEvent(int capacity)
        {
            listeners = new Action[capacity];
        }

        public void Register(Action listener)
        {
            if (listenerCount < listeners.Length && !IsRegistered(listener))
            {
                listeners[listenerCount++] = listener;
            }
        }

        public void Unregister(Action listener)
        {
            for (int i = 0; i < listenerCount; i++)
            {
                if (listeners[i] == listener)
                {
                    listeners[i] = listeners[listenerCount - 1];
                    listeners[listenerCount - 1] = null;
                    listenerCount--;
                    break;
                }
            }
        }

        public void Invoke()
        {
            for (int i = 0; i < listenerCount; i++)
            {
                listeners[i]?.Invoke();
            }
        }

        private bool IsRegistered(Action listener)
        {
            for (int i = 0; i < listenerCount; i++)
            {
                if (listeners[i] == listener) return true;
            }
            return false;
        }
    }
}