using System;
using System.Diagnostics;
using TheArchive.Interfaces;

namespace TheArchive.Utilities;

internal static class ArchiveLogger
{
    internal static IArchiveLogger Logger = null;

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

    //[Conditional("DEBUG")]
    public static void Debug(string msg)
    {
        Logger.Msg(ConsoleColor.DarkGray, msg);
    }

    public static void Info(string msg)
    {
        Logger.Info(msg);
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
        Error($"{ex}: {ex.Message}\n{ex.StackTrace}");
    }
}