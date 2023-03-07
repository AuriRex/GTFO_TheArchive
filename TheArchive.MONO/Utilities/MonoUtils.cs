using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace TheArchive.Utilities
{
    public static class MonoUtils
    {

        public static IEnumerator DoAfter(float time, Action action)
        {
            float start = Time.fixedTime;
            while (start + time > Time.fixedTime)
                yield return null;
            action?.Invoke();
            yield break;
        }

        public static void CallEvent<TOn>(string eventFieldName, object instance = null, params object[] parameters)
        {
            var eventInfo = typeof(TOn).GetType().GetEvent(eventFieldName, Utils.AnyBindingFlagss);
            var eventDelegate = (MulticastDelegate) typeof(TOn).GetField(eventFieldName, Utils.AnyBindingFlagss).GetValue(instance);
            if (eventDelegate != null)
            {
                foreach (var handler in eventDelegate.GetInvocationList())
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"event {typeof(TOn)}.{eventFieldName}() calling: {handler.Method.DeclaringType.Name}.{handler.Method.Name}()");
                    handler.Method.Invoke(handler.Target, parameters);
                }
            }
        }

        public static void RemoveAllEventHandlers<TOn>(string eventFieldName, object instance = null)
        {
            var eventFieldInfo = typeof(TOn).GetField(eventFieldName, Utils.AnyBindingFlagss);

            eventFieldInfo?.SetValue(instance, null);
        }

    }
}
