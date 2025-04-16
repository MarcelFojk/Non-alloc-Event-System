using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using System.Linq;

namespace EventSystem
{
    public static class EventListenerResolver
    {
        private static readonly Dictionary<MonoBehaviour, List<(object eventInstance, Delegate delegateInstance)>> _subscribedDelegates
            = new Dictionary<MonoBehaviour, List<(object, Delegate)>>();

        public static void ResolveEventListeners(MonoBehaviour instance)
        {
            var methods = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (!_subscribedDelegates.ContainsKey(instance))
            {
                _subscribedDelegates[instance] = new List<(object, Delegate)>();
            }

            var delegateList = _subscribedDelegates[instance];

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<EventListenerAttribute>();
                if (attribute == null) continue;

                var eventType = attribute.EventType;

                if (eventType == typeof(NonAllocEvent) || eventType.IsSubclassOf(typeof(NonAllocEvent)))
                {
                    var eventField = typeof(EventsManager).GetField(eventType.Name, BindingFlags.Public | BindingFlags.Static);
                    if (eventField == null)
                    {
                        Debug.LogError($"Event {eventType.Name} not found in EventsManager.");
                        continue;
                    }

                    var eventInstance = eventField.GetValue(null) as NonAllocEvent;
                    if (eventInstance == null)
                    {
                        Debug.LogError($"Event {eventType.Name} in EventsManager is not of type NonAllocEvent.");
                        continue;
                    }

                    if (method.GetParameters().Length != 0)
                    {
                        Debug.LogError($"Method {method.Name} in {instance.GetType().Name} for NonAllocEvent must have no parameters.");
                        continue;
                    }

                    var delegateInstance = (Action)Delegate.CreateDelegate(typeof(Action), instance, method);
                    eventInstance.Register(delegateInstance);

                    delegateList.Add((eventInstance, delegateInstance));
                }
                else
                {
                    var baseType = eventType;
                    while (baseType != null && baseType != typeof(object))
                    {
                        if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(NonAllocEvent<>))
                        {
                            break;
                        }
                        baseType = baseType.BaseType;
                    }

                    if (baseType == null || !baseType.IsGenericType)
                    {
                        Debug.LogError($"Method {method.Name} in {instance.GetType().Name} uses EventListener with an invalid event type {eventType.Name}.");
                        continue;
                    }

                    var dataType = baseType.GetGenericArguments()[0];
                    var delegateType = typeof(RefAction<>).MakeGenericType(dataType);

                    var eventField = typeof(EventsManager).GetFields(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(f => f.FieldType == eventType);
                    if (eventField == null)
                    {
                        Debug.LogError($"No event of type {eventType.Name} found in EventsManager.");
                        continue;
                    }

                    var eventInstance = eventField.GetValue(null);
                    if (eventInstance == null || !eventInstance.GetType().IsSubclassOf(typeof(NonAllocEventBase)))
                    {
                        Debug.LogError($"Event {eventType.Name} in EventsManager is not of type NonAllocEvent<T>.");
                        continue;
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length != 1 || !parameters[0].ParameterType.IsByRef || parameters[0].ParameterType.GetElementType() != dataType)
                    {
                        Debug.LogError($"Method {method.Name} in {instance.GetType().Name} must have a single parameter of type 'ref {dataType.Name}'.");
                        continue;
                    }

                    var delegateInstance = Delegate.CreateDelegate(delegateType, instance, method);
                    var registerMethod = eventInstance.GetType().GetMethod("Register");
                    registerMethod.Invoke(eventInstance, new object[] { delegateInstance });

                    delegateList.Add((eventInstance, delegateInstance));
                }
            }
        }

        public static void UnsubscribeEventListeners(MonoBehaviour instance)
        {
            if (!_subscribedDelegates.ContainsKey(instance))
            {
                return;
            }

            var delegateList = _subscribedDelegates[instance];

            foreach (var (eventInstance, delegateInstance) in delegateList)
            {
                if (eventInstance is NonAllocEvent noDataEvent)
                {
                    noDataEvent.Unregister((Action)delegateInstance);
                }
                else
                {
                    var unregisterMethod = eventInstance.GetType().GetMethod("Unregister");
                    unregisterMethod.Invoke(eventInstance, new object[] { delegateInstance });
                }
            }

            _subscribedDelegates.Remove(instance);
        }
    }
}