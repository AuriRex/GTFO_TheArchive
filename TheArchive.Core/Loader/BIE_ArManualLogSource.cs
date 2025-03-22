#if BepInEx
using System;
using BepInEx.Logging;

namespace TheArchive.Loader;

public class ArManualLogSource : ManualLogSource
{
    public ConsoleColor? Color { get; }
    
    public ArManualLogSource(string sourceName, ConsoleColor? col = null) : base(sourceName)
    {
        Color = col;
    }
}
#endif