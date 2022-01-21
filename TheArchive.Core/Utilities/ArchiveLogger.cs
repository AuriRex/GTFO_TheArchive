using System;

namespace TheArchive.Utilities
{
    public class ArchiveLogger
    {
        private static MelonLoader.MelonLogger.Instance _logger = null;
        private static MelonLoader.MelonLogger.Instance Logger
        {
            get
            {
                if(_logger == null)
                {
                    _logger = ArchiveMod.Instance.LoggerInstance;
                }
                return _logger;
            }
        }

        public static bool LogDebug { get; set; }
#if DEBUG
            = true;
#else
            = false;
#endif


        public static void Success(string msg)
        {
            Logger.Msg(ConsoleColor.Green, msg);
        }

        public static void Notice(string msg)
        {
            Logger.Msg(ConsoleColor.Cyan, msg);
        }

        public static void Msg(ConsoleColor col, string msg)
        {
            Logger.Msg(col, msg);
        }

        public static void Debug(string msg)
        {
            if (!LogDebug) return;
            Logger.Msg(ConsoleColor.Gray, msg);
        }

        public static void Info(string msg)
        {
            Logger.Msg(msg);
        }

        public static void Warning(string msg)
        {
            Logger.Msg(ConsoleColor.DarkYellow, msg);
        }

        public static void Error(string msg)
        {
            Logger.Error(msg);
        }

        public static void Msg(string v) => Info(v);
        public static void Exception(Exception ex)
        {
            ArchiveLogger.Error($"{ex}: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
