// This file is licensed under the LGPL 2.1 LICENSE
// See LICENSE_BepInEx in the projects root folder
// Original code from https://github.com/BepInEx/BepInEx

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using TheArchive.Core.Attributes;

namespace TheArchive.Core.Bootstrap;

/// <summary>
///     Data class that represents information about a loadable Archive module.
///     Contains all metadata and additional info required for module loading by <see cref="ArchiveModuleChainloader" />.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ModuleInfo : ICacheable
{
    /// <summary>
    ///     General metadata about a module.
    /// </summary>
    public ArchiveModule Metadata { get; internal set; }

    /// <summary>
    ///     Collection of <see cref="ArchiveDependency" /> attributes that describe what modules this module depends on.
    /// </summary>
    public IEnumerable<ArchiveDependency> Dependencies { get; internal set; }

    /// <summary>
    ///     Collection of <see cref="ArchiveIncompatibility" /> attributes that describe what modules this module
    ///     is incompatible with.
    /// </summary>
    public IEnumerable<ArchiveIncompatibility> Incompatibilities { get; internal set; }

    /// <summary>
    ///     File path to the module DLL
    /// </summary>
    public string Location { get; internal set; }

    /// <summary>
    ///     Instance of the module that represents this info. NULL if no module is instantiated from info (yet)
    /// </summary>
    public object Instance { get; internal set; }

    /// <summary>
    /// Type Name
    /// </summary>
    public string TypeName { get; internal set; }

    internal Version TargetedTheArchiveVersion { get; set; }

    /// <summary>
    /// Save method for caching.
    /// </summary>
    /// <param name="bw">The BinaryWriter to write to.</param>
    public void Save(BinaryWriter bw)
    {
        bw.Write(TypeName);
        bw.Write(Location);
        
        bw.Write(Metadata.GUID);
        bw.Write(Metadata.Name);
        bw.Write(Metadata.Version.ToString());
        
        var depList = Dependencies.ToList();
        bw.Write(depList.Count);
        foreach (var bepInDependency in depList)
        {
            ((ICacheable)bepInDependency).Save(bw);
        }
        
        var incList = Incompatibilities.ToList();
        bw.Write(incList.Count);
        foreach (var bepInIncompatibility in incList)
        {
            ((ICacheable)bepInIncompatibility).Save(bw);
        }
        
        bw.Write(TargetedTheArchiveVersion.ToString(4));
    }

    /// <summary>
    /// Load method for caching.
    /// </summary>
    /// <param name="br">The BinaryReader to read from.</param>
    public void Load(BinaryReader br)
    {
        TypeName = br.ReadString();
        Location = br.ReadString();
        
        Metadata = new ArchiveModule(br.ReadString(), br.ReadString(), br.ReadString());
        
        var depCount = br.ReadInt32();
        var depList = new List<ArchiveDependency>(depCount);
        for (var j = 0; j < depCount; j++)
        {
            var dep = new ArchiveDependency(string.Empty);
            ((ICacheable)dep).Load(br);
            depList.Add(dep);
        }
        
        Dependencies = depList;
        
        var incCount = br.ReadInt32();
        var incList = new List<ArchiveIncompatibility>(incCount);
        for (var k = 0; k < incCount; k++)
        {
            var inc = new ArchiveIncompatibility(string.Empty);
            ((ICacheable)inc).Load(br);
            incList.Add(inc);
        }
        
        Incompatibilities = incList;
        
        TargetedTheArchiveVersion = new Version(br.ReadString());
    }

    /// <inheritdoc />
    public override string ToString() => $"{Metadata?.Name} {Metadata?.Version}";
}
