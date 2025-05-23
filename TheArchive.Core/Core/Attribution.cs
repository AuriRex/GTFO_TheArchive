using System.Collections.Generic;

namespace TheArchive.Core;

public static class Attribution
{
    internal static HashSet<AttributionInfo> AttributionInfos { get; } = new();
    
    public static void Add(AttributionInfo attribution)
    {
        AttributionInfos.Add(attribution);
    }

    public record class AttributionInfo(
        string Name,
        string Content,
        string Comment = "",
        string Origin = "");
}