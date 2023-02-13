using MelonLoader;
using System;
using TheArchive.Interfaces;
using TheArchive.Interfaces.Loader;
using UnityEngine;

namespace TheArchive.Loader
{
    public static partial class LoaderWrapper
    {
        public static ILoaderWrapper LoaderWrapperInstance { private get; set; }

        public static bool IsIL2CPPType(Type type)
        {
            if (!LoaderWrapperInstance.IsGameIL2CPP())
                return false;

            return ArchiveMod.IL2CPP_BaseType.IsAssignableFrom(type);
        }

        public static string GameDirectory => LoaderWrapperInstance.GameDirectory;

        public static string UserDataDirectory => LoaderWrapperInstance.UserDataDirectory;

        public static string BaseInstallPath => LoaderWrapperInstance.BaseInstallPath;

        public static bool IsGameIL2CPP() => LoaderWrapperInstance.IsGameIL2CPP();

        public static IArchiveLogger CreateLoggerInstance(string name, ConsoleColor col = ConsoleColor.White)
        {
            return LoaderWrapperInstance.CreateLoggerInstance(name, col);
        }

        public static IArchiveLogger CreateArSubLoggerInstance(string name, ConsoleColor col = ConsoleColor.White) => CreateLoggerInstance($"{ArchiveMod.ABBREVIATION}::{name}", col);

        public static IArchiveLogger WrapLogger(object loggerInstance)
        {
            return LoaderWrapperInstance.WrapLogger(loggerInstance);
        }

        public static Coroutine StartCoroutine(System.Collections.IEnumerator routine)
        {
            return LoaderWrapperInstance.StartCoroutine(routine);
        }

        public static void StopCoroutine(Coroutine coroutineToken)
        {
            LoaderWrapperInstance.StopCoroutine(coroutineToken);
        }

        [Obsolete]
        public static void NativeHookAttach(IntPtr target, IntPtr detour)
        {
            LoaderWrapperInstance.NativeHookAttach(target, detour);
        }

        public static bool IsModInstalled(string modName)
        {
            return LoaderWrapperInstance.IsModInstalled(modName);
        }

        public static class ClassInjector
        {
            public static IClassInjector ClassInjectorInstance { private get; set; }


            public static IntPtr DerivedConstructorPointer<T>()
            {
                return ClassInjectorInstance.DerivedConstructorPointer<T>();
            }

#if IL2CPP
            public static void DerivedConstructorBody(Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase objectBase)
            {
                ClassInjectorInstance.DerivedConstructorBody(objectBase);
            }
#endif
#if MONO
            public static void DerivedConstructorBody(object objectBase)
            {
                ClassInjectorInstance.DerivedConstructorBody(objectBase);
            }
#endif

            public static void RegisterTypeInIl2Cpp<T>(bool logSuccess = false) where T : class
            {
                ClassInjectorInstance.RegisterTypeInIl2Cpp<T>(logSuccess);
            }

            public static void RegisterTypeInIl2CppWithInterfaces<T>(bool logSuccess = false, params Type[] interfaces) where T : class
            {
                ClassInjectorInstance.RegisterTypeInIl2CppWithInterfaces<T>(logSuccess, interfaces);
            }
        }
    }
}
