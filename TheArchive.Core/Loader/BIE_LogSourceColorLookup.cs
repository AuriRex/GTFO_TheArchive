#if BepInEx
using BepInEx.Logging;
using System;
using System.Collections.Generic;

namespace TheArchive.Loader;

public static class BIE_LogSourceColorLookup
{
    private static readonly Dictionary<ManualLogSource, ConsoleColor> _colorLookup = new();

    public static void SetColor(ManualLogSource logger, ConsoleColor color)
    {
        _colorLookup[logger] = color;
    }

    public static bool GetColor(ManualLogSource logger, out ConsoleColor color, bool clearColor = true)
    {
        var res = _colorLookup.TryGetValue(logger, out color);

        if (res && clearColor)
        {
            _colorLookup.Remove(logger);
        }
        
        return res;
    }
}
#endif