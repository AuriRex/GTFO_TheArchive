using System;
using UnityEngine;

namespace TheArchive.Interfaces.Loader
{
    public interface ILoaderWrapper
    {
        public string GameDirectory { get; }

        public string UserDataDirectory { get; }

        //public bool IsIL2CPPType(Type type);

        /// <summary>
        /// Path where the .net version specific mod files are stored.
        /// </summary>
        public string BaseInstallPath { get; }

        public bool IsGameIL2CPP();

        public IArchiveLogger CreateLoggerInstance(string name, ConsoleColor col = ConsoleColor.White);

        public IArchiveLogger WrapLogger(object loggerInstance);

        public Coroutine StartCoroutine(System.Collections.IEnumerator routine);

        public void StopCoroutine(Coroutine coroutineToken);

        [Obsolete]
        public void NativeHookAttach(IntPtr target, IntPtr detour);

        public bool IsModInstalled(string modName);
    }
}
