using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheArchive.Core.Bootstrap;
using TheArchive.Utilities;
using Range = SemanticVersioning.Range;

namespace TheArchive.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ArchiveDependency : Attribute, ICacheable
{
    public string DependencyGUID { get; protected set; }
    public DependencyFlags Flags { get; protected set; }
    public Range VersionRange { get; protected set; }

    public ArchiveDependency(string DependencyGUID, DependencyFlags Flags = DependencyFlags.HardDependency)
    {
        this.DependencyGUID = DependencyGUID;
        this.Flags = Flags;
        VersionRange = null;
    }
    public ArchiveDependency(string guid, string version)
            : this(guid, DependencyFlags.HardDependency)
    {
        VersionRange = Range.Parse(version, false);
    }

    [Flags]
    public enum DependencyFlags
    {
        HardDependency = 1,
        SoftDependency = 2
    }

    public void Save(BinaryWriter bw)
    {
        bw.Write(DependencyGUID);
        bw.Write((int)Flags);
        Range versionRange = VersionRange;
        bw.Write(((versionRange != null) ? versionRange.ToString() : null) ?? string.Empty);
    }

    public void Load(BinaryReader br)
    {
        DependencyGUID = br.ReadString();
        Flags = (DependencyFlags)br.ReadInt32();
        string versionRange = br.ReadString();
        VersionRange = (versionRange == string.Empty) ? null : Range.Parse(versionRange, false);
    }

    internal static IEnumerable<ArchiveDependency> FromCecilType(TypeDefinition td)
    {
        return MetadataHelper.GetCustomAttributes<ArchiveDependency>(td, true).Select(delegate (CustomAttribute customAttribute)
        {
            string dependencyGuid = (string)customAttribute.ConstructorArguments[0].Value;
            object secondArg = customAttribute.ConstructorArguments[1].Value;
            string minVersion = secondArg as string;
            if (minVersion != null)
            {
                return new ArchiveDependency(dependencyGuid, minVersion);
            }
            return new ArchiveDependency(dependencyGuid, (DependencyFlags)secondArg);
        }).ToList();
    }
}
