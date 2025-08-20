#if BepInEx
using System;
using BepInEx.Logging;

namespace TheArchive.Loader;

/// <summary>
/// Extended BepInEx ManualLogSource that changes the console color on print.
/// </summary>
public class ArManualLogSource : ManualLogSource
{
    /// <summary>
    /// The color to use for the message prefix.
    /// </summary>
    public ConsoleColor? Color { get; }
    
    /// <summary>
    /// Creates an Archive manual log source.
    /// </summary>
    /// <param name="sourceName">Name of the log source.</param>
    /// <param name="col">The color to use for the message prefix.</param>
    public ArManualLogSource(string sourceName, ConsoleColor? col = null) : base(sourceName)
    {
        Color = col;
    }
}
#endif