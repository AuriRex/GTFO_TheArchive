using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace TheArchive.Utilities
{
    public static class MonoUtils
    {

        // https://gamedev.stackexchange.com/a/183962
        public static Bounds GetMaxBounds(GameObject g)
        {
            var renderers = g.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(g.transform.position, Vector3.zero);
            var b = renderers[0].bounds;
            foreach (Renderer r in renderers)
            {
                b.Encapsulate(r.bounds);
            }
            return b;
        }


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
            var eventInfo = typeof(TOn).GetType().GetEvent(eventFieldName, HarmonyLib.AccessTools.all);
            var eventDelegate = (MulticastDelegate) typeof(TOn).GetField(eventFieldName, HarmonyLib.AccessTools.all).GetValue(instance);
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
            var eventFieldInfo = typeof(TOn).GetField(eventFieldName, HarmonyLib.AccessTools.all);

            eventFieldInfo?.SetValue(instance, null);
        }

    }
}
