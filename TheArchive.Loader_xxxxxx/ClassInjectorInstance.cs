using System;
using TheArchive.Interfaces.Loader;

namespace TheArchive.Loader
{
    public class ClassInjectorInstance : IClassInjector
    {
#if IL2CPP && Il2CppInterop
        public IntPtr DerivedConstructorPointer<T>()
        {
            return Il2CppInterop.Runtime.Injection.ClassInjector.DerivedConstructorPointer<T>();
        }

        public void DerivedConstructorBody(Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase objectBase)
        {
            Il2CppInterop.Runtime.Injection.ClassInjector.DerivedConstructorBody(objectBase);
        }

        public void RegisterTypeInIl2Cpp<T>(bool logSuccess = false) where T : class
        {
            Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<T>(new Il2CppInterop.Runtime.Injection.RegisterTypeOptions
            {
                LogSuccess = logSuccess
            });
        }

        public void RegisterTypeInIl2CppWithInterfaces<T>(bool logSuccess = false, params Type[] interfaces) where T : class
        {
            Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<T>(new Il2CppInterop.Runtime.Injection.RegisterTypeOptions
            {
                Interfaces = interfaces,
                LogSuccess = logSuccess
            });
        }
#endif
#if MONO
        public IntPtr DerivedConstructorPointer<T>() => IntPtr.Zero;

        public void DerivedConstructorBody(object objectBase) { }

        public void RegisterTypeInIl2Cpp<T>(bool logSuccess = false) where T : class { }

        public void RegisterTypeInIl2CppWithInterfaces<T>(bool logSuccess = false, params Type[] interfaces) where T : class { }
#endif
    }
}
