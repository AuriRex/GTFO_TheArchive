using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.Models;

/// <summary>
/// The currently running game version
/// </summary>
/// <seealso cref="RundownID"/>
/// <seealso cref="BuildDB"/>
public struct GameBuildInfo
{
    /// <summary>
    /// The currently running game version (<c>RundownID</c>)
    /// </summary>
    public RundownID Rundown { get; init; }
    
    /// <summary>
    /// The currently running game build version number.
    /// </summary>
    public int BuildNumber { get; init; }
}