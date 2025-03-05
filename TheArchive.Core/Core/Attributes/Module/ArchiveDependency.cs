// This file is licensed under the LGPL 2.1 LICENSE
// See LICENSE_BepInEx in the projects root folder
// Original code from https://github.com/BepInEx/BepInEx

using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using TheArchive.Core.Bootstrap;
using TheArchive.Utilities;
using Range = SemanticVersioning.Range;

namespace TheArchive.Core.Attributes;

/// <summary>
///     This attribute specifies any dependencies that this module has on other modules.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class ArchiveDependency : Attribute, ICacheable
{
    /// <summary>
    ///     Flags that are applied to a dependency
    /// </summary>
    [Flags]
    public enum DependencyFlags
    {
        /// <summary>
        ///     The module has a hard dependency on the referenced module, and will not run without it.
        /// </summary>
        HardDependency = 1,
        
        /// <summary>
        ///     This module has a soft dependency on the referenced module, and is able to run without it.
        /// </summary>
        SoftDependency = 2
    }
    
    /// <summary>
    ///     The GUID of the referenced module.
    /// </summary>
    public string DependencyGUID { get; protected set; }
    
    /// <summary>
    ///     The flags associated with this dependency definition.
    /// </summary>
    public DependencyFlags Flags { get; protected set; }
    
    /// <summary>
    ///     The version <see cref="SemanticVersioning.Range">range</see> of the referenced module.
    /// </summary>
    public Range VersionRange { get; protected set; }

    /// <summary>
    ///     Marks this <see cref="IArchiveModule" /> as dependent on another module. The other module will be loaded before
    ///     this one.
    ///     If the other module doesn't exist, what happens depends on the <see cref="Flags" /> parameter.
    /// </summary>
    /// <param name="DependencyGUID">The GUID of the referenced module.</param>
    /// <param name="Flags">The flags associated with this dependency definition.</param>
    public ArchiveDependency(string DependencyGUID, DependencyFlags Flags = DependencyFlags.HardDependency)
    {
        this.DependencyGUID = DependencyGUID;
        this.Flags = Flags;
        VersionRange = null;
    }
    
    /// <summary>
    ///     Marks this <see cref="IArchiveModule" /> as dependent on another module. The other module will be loaded before
    ///     this one.
    ///     If the other module doesn't exist or is of a version not satisfying <see cref="VersionRange" />, this module will
    ///     not load and an error will be logged instead.
    /// </summary>
    /// <param name="guid">The GUID of the referenced module.</param>
    /// <param name="version">The version range of the referenced module.</param>
    /// <remarks>When version is supplied the dependency is always treated as HardDependency</remarks>
    public ArchiveDependency(string guid, string version) : this(guid)
    {
        VersionRange = Range.Parse(version);
    }

    public void Save(BinaryWriter bw)
    {
        bw.Write(DependencyGUID);
        bw.Write((int)Flags);
        bw.Write(VersionRange?.ToString() ?? string.Empty);
    }

    public void Load(BinaryReader br)
    {
        DependencyGUID = br.ReadString();
        Flags = (DependencyFlags)br.ReadInt32();
        
        var versionRange = br.ReadString();
        VersionRange = versionRange == string.Empty ? null : Range.Parse(versionRange);
    }

    internal static IEnumerable<ArchiveDependency> FromCecilType(TypeDefinition td)
    {
        var attrs = MetadataHelper.GetCustomAttributes<ArchiveDependency>(td, true);
        return attrs.Select(customAttribute =>
        {
            var dependencyGuid = (string)customAttribute.ConstructorArguments[0].Value;
            var secondArg = customAttribute.ConstructorArguments[1].Value;
            if (secondArg is string minVersion) return new ArchiveDependency(dependencyGuid, minVersion);
            return new ArchiveDependency(dependencyGuid, (DependencyFlags)secondArg);
        }).ToList();
    }
}
