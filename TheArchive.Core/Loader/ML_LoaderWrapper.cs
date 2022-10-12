#if MelonLoader
using MelonLoader;
#endif
using System;
using System.Linq;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Loader
{
    public static partial class LoaderWrapper
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

        public static bool IsModInstalled(string modName)
        {
            return MelonHandler.Mods.Any(m => m.Info.Name == modName);
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
    }
}
