using System;
using System.Linq;
using System.Runtime.InteropServices;
using TheArchive.Interfaces;

namespace TheArchive.Loader;

public static partial class LoaderWrapper
{
#if BepInEx
    public static string GameDirectory => BepInEx.Paths.GameRootPath;

    public static string UserDataDirectory => System.IO.Path.Combine(GameDirectory, "UserData");

    public static bool IsGameIL2CPP() => true;

    public static IArchiveLogger CreateLoggerInstance(string name, ConsoleColor col = ConsoleColor.White)
    {
        return new BIE_LogWrapper(new ArManualLogSource(name, col));
    }

    public static IArchiveLogger CreateArSubLoggerInstance(string name, ConsoleColor col = ConsoleColor.White) => CreateLoggerInstance($"{ArchiveMod.ABBREVIATION}::{name}", col);

    public static IArchiveLogger WrapLogger(BepInEx.Logging.ManualLogSource loggerInstance, ConsoleColor? col = null)
    {
        return new BIE_LogWrapper(new ArManualLogSource(loggerInstance.SourceName, col));
    }

    public static object StartCoroutine(System.Collections.IEnumerator routine)
    {
        return BepInEx.Unity.IL2CPP.Utils.MonoBehaviourExtensions.StartCoroutine(BIE_ArchiveMod.MainComponent, routine);
    }

    public static void StopCoroutine(object coroutineToken)
    {
        BIE_ArchiveMod.MainComponent.StopCoroutine((UnityEngine.Coroutine)coroutineToken);
    }

    public static unsafe void* GetIl2CppMethod<T>(string methodName, string returnTypeName, bool isGeneric, params string[] argTypes) where T : Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {
        void** ppMethod = (void**)Il2CppInterop.Runtime.IL2CPP.GetIl2CppMethod(Il2CppInterop.Runtime.Il2CppClassPointerStore<T>.NativeClassPtr, isGeneric, methodName, returnTypeName, argTypes).ToPointer();
        if ((long)ppMethod == 0) return ppMethod;

        return *ppMethod;
    }

    public static unsafe TDelegate GetIl2CppMethod<T, TDelegate>(string methodName, string returnTypeName, bool isGeneric, params string[] argTypes)
        where T : Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
        where TDelegate : Delegate
    {
        void* pMethod = GetIl2CppMethod<T>(methodName, returnTypeName, isGeneric, argTypes);
        if ((long)pMethod == 0) return null;

        return Marshal.GetDelegateForFunctionPointer<TDelegate>((IntPtr)pMethod);
    }

    public static unsafe BepInEx.Unity.IL2CPP.Hook.INativeDetour ApplyNativeHook<TClass, TDelegate>(string methodName, string returnType, string[] paramTypes, TDelegate to, out TDelegate original)
        where TClass : Il2CppSystem.Object
        where TDelegate : Delegate
    {
        IntPtr classPtr = Il2CppInterop.Runtime.Il2CppClassPointerStore<TClass>.NativeClassPtr;
        if (classPtr == IntPtr.Zero) throw new ArgumentException($"{typeof(TClass).Name} does not exist in il2cpp domain");
        IntPtr methodPtr = Il2CppInterop.Runtime.IL2CPP.GetIl2CppMethod(classPtr, false, methodName, returnType, paramTypes);

        Il2CppSystem.Reflection.MethodInfo methodInfo = new(Il2CppInterop.Runtime.IL2CPP.il2cpp_method_get_object(methodPtr, classPtr));

        Il2CppInterop.Runtime.Runtime.VersionSpecific.MethodInfo.INativeMethodInfoStruct il2cppMethodInfo = Il2CppInterop.Runtime.Runtime.UnityVersionHandler.Wrap((Il2CppInterop.Runtime.Runtime.Il2CppMethodInfo*)Il2CppInterop.Runtime.IL2CPP.il2cpp_method_get_from_reflection(methodInfo.Pointer));

        return BepInEx.Unity.IL2CPP.Hook.INativeDetour.CreateAndApply(il2cppMethodInfo.MethodPointer, to, out original);
    }

    public static unsafe BepInEx.Unity.IL2CPP.Hook.INativeDetour ApplyNativeHook<TClass, TDelegate>(string methodName, string returnType, string[] paramTypes, Type[] genericArguments, TDelegate to, out TDelegate original)
        where TClass : Il2CppSystem.Object
        where TDelegate : Delegate
    {
        IntPtr classPtr = Il2CppInterop.Runtime.Il2CppClassPointerStore<TClass>.NativeClassPtr;
        if (classPtr == IntPtr.Zero) throw new ArgumentException($"{typeof(TClass).Name} does not exist in il2cpp domain");
        IntPtr methodPtr = Il2CppInterop.Runtime.IL2CPP.GetIl2CppMethod(classPtr, true, methodName, returnType, paramTypes);

        Il2CppSystem.Reflection.MethodInfo methodInfo = new(Il2CppInterop.Runtime.IL2CPP.il2cpp_method_get_object(methodPtr, classPtr));
        Il2CppSystem.Reflection.MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(genericArguments.Select(Il2CppInterop.Runtime.Il2CppType.From).ToArray());

        Il2CppInterop.Runtime.Runtime.VersionSpecific.MethodInfo.INativeMethodInfoStruct il2cppMethodInfo = Il2CppInterop.Runtime.Runtime.UnityVersionHandler.Wrap((Il2CppInterop.Runtime.Runtime.Il2CppMethodInfo*)Il2CppInterop.Runtime.IL2CPP.il2cpp_method_get_from_reflection(methodInfo.Pointer));

        return BepInEx.Unity.IL2CPP.Hook.INativeDetour.CreateAndApply(il2cppMethodInfo.MethodPointer, to, out original);
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