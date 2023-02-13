using System;

namespace TheArchive.Interfaces.Loader
{
    public interface IClassInjector
    {
        public IntPtr DerivedConstructorPointer<T>();

#if IL2CPP && Il2CppInterop
        public void DerivedConstructorBody(Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase objectBase);
#endif
#if MONO
        public void DerivedConstructorBody(object objectBase);
#endif

        public void RegisterTypeInIl2Cpp<T>(bool logSuccess = false) where T : class;

        public void RegisterTypeInIl2CppWithInterfaces<T>(bool logSuccess = false, params Type[] interfaces) where T : class;
    }
}
