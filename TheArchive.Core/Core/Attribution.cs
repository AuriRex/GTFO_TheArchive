using System.Collections.Generic;

namespace TheArchive.Core;

/// <summary>
/// Attribution manager for the mod settings attributions page
/// </summary>
public static class Attribution
{
    internal static HashSet<AttributionInfo> AttributionInfos { get; } = new();
    
    /// <summary>
    /// Adds your <c>AttributionInfo</c>
    /// </summary>
    /// <param name="attribution">Attribution</param>
    public static void Add(AttributionInfo attribution)
    {
        AttributionInfos.Add(attribution);
    }

    /// <summary>
    /// Attribution info for the mod settings attributions page
    /// </summary>
    /// <param name="Name">Display Name</param>
    /// <param name="Content">Content, usually a license text</param>
    /// <param name="Comment">A comment at the bottom of the panel</param>
    /// <param name="Origin">The origin, usually your mod/module name</param>
    public record class AttributionInfo(
        string Name,
        string Content,
        string Comment = "",
        string Origin = "");
}