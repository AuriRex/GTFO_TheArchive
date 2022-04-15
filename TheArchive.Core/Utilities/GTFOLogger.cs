using MelonLoader;
using System;

namespace TheArchive.Utilities
{
    public static class GTFOLogger
    {

        internal static MelonLogger.Instance Logger { private get; set; }

        public const string GTFO_LOG = "GTFO_Log: ";
        public const string GTFO_WARN = "GTFO_Warn: ";
        public const string GTFO_ERROR = "GTFO_Error: ";

        public static void Log(string message) => Logger.Msg(message);

        public static void Warn(string message) => Logger.Warning(message);
        public static void Error(string message) => Logger.Error(message);

    }
}
