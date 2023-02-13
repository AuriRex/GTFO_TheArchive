using System.Runtime.CompilerServices;

namespace TheArchive.Loader
{
    internal static class CoreModEvents
    {
        private static bool _hasBeenSetup = false;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Setup()
        {
            if (_hasBeenSetup)
                return;

            LoaderCore.LoadWrapperASM();

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