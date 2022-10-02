#if MelonLoader
using MelonLoader;
#endif
using System;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Loader
{
    public static class LoaderWrapper
    {
#if MelonLoader
        public static string GameDirectory => MelonUtils.GameDirectory;

        public static string UserDataDirectory => MelonUtils.UserDataDirectory;

        public static bool IsGameIL2CPP() => MelonUtils.IsGameIl2Cpp();

        public static IArchiveLogger CreateLoggerInstance(string name, ConsoleColor col = ConsoleColor.White)
        {
            return new ML_LogWrapper(new MelonLogger.Instance(name, col));
        }

        public static IArchiveLogger CreateArSubLoggerInstance(string name, ConsoleColor col = ConsoleColor.White) => CreateLoggerInstance($"{ArchiveMod.ABBREVIATION}::{name}", col);

        public static IArchiveLogger WrapLogger(MelonLogger.Instance loggerInstance)
        {
            return new ML_LogWrapper(loggerInstance);
        }

        public static object StartCoroutine(System.Collections.IEnumerator routine)
        {
            return MelonCoroutines.Start(routine);
        }

        public static void StopCoroutine(object coroutineToken)
        {
            MelonCoroutines.Stop(coroutineToken);
        }

        public static void NativeHookAttach(IntPtr target, IntPtr detour)
        {
            MelonUtils.NativeHookAttach(target, detour);
        }

        public static class ClassInjector
        {
            public static IntPtr DerivedConstructorPointer<T>()
            {
                return UnhollowerRuntimeLib.ClassInjector.DerivedConstructorPointer<T>();
            }

            public static void DerivedConstructorBody(UnhollowerBaseLib.Il2CppObjectBase objectBase)
            {
                UnhollowerRuntimeLib.ClassInjector.DerivedConstructorBody(objectBase);
            }

            public static void RegisterTypeInIl2Cpp<T>(bool logSuccess = false) where T : class
            {
                UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<T>(logSuccess);
            }

            public static void RegisterTypeInIl2CppWithInterfaces<T>(bool logSuccess = false, params Type[] interfaces) where T : class
            {
                UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2CppWithInterfaces<T>(logSuccess, interfaces);
            }
        }
#endif
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
            throw new NotImplementedException();
        }

        public static void StopCoroutine(object coroutineToken)
        {
            throw new NotImplementedException();
        }

        public static void NativeHookAttach(IntPtr target, IntPtr detour)
        {
            ArchiveLogger.Warning("NativeHookAttach not implemented");
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
