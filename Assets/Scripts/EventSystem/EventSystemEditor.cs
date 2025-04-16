#if UNITY_EDITOR
using System.Reflection;
using System;
using UnityEditor;
using System.Linq;
using UnityEngine;

namespace EventSystem.Editor
{
    public static class EventListenerValidator
    {
        [InitializeOnLoadMethod]
        private static void ValidateEventListeners()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.GetName().Name.StartsWith("Unity") && !a.GetName().Name.StartsWith("System"));

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        var attribute = method.GetCustomAttribute<EventListenerAttribute>();
                        if (attribute == null) continue;

                        ValidateMethodSignature(method, attribute);
                    }
                }
            }
        }

        private static void ValidateMethodSignature(MethodInfo method, EventListenerAttribute attribute)
        {
            var eventType = attribute.EventType;

            if (eventType == typeof(NonAllocEvent) || eventType.IsSubclassOf(typeof(NonAllocEvent)))
            {
                if (method.GetParameters().Length != 0)
                {
                    Debug.LogError($"Method {method.Name} in {method.DeclaringType.Name} for NonAllocEvent must have no parameters.");
                    return;
                }

                if (method.ReturnType != typeof(void))
                {
                    Debug.LogError($"Method {method.Name} in {method.DeclaringType.Name} must return void for EventListener.");
                    return;
                }

                Debug.Log($"Method {method.Name} in {method.DeclaringType.Name} passed EventListener validation (no data).");
                return;
            }

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
                Debug.LogError($"Method {method.Name} in {method.DeclaringType.Name} uses EventListener with an invalid event type {eventType.Name}. It must inherit from NonAllocEvent<T> or be NonAllocEvent.");
                return;
            }

            var dataType = baseType.GetGenericArguments()[0];

            var parameters = method.GetParameters();
            if (parameters.Length != 1)
            {
                Debug.LogError($"Method {method.Name} in {method.DeclaringType.Name} must have exactly one parameter for EventListener with data.");
                return;
            }

            var param = parameters[0];
            if (!param.ParameterType.IsByRef || param.ParameterType.GetElementType() != dataType)
            {
                Debug.LogError($"Method {method.Name} in {method.DeclaringType.Name} must have a single parameter of type 'ref {dataType.Name}'. Found: {param.ParameterType.Name}");
                return;
            }

            if (method.ReturnType != typeof(void))
            {
                Debug.LogError($"Method {method.Name} in {method.DeclaringType.Name} must return void for EventListener.");
                return;
            }

            Debug.Log($"Method {method.Name} in {method.DeclaringType.Name} passed EventListener validation (with data).");
        }
    }
}
#endif