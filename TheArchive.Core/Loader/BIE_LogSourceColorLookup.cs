#if BepInEx
using BepInEx.Logging;
using System;
using System.Collections.Generic;

namespace TheArchive.Loader;

/// <summary>
/// Color lookup for the custom colored loggers.
/// </summary>
public static class BIE_LogSourceColorLookup
{
    private static readonly Dictionary<ManualLogSource, ConsoleColor> _colorLookup = new();

    /// <summary>
    /// Set the next messages color for a given logger.
    /// </summary>
    /// <param name="logger">Logger to set the next color for.</param>
    /// <param name="color">The color to use.</param>
    public static void SetColor(ManualLogSource logger, ConsoleColor color)
    {
        _colorLookup[logger] = color;
    }

    /// <summary>
    /// Tries to get the message color for a given logger instance.
    /// </summary>
    /// <param name="logger">The logger to check for colors.</param>
    /// <param name="color">The color to use.</param>
    /// <param name="clearColor">Clear the set color after returning it.</param>
    /// <returns><c>True</c> if the given logger has a color set.</returns>
    public static bool TryGetColor(ManualLogSource logger, out ConsoleColor color, bool clearColor = true)
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