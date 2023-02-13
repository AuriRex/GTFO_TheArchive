using System;
using System.Linq;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Loader
{
    public partial class BIE_LoaderWrapper
    {
#if BepInEx
        public static string GameDirectory => CoreModLoader.GameRootDirectory;

        public static string UserDataDirectory => System.IO.Path.Combine(GameDirectory, "UserData");


        public static bool IsGameIL2CPP()
#if IL2CPP
            => true;
#else
            => false;
#endif

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
#endif
    }
}
