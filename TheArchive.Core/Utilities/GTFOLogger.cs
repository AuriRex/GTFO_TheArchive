using MelonLoader;
using System;

namespace TheArchive.Utilities
{
    public static class GTFOLogger
    {

        public const string GTFO_LOG = "GTFO_Log: ";
        public const string GTFO_WARN = "GTFO_Warn: ";
        public const string GTFO_ERROR = "GTFO_Error: ";

        public static void Log(string message) => ArchiveLogger.Info(GTFO_LOG + message);

        public static void Warn(string message) => ArchiveLogger.Warning(GTFO_WARN + message);
        public static void Error(string message) => ArchiveLogger.Error( GTFO_ERROR + message);

    }
}
