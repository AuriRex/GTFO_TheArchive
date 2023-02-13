using System.Runtime.CompilerServices;
using TheArchive.Interfaces.Loader;

namespace TheArchive.Loader
{
    internal static class LoaderCore
    {
        private static object _loaderWrapper;
        private static object _classInjector;

        private static bool _hasBeenSetup = false;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Setup()
        {
            if (_hasBeenSetup)
                return;

#if MelonLoader
            _loaderWrapper = new ML_LoaderWrapper();
#endif
#if BepInEx
            _loaderWrapper = new BIE_LoaderWeapper();
#endif

            _classInjector = new ClassInjectorInstance();

            LoaderWrapper.LoaderWrapperInstance = (ILoaderWrapper) _loaderWrapper;
            LoaderWrapper.ClassInjector.ClassInjectorInstance = (IClassInjector) _classInjector;

            _hasBeenSetup = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void OnApplicationStart(object loggerInstance, HarmonyLib.Harmony harmony)
        {
            Setup();

            ArchiveMod.OnApplicationStart(LoaderWrapper.WrapLogger(loggerInstance), harmony);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void OnApplicationQuit()
        {
            ArchiveMod.OnApplicationQuit();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void OnUpdate()
        {
            ArchiveMod.OnUpdate();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void OnLateUpdate()
        {
            ArchiveMod.OnLateUpdate();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            ArchiveMod.OnSceneWasLoaded(buildIndex, sceneName);
        }
    }
}