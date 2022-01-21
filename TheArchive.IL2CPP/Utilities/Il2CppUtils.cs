using MelonLoader;
using System;
using System.Collections;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;
using IL2Tasks = Il2CppSystem.Threading.Tasks;

namespace TheArchive.Utilities
{
    public class Il2CppUtils
    {
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

    }
}
