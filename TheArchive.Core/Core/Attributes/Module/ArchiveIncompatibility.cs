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

namespace TheArchive.Core.Attributes;

/// <summary>
///     This attribute specifies other modules that are incompatible with this module.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class ArchiveIncompatibility : Attribute, ICacheable
{
    /// <summary>
    ///     Marks this <see cref="IArchiveModule" /> as incompatible with another module.
    ///     If the other module exists, this module will not be loaded and a warning will be shown.
    /// </summary>
    /// <param name="IncompatibilityGUID">The GUID of the referenced module.</param>
    public ArchiveIncompatibility(string IncompatibilityGUID)
    {
        this.IncompatibilityGUID = IncompatibilityGUID;
    }

    /// <summary>
    ///     The GUID of the referenced module.
    /// </summary>
    public string IncompatibilityGUID { get; protected set; }

    internal static IEnumerable<ArchiveIncompatibility> FromCecilType(TypeDefinition td)
    {
        var attrs = MetadataHelper.GetCustomAttributes<ArchiveIncompatibility>(td, true);
        return attrs.Select(customAttribute =>
        {
            var dependencyGuid = (string) customAttribute.ConstructorArguments[0].Value;
            return new ArchiveIncompatibility(dependencyGuid);
        }).ToList();
    }

    /// <summary>
    /// Save method for caching.
    /// </summary>
    /// <param name="bw">The BinaryWriter to write to.</param>
    public void Save(BinaryWriter bw)
    {
        bw.Write(IncompatibilityGUID);
    }

    /// <summary>
    /// Load method for caching.
    /// </summary>
    /// <param name="br">The BinaryReader to read from.</param>
    public void Load(BinaryReader br)
    {
        IncompatibilityGUID = br.ReadString();
    }
}
