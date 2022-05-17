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

        /// <summary>
        /// Run <paramref name="func"/> on every first child of <paramref name="trans"/>
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="func"></param>
        public static void ForEachFirstChildDo(this Transform trans, Action<Transform> func) => trans.ForEachChildDo(func, recursive: false);

        /// <summary>
        /// Run <paramref name="func"/> on every child of <paramref name="trans"/>
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="func"></param>
        /// <param name="recursive">If the children of every child should be included</param>
        public static void ForEachChildDo(this Transform trans, Action<Transform> func, bool recursive = true)
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                var child = trans.GetChild(i);
                func?.Invoke(child);
                if (recursive)
                {
                    ForEachChildDo(child, func, recursive);
                }
            }
        }

        public static Transform GetChildWithExactName(this Transform trans, string name)
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                var child = trans.GetChild(i);
                if (child.name == name) return child;
            }
            return null;
        }

        public static T GetComponentOnSelfOrParents<T>(this Transform trans) where T : Component
        {
            if (trans == null) return null;

            T component = trans.GetComponent<T>();

            if (component != null)
            {
                return component;
            }

            return trans?.GetComponentOnParents<T>();
        }

        public static T GetComponentOnParents<T>(this Transform trans) where T : Component
        {
            if (trans == null) return null;

            var parent = trans.parent;

            T component = parent.GetComponent<T>();

            if (component != null)
            {
                return component;
            }

            return parent.parent?.GetComponentOnParents<T>();
        }
    }
}
