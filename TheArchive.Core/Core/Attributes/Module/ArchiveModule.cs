// This file is licensed under the LGPL 2.1 LICENSE
// See LICENSE_BepInEx in the projects root folder
// Original code from https://github.com/BepInEx/BepInEx

using Mono.Cecil;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using TheArchive.Utilities;
using Version = SemanticVersioning.Version;

namespace TheArchive.Core.Attributes;

/// <summary>
///     This attribute denotes that a class is a module, and specifies the required metadata.
/// </summary>
#pragma warning disable CS0436
[AttributeUsage(AttributeTargets.Class)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[MeansImplicitUse(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
public class ArchiveModule : Attribute
{
    /// <summary>
    ///     The unique identifier of the module. Should not change between module versions.
    /// </summary>
    public string GUID { get; protected set; }
    
    /// <summary>
    ///     The user friendly name of the module. Is able to be changed between versions.
    /// </summary>
    public string Name { get; protected set; }
    
    /// <summary>
    ///     The specific version of the module.
    /// </summary>
    public Version Version { get; protected set; }

    /// <param name="GUID">The unique identifier of the module. Should not change between module versions.</param>
    /// <param name="Name">The user friendly name of the module. Is able to be changed between versions.</param>
    /// <param name="Version">The specific version of the module.</param>
    public ArchiveModule(string GUID, string Name, string Version)
    {
        this.GUID = GUID;
        this.Name = Name;
        this.Version = TryParseLongVersion(Version);
    }

    private static Version TryParseLongVersion(string version)
    {
        if (Version.TryParse(version, out var v))
            return v;
        
        try
        {
            var longVersion = new System.Version(version);
            
            return new Version(longVersion.Major, longVersion.Minor, longVersion.Build != -1 ? longVersion.Build : 0);
        }
        catch
        {
            // ignored
        }

        return null;
    }

    internal static ArchiveModule FromCecilType(TypeDefinition td)
    {
        var attr = MetadataHelper.GetCustomAttributes<ArchiveModule>(td, false).FirstOrDefault();
        
        if (attr == null)
            return null;
        
        return new ArchiveModule((string) attr.ConstructorArguments[0].Value,
                                (string) attr.ConstructorArguments[1].Value, 
                                (string) attr.ConstructorArguments[2].Value);
    }
}