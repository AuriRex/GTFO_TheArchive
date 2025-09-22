using System;
using System.Linq;

namespace TheArchive.Core.Attributes.Feature.Patches;

[AttributeUsage(AttributeTargets.Method)]
public class ArchiveAfter : Attribute
{
    public string[] After { get; internal set; }

    public ArchiveAfter(params string[] harmonyIds)
    {
        After = harmonyIds?.Distinct().ToArray();
    }
}

public class ArchiveAfterFeature : ArchiveAfter
{
    public ArchiveAfterFeature(params string[] identifiers)
    {
        After = identifiers?.Select(s => string.Format($"{ArchiveMod.MOD_NAME}_FeaturesAPI_{s}")).Distinct().ToArray();
    }
}