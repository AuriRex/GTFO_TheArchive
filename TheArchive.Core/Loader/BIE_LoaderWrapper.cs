using System;
using System.Linq;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Loader
{
    public static partial class LoaderWrapper
    {
#if BepInEx
        public static string GameDirectory => BepInEx.Paths.GameRootPath;

        public static string UserDataDirectory => System.IO.Path.Combine(GameDirectory, "UserData");

        public static bool IsGameIL2CPP() => true;

        public static IArchiveLogger CreateLoggerInstance(string name, ConsoleColor col = ConsoleColor.White)
        {
            return new BIE_LogWrapper(new BepInEx.Logging.ManualLogSource(name));
        }

        public static IArchiveLogger CreateArSubLoggerInstance(string name, ConsoleColor col = ConsoleColor.White) => CreateLoggerInstance($"{ArchiveMod.ABBREVIATION}::{name}", col);

        public static IArchiveLogger WrapLogger(BepInEx.Logging.ManualLogSource loggerInstance)
        {
            return new BIE_LogWrapper(loggerInstance);
        }

        public static object StartCoroutine(System.Collections.IEnumerator routine)
        {
            return BepInEx.Unity.IL2CPP.Utils.MonoBehaviourExtensions.StartCoroutine(BIE_ArchiveMod.MainComponent, routine);
        }

        public static void StopCoroutine(object coroutineToken)
        {
            BIE_ArchiveMod.MainComponent.StopCoroutine((UnityEngine.Coroutine)coroutineToken);
        }

        public static void NativeHookAttach(IntPtr target, IntPtr detour)
        {
            ArchiveLogger.Warning("NativeHookAttach not implemented");
        }

        public static bool IsModInstalled(string guid)
        {
            BepInEx.Unity.IL2CPP.IL2CPPChainloader.Instance.Plugins.TryGetValue(guid, out var plugin);

            return plugin != null;
        }

        public static class ClassInjector
        {
            public static IntPtr DerivedConstructorPointer<T>()
            {
                return Il2CppInterop.Runtime.Injection.ClassInjector.DerivedConstructorPointer<T>();
            }

            public static void DerivedConstructorBody(Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase objectBase)
            {
                Il2CppInterop.Runtime.Injection.ClassInjector.DerivedConstructorBody(objectBase);
            }

            public static void RegisterTypeInIl2Cpp<T>(bool logSuccess = false) where T : class
            {
                Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<T>(new Il2CppInterop.Runtime.Injection.RegisterTypeOptions
                {
                    LogSuccess = logSuccess
                });
            }

            public static void RegisterTypeInIl2CppWithInterfaces<T>(bool logSuccess = false, params Type[] interfaces) where T : class
            {
                Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<T>(new Il2CppInterop.Runtime.Injection.RegisterTypeOptions
                {
                    Interfaces = interfaces,
                    LogSuccess = logSuccess
                });
            }
        }
#endif
    }
}
