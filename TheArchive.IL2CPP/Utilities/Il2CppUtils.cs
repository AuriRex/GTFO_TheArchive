using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;
using IL2Tasks = Il2CppSystem.Threading.Tasks;

namespace TheArchive.Utilities
{
    public static class Il2CppUtils
    {
        public static List<T> ToSystemList<T>(this Il2CppSystem.Collections.Generic.List<T> il2List)
        {
            var list = new List<T>();

            foreach(var item in il2List)
            {
                list.Add(item);
            }

            return list;
        }

        public static IntPtr GetFieldPointer<T>(string fieldName)
        {
            return (IntPtr) typeof(T).GetField($"NativeFieldInfoPtr_{fieldName}", HarmonyLib.AccessTools.all).GetValue(null);
        }

        public unsafe static void SetFieldUnsafe<T, TValue>(T instance, TValue value, string fieldName) where T : UnhollowerBaseLib.Il2CppObjectBase where TValue : UnhollowerBaseLib.Il2CppObjectBase
        {
            IntPtr fieldOffset = GetFieldPointer<T>(fieldName);
            System.Runtime.CompilerServices.Unsafe.CopyBlock((void*) ((long) UnhollowerBaseLib.IL2CPP.Il2CppObjectBaseToPtrNotNull(instance) + (long) (int) UnhollowerBaseLib.IL2CPP.il2cpp_field_get_offset(fieldOffset)), (void*) UnhollowerBaseLib.IL2CPP.il2cpp_object_unbox(UnhollowerBaseLib.IL2CPP.Il2CppObjectBaseToPtr(value)), (uint) UnhollowerBaseLib.IL2CPP.il2cpp_class_value_size(Il2CppClassPointerStore<TValue>.NativeClassPtr, ref *(uint*) null));
        }

        public static IEnumerator DoAfter(float time, Action action)
        {
            float start = Time.fixedTime;
            while (start + time > Time.fixedTime)
                yield return null;
            action?.Invoke();
            yield break;
        }

        public const BindingFlags AnyBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static void SetPropertyValue<T, V>(string propertyName, V value, object host = null)
        {
            typeof(T).GetProperty(propertyName, AnyBindingFlags).SetValue(host, value);
        }

        public static void CallEvent<T>(string eventPropertyName)
        {
            //var eventInfo = typeof(PlayFabManager).GetType().GetEvent(eventPropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var eventDelegate = (Il2CppSystem.MulticastDelegate) typeof(T).GetProperty(eventPropertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
            if (eventDelegate != null)
            {
                foreach (var handler in eventDelegate.GetInvocationList())
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"event {typeof(T)}.{eventPropertyName} calling: {handler.Method.DeclaringType.Name}.{handler.Method.Name}()");
                    handler.Method.Invoke(handler.Target, null);
                }
            }
        }

        public static IL2Tasks.Task<T> NullTask<T>() where T : class
        {
            return IL2Tasks.Task.FromResult<T>(null);
        }

        public static T PickRandom<T>(this T[] array)
        {
            return (T) array.GetValue(UnityEngine.Random.RandomRangeInt(0, array.Length - 1));
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
            for(int i = 0; i < trans.childCount; i++)
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

            if(component != null)
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
