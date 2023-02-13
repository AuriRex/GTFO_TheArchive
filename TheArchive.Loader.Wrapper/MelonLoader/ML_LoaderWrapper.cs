#if MelonLoader
using MelonLoader;
using MelonLoader.Utils;
using System;
using System.Linq;
using TheArchive.Interfaces;
using TheArchive.Interfaces.Loader;
using UnityEngine;

namespace TheArchive.Loader
{
    internal class ML_LoaderWrapper : ILoaderWrapper
    {
        public string GameDirectory => CoreModLoader.GameRootDirectory;

        public string UserDataDirectory => MelonEnvironment.UserDataDirectory;

        public string BaseInstallPath => CoreModLoader.BaseInstallPath;

        public bool IsGameIL2CPP() => MelonUtils.IsGameIl2Cpp();

        public IArchiveLogger CreateLoggerInstance(string name, ConsoleColor col = ConsoleColor.White)
        {
            return new ML_LogWrapper(new MelonLogger.Instance(name, col));
        }

        public IArchiveLogger WrapLogger(object loggerInstance)
        {
            return new ML_LogWrapper((MelonLogger.Instance)loggerInstance);
        }

        public Coroutine StartCoroutine(System.Collections.IEnumerator routine)
        {
            return (Coroutine)MelonCoroutines.Start(routine);
        }

        public void StopCoroutine(Coroutine coroutineToken)
        {
            MelonCoroutines.Stop(coroutineToken);
        }

        public void NativeHookAttach(IntPtr target, IntPtr detour)
        {
            MelonUtils.NativeHookAttach(target, detour);
        }

        public bool IsModInstalled(string modName)
        {
            return MelonMod.RegisteredMelons.Any(m => m.Info.Name == modName);
        }
    }
}
#endif
