using System;

namespace TheArchive.Utilities
{
    public class ArchiveLogger
    {
        public static void Success(string msg)
        {
            MelonLoader.MelonLogger.Msg(ConsoleColor.Green, msg);
        }

        public static void Notice(string msg)
        {
            MelonLoader.MelonLogger.Msg(ConsoleColor.Cyan, msg);
        }

        public static void Msg(ConsoleColor col, string msg)
        {
            MelonLoader.MelonLogger.Msg(col, msg);
        }

        public static void Info(string msg)
        {
            MelonLoader.MelonLogger.Msg(msg);
        }

        public static void Warning(string msg)
        {
            MelonLoader.MelonLogger.Msg(ConsoleColor.DarkYellow, msg);
        }

        public static void Error(string msg)
        {
            MelonLoader.MelonLogger.Error(msg);
        }

        public static void Msg(string v) => Info(v);
    }
}
