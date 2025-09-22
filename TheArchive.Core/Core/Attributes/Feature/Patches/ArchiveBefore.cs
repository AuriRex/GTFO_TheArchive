using System;
using System.Linq;

namespace TheArchive.Core.Attributes.Feature.Patches;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ArchiveBefore : Attribute
{
    public string[] Before { get; internal set; }

    public ArchiveBefore(params string[] harmonyIds)
    {
        Before = harmonyIds?.Distinct().ToArray();
    }
}

public class ArchiveBeforeFeature : ArchiveBefore
{
    public ArchiveBeforeFeature(params string[] identifiers)
    {
        Before = identifiers?.Select(s => string.Format($"{ArchiveMod.MOD_NAME}_FeaturesAPI_{s}")).Distinct().ToArray();
    }
}
