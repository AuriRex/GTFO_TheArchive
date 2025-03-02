using System;
using System.Diagnostics;
using TheArchive.Interfaces;

namespace TheArchive.Utilities;

public class ArchiveLogger
{
    internal static IArchiveLogger logger = null;

    public static void Success(string msg)
    {
        logger.Msg(ConsoleColor.Green, msg);
    }

    public static void Notice(string msg)
    {
        logger.Msg(ConsoleColor.Cyan, msg);
    }

    public static void Msg(ConsoleColor col, string msg)
    {
        logger.Msg(col, msg);
    }

    //[Conditional("DEBUG")]
    public static void Debug(string msg)
    {
        logger.Msg(ConsoleColor.DarkGray, msg);
    }

    public static void Info(string msg)
    {
        logger.Info(msg);
    }

    public static void Warning(string msg)
    {
        logger.Msg(ConsoleColor.DarkYellow, msg);
    }

    public static void Error(string msg)
    {
        logger.Error(msg);
    }

    public static void Msg(string v) => Info(v);
    public static void Exception(Exception ex)
    {
        ArchiveLogger.Error($"{ex}: {ex.Message}\n{ex.StackTrace}");
    }
}