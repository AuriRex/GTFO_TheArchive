using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.Models;

public struct GameBuildInfo
{
    public RundownID Rundown { get; set; }
    public int BuildNumber { get; set; }
}