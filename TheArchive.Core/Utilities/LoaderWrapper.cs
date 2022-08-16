using MelonLoader;
using System;
using TheArchive.Interfaces;

namespace TheArchive.Utilities
{
    public class LoaderWrapper
    {

        public static string GameDirectory
        {
            get
            {
                return MelonUtils.GameDirectory;
            }
        }

        public static string UserDataDirectory
        {
            get
            {
                return MelonUtils.UserDataDirectory;
            }
        }

        public static bool IsGameIL2CPP()
        {
            return MelonUtils.IsGameIl2Cpp();
        }

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

        
    }
}
