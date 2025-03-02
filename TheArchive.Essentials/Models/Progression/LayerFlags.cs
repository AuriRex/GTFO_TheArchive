using System;

namespace TheArchive.Models.Progression;

[Flags]
public enum LayerFlags
{
    None = 0,
    Main = 1 << 0,
    Secondary = 1 << 1,
    Third = 1 << 2,
    All = Main | Secondary | Third,
}