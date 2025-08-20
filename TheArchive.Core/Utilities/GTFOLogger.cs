using System.Collections.Generic;
using System.Linq;
using TheArchive.Interfaces;

namespace TheArchive.Utilities;

/// <summary>
/// Filtered Unity/GTFO game log logger.
/// </summary>
internal static class GTFOLogger
{
    internal static IArchiveLogger Logger { private get; set; }

    private static readonly HashSet<string> _ignoreListExact = new HashSet<string>() {
        "show crosshair",
        "Setting and getting Body Position/Rotation, IK Goals, Lookat and BoneLocalRotation should only be done in OnAnimatorIK or OnStateIK",
    };

    private static readonly HashSet<string> _ignoreListStartsWith = new HashSet<string>() {
        "Wielding new item in slot",
        "Backend.", // Just constant console spam in Alt://R4
        "BE.OnGameEvent", // Same but Alt://R5
    };

    public static void Ignore(string str) => _ignoreListExact.Add(str);

    public static void Log(string message)
    {
        if (_ignoreListExact.Contains(message)) return;
        if (_ignoreListStartsWith.Any(s => message.StartsWith(s))) return;
        Logger.Info(message);
    }

    public static void Warn(string message)
    {
        if (_ignoreListExact.Contains(message)) return;
        if (_ignoreListStartsWith.Any(s => message.StartsWith(s))) return;
        Logger.Warning(message);
    }

    public static void Error(string message)
    {
        if (_ignoreListExact.Contains(message)) return;
        if (_ignoreListStartsWith.Any(s => message.StartsWith(s))) return;
        Logger.Error(message);
    }
}